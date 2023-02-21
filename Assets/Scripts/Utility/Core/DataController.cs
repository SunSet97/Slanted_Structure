using System;
using System.Collections.Generic;
using CommonScript;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility.Interaction;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        private static DataController _instance;

        public static DataController Instance => _instance;

        [Header("캐릭터")]
        [SerializeField] private CharacterManager rau;
        [SerializeField] private CharacterManager oun;
        [SerializeField] private CharacterManager speat;
        [SerializeField] private CharacterManager speat_Adult;
        [SerializeField] private CharacterManager speat_Child;
        [SerializeField] private CharacterManager speat_Adolescene;
        
        [Header("카메라 경계값")] public CamInfo camInfo;

        /// <summary>
        /// 디버깅용
        /// </summary>
        public float camOrthgraphicSize;

        [Header("맵")]
        public Transform mapGenerate;
        public string mapCode;

        [FormerlySerializedAs("charData")] public CharRelationshipData charRelationshipData;
        
        [NonSerialized] public Camera Cam;
        [NonSerialized] public MapData[] Storymaps;
        [NonSerialized] public MapData CurrentMap;
        
        private CharacterManager _mainChar;

        private AssetBundle _mapDB;
        internal AssetBundle DialogueDB;

        private UnityAction _onLoadMap;
        
        [NonSerialized] public List<InteractionObject> InteractionObjects;

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(_instance);
            }
        }

        private void Init()
        {
            speat = GameObject.Find("Speat").GetComponent<CharacterManager>();
            oun = GameObject.Find("Oun").GetComponent<CharacterManager>();
            rau = GameObject.Find("Rau").GetComponent<CharacterManager>();
            speat_Adolescene = GameObject.Find("Speat_Adolescene").GetComponent<CharacterManager>();
            speat_Adult = GameObject.Find("Speat_Adult").GetComponent<CharacterManager>();
            speat_Child = GameObject.Find("Speat_Child").GetComponent<CharacterManager>();

            mapGenerate = GameObject.Find("MapGenerate").transform;

            InteractionObjects = new List<InteractionObject>();

            AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/modulematerials");

            Cam = Camera.main;
            
            speat.Init();
            oun.Init();
            rau.Init();
            speat_Adolescene.Init();
            speat_Adult.Init();
            speat_Child.Init();
        }

        public void GameStart(string mapCode = "001010", SaveData save = null)
        {
            Debug.Log("게임 시작");
            Init();
            ChangeMap(mapCode, save);
        }
        

        private MapData[] LoadMap(string desMapCode)
        {
            Debug.Log("현재 맵: " + mapCode);
            Debug.Log("다음 맵: " + desMapCode);
            var curEp = int.Parse(mapCode.Substring(0, 1));
            var desEp = int.Parse(desMapCode.Substring(0, 1));

            var curDay = int.Parse(mapCode.Substring(1, 2));
            var desDay = int.Parse(desMapCode.Substring(1, 2));

            mapCode = $"{desMapCode:000000}"; // 맵 코드 변경

            Debug.Log("Episode:  " + curEp + " : " + desEp);
            Debug.Log("Day:   " + curDay + " : " + desDay);
            if (curEp == desEp && curDay == desDay)
            {
                return Storymaps;
            }
            
            Debug.Log(Application.dataPath + $"/AssetBundles/map/ep{desEp}/day{desDay}");

            if (_mapDB != null)
            {
                _mapDB.Unload(true);
                Debug.Log(_mapDB);
                _mapDB = null;
            }

            _mapDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/map/ep{desEp}/day{desDay}");
            if (curDay != desDay)
            {
                if (DialogueDB != null)
                {
                    DialogueDB.Unload(true);
                    Debug.Log(DialogueDB);
                    DialogueDB = null;
                }
                DialogueDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}");
            }

            if (_mapDB == null)
            {
                Debug.LogError("맵 어셋 번들 오류");
            }

            var mapDataObjects = _mapDB.LoadAllAssets<GameObject>();
            MapData[] mapDatas = new MapData[mapDataObjects.Length];
            for (int i = 0; i < mapDataObjects.Length; i++)
            {
                mapDatas[i] = mapDataObjects[i].GetComponent<MapData>();
            }

            return mapDatas;
        }

        public CharacterManager GetCharacter(Character characterType)
        {
            CharacterManager character = null;
            switch (characterType)
            {
                case Character.Main:
                    character = _mainChar;
                    break;
                case Character.Rau:
                    character = rau;
                    break;
                case Character.Oun:
                    character = oun;
                    break;
                case Character.Speat:
                    character = speat;
                    break;
                case Character.Speat_Adult:
                    character = speat_Adult;
                    break;
                case Character.Speat_Child:
                    character = speat_Child;
                    break;
                case Character.Speat_Adolescene:
                    character = speat_Adolescene;
                    break;
            }

            return character;
        }

        public void ChangeMap(string desMapCode, SaveData saveData = null)
        {
            Debug.Log("Change Map");
            
            speat.WaitInRoom();
            oun.WaitInRoom();
            rau.WaitInRoom();
            speat_Child.WaitInRoom();
            speat_Adolescene.WaitInRoom();
            speat_Adult.WaitInRoom();

            if (CurrentMap != null)
            {
                CurrentMap.gameObject.SetActive(false);
                CurrentMap.DestroyMap();
            }

            Storymaps = LoadMap(desMapCode);

            ObjectClicker.instance.gameObject.SetActive(false);
            ObjectClicker.instance.isCustomUse = false;

            var nextMap = Array.Find(Storymaps, mapData => mapData.mapCode.Equals(mapCode));
            Debug.Log(mapCode);
            Debug.Log(nextMap);
            InteractionObjects.Clear();
            CurrentMap = Instantiate(nextMap, mapGenerate);
            CurrentMap.Initialize();
            
            SetByChangedMap();
            
            LoadData(saveData);
            
            DialogueController.instance.Initialize();

            if (FadeEffect.Instance.IsAlreadyFadeOut)
            {
                FadeEffect.Instance.OnFadeOver.AddListener(() =>
                {
                    FadeEffect.Instance.OnFadeOver.RemoveAllListeners();
                    Debug.Log("??");
                    _onLoadMap?.Invoke();
                    _onLoadMap = () => { };
                });

                FadeEffect.Instance.FadeIn(CurrentMap.fadeInSec);
            }
            else
            {
                _onLoadMap?.Invoke();
                _onLoadMap = () => { };
            }
        }

        public void AddOnLoadMap(UnityAction onLoadMap)
        {
            _onLoadMap += onLoadMap;
        }

        public void AddInteractor(InteractionObject interactionObject)
        {
            InteractionObjects.Add(interactionObject);
        }

        private void SetByChangedMap()
        {
            speat.PickUpCharacter();
            oun.PickUpCharacter();
            rau.PickUpCharacter();
            speat_Adolescene.PickUpCharacter();
            speat_Adult.PickUpCharacter();
            speat_Child.PickUpCharacter();

            //카메라 위치와 회전
            camInfo.camDis = CurrentMap.camDis;
            camInfo.camRot = CurrentMap.camRot;

            // 해당되는 캐릭터 초기화
            speat.InitializeCharacter();
            oun.InitializeCharacter();
            rau.InitializeCharacter();
            speat_Adolescene.InitializeCharacter();
            speat_Adult.InitializeCharacter();
            speat_Child.InitializeCharacter();

            var temp = CurrentMap.positionSets;
            for (int k = 0; k < temp.Count; k++)
            {
                if (temp[k].who.Equals(Character.Speat))
                {
                    if (temp[k].isMain)
                        _mainChar = speat;
                    speat.SetCharacter(temp[k].startPosition);
                    speat.Emotion = Expression.IDLE;
                }
                else if (temp[k].who.Equals(Character.Oun))
                {
                    if (temp[k].isMain)
                        _mainChar = oun;
                    oun.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Rau))
                {
                    if (temp[k].isMain)
                        _mainChar = rau;
                    rau.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Adolescene))
                {
                    if (temp[k].isMain)
                        _mainChar = speat_Adolescene;
                    speat_Adolescene.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Adult))
                {
                    if (temp[k].isMain)
                        _mainChar = speat_Adult;
                    speat_Adult.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Child))
                {
                    if (temp[k].isMain)
                        _mainChar = speat_Child;
                    speat_Child.SetCharacter(temp[k].startPosition);
                }
            }

            if (_mainChar)
            {
                _mainChar.gameObject.layer = LayerMask.NameToLayer("Player");
            }

            //조이스틱 초기화
            JoystickController.instance.Init(CurrentMap.joystickType);
            JoystickController.instance.InitializeJoyStick(!CurrentMap.isJoystickNone);


            // CameraMoving 컨트롤
            var cameraMoving = Cam.GetComponent<CameraMoving>();

            cameraMoving.Initialize();


            //스카이박스 세팅
            RenderSettings.skybox = CurrentMap.skyboxSetting;
            DynamicGI.UpdateEnvironment();
            //현재 맵에서 카메라 세팅

            //카메라 orthographic 컨트롤
            Cam.orthographic = CurrentMap.isOrthographic;
            if (CurrentMap.isOrthographic)
            {
                camOrthgraphicSize = CurrentMap.orthographicSize;
            }

            // 조작 가능한 상태로 변경 (중력 적용)
            speat.UseJoystickCharacter();
            oun.UseJoystickCharacter();
            rau.UseJoystickCharacter();
            speat_Adolescene.UseJoystickCharacter();
            speat_Child.UseJoystickCharacter();
            speat_Adult.UseJoystickCharacter();
        }
        
        public void UpdateLikeable(int[] rauLikeables)
        {
            charRelationshipData.selfEstm += rauLikeables[0];
            charRelationshipData.intimacySpRau += rauLikeables[1];
            charRelationshipData.intimacyOunRau += rauLikeables[2];
        }

        public int[] GetLikeable()
        {
            int[] likable = {charRelationshipData.selfEstm, charRelationshipData.intimacySpRau, charRelationshipData.intimacyOunRau};
            return likable;
        }

        private void LoadData(SaveData saveData)
        {
            Debug.Log($"Load 시도 {mapCode} {saveData?.saveCoverData.mapCode}");
            if (saveData != null && mapCode == saveData.saveCoverData.mapCode)
            {
                Debug.Log($"Load {mapCode}");
                Debug.Log($"{InteractionObjects.Count}   {saveData.interactionDatas.Count}");
                Debug.Log($"{CurrentMap.positionSets.Count}   {saveData.charDatas.Count}");
                foreach (var positionSet in CurrentMap.positionSets)
                {
                    var character = GetCharacter(positionSet.who);
                    var charData = saveData.charDatas.Find(item => item.character == character.who);
                    character.transform.position = charData.pos;
                    character.transform.rotation = charData.rot;
                }
                
                charRelationshipData = saveData.charRelationshipData;
                foreach (var interactionObject in InteractionObjects)
                {
                    var interactionSaveData = saveData.interactionDatas.Find(item => item.id == interactionObject.id);

                    interactionObject.Load(interactionSaveData);
                }
            }
        }
    }
}