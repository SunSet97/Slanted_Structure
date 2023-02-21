using System;
using System.Collections.Generic;
using CommonScript;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility.Interaction;
using Utility.Interaction.Click;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        private static DataController _instance;

        public static DataController Instance => _instance;

        [Header("캐릭터")]
        [SerializeField] private CharacterManager[] characters;
        
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
            for (var index = 0; index < characters.Length; index++)
            {
                characters[index] = GameObject.Find(characters[index].who.ToString()).GetComponent<CharacterManager>();
                characters[index].Init();
            }

            mapGenerate = GameObject.Find("MapGenerate").transform;

            InteractionObjects = new List<InteractionObject>();

            AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/modulematerials");

            Cam = Camera.main;
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
            if (characterType == Character.Main)
            {
                return _mainChar;
            }
            
            return Array.Find(characters, item => item.who == characterType);
        }
        
        public CharacterManager[] GetFollowCharacters()
        {
            List<CharacterManager> characterManagers = new List<CharacterManager>();
            foreach (var positionSet in CurrentMap.positionSets)
            {
                if (positionSet.isFollow)
                {
                    characterManagers.Add(GetCharacter(positionSet.who));
                }
            }

            return characterManagers.ToArray();
        }

        public void ChangeMap(string desMapCode, SaveData saveData = null)
        {
            Debug.Log("Change Map");
            
            foreach (var character in characters)
            {
                character.WaitInRoom();
            }

            if (CurrentMap != null)
            {
                CurrentMap.gameObject.SetActive(false);
                CurrentMap.DestroyMap();
            }

            Storymaps = LoadMap(desMapCode);

            ObjectClicker.Instance.Reset();

            var nextMap = Array.Find(Storymaps, mapData => mapData.mapCode.Equals(mapCode));
            
            InteractionObjects.Clear();
            
            CurrentMap = Instantiate(nextMap, mapGenerate);
            
            SetByChangedMap();
            
            CurrentMap.Initialize();
            
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
            foreach (var character in characters)
            {
                character.PickUpCharacter();
                character.InitializeCharacter();
            }

            var mainPosSet = CurrentMap.positionSets.Find(item => item.isMain);
            _mainChar = Array.Find(characters, item => item.who == mainPosSet.who);
            
            foreach (var posSet in CurrentMap.positionSets)
            {
                var character = Array.Find(characters, item => item.who == posSet.who);
                character.SetCharacter(posSet.startPosition);
            }
            
            JoystickController.instance.Init(CurrentMap.joystickType);
            JoystickController.instance.InitializeJoyStick(!CurrentMap.isJoystickNone);

            camInfo.camDis = CurrentMap.camDis;
            camInfo.camRot = CurrentMap.camRot;

            var cameraMoving = Cam.GetComponent<CameraMoving>();
            cameraMoving.Initialize();

            RenderSettings.skybox = CurrentMap.skyboxSetting;
            DynamicGI.UpdateEnvironment();

            Cam.orthographic = CurrentMap.isOrthographic;
            if (Cam.orthographic)
            {
                camOrthgraphicSize = CurrentMap.orthographicSize;
            }

            foreach (var character in characters)
            {
                character.UseJoystickCharacter();
            }
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