using System;
using System.Collections.Generic;
using CommonScript;
using Data;
using UnityEngine;
using UnityEngine.Events;
using Utility.Interaction;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        private static DataController _instance;

        public static DataController instance => _instance;

        [Header("캐릭터")] private CharacterManager mainChar;
        [SerializeField] private CharacterManager rau;
        [SerializeField] private CharacterManager oun;
        [SerializeField] private CharacterManager speat;
        [SerializeField] private CharacterManager speat_Adult;
        [SerializeField] private CharacterManager speat_Child;
        [SerializeField] private CharacterManager speat_Adolescene;

        [NonSerialized] public Camera cam;
        [Header("카메라 경계값")] public CamInfo camInfo;

        /// <summary>
        /// 디버깅용
        /// </summary>
        public float camOrthgraphicSize;

        [Header("맵")] [NonSerialized] public MapData[] storymaps;
        public Transform mapGenerate;
        [NonSerialized] public MapData currentMap;
        public string mapCode;

        public CharData charData;

        private AssetBundle _mapDB;
        private AssetBundle _materialDB;
        internal AssetBundle dialogueDB;

        private UnityAction onLoadMap;
        
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
            
            cam = Camera.main;
            speat.Init();
            oun.Init();
            rau.Init();
            speat_Adolescene.Init();
            speat_Adult.Init();
            speat_Child.Init();
        }

        public void GameStart(string mapcode)
        {
            Debug.Log("게임 시작");
            Init();
            ChangeMap(mapcode);
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
                return storymaps;
            }

            if (_mapDB != null)
            {
                _mapDB.Unload(true);
            }

            if (dialogueDB != null)
            {
                dialogueDB.Unload(true);
            }

            if (_materialDB != null)
            {
                _materialDB.Unload(true);
            }

            Debug.Log(Application.dataPath + $"/AssetBundles/map/ep{desEp}/day{desDay}");

            _mapDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/map/ep{desEp}/day{desDay}");
            if (curDay != desDay)
            {
                dialogueDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}");
            }

            _materialDB = AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/modulematerials");

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
                    character = mainChar;
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

        #region 맵 이동

        /// <summary>
        /// 맵 바꾸는 함수
        /// </summary>
        /// <param name="desMapCode">생성되는 맵의 코드</param>
        public void ChangeMap(string desMapCode)
        {
            //모든 캐릭터 위치 대기실로 이동
            speat.WaitInRoom();
            oun.WaitInRoom();
            rau.WaitInRoom();
            speat_Child.WaitInRoom();
            speat_Adolescene.WaitInRoom();
            speat_Adult.WaitInRoom();

            if (currentMap != null)
            {
                currentMap.gameObject.SetActive(false);
                currentMap.DestroyMap();
            }

            storymaps = LoadMap(desMapCode);

            ObjectClicker.instance.gameObject.SetActive(false);
            ObjectClicker.instance.isCustomUse = false;

            var nextMap = Array.Find(storymaps, mapData => mapData.mapCode.Equals(mapCode));
            Debug.Log(mapCode);
            Debug.Log(nextMap);
            InteractionObjects.Clear();
            currentMap = Instantiate(nextMap, mapGenerate);
            currentMap.Initialize();
            SetByChangedMap();
            DialogueController.instance.Initialize();

            Debug.Log("OnChangeMap");
            if (FadeEffect.instance.isFadeOut)
            {
                FadeEffect.instance.FadeIn();
                FadeEffect.instance.onFadeOver.AddListener(() =>
                {
                    onLoadMap?.Invoke();
                    onLoadMap = () => { };
                });
            }
            else
            {
                onLoadMap?.Invoke();
                onLoadMap = () => { };
            }
        }

        public void AddOnLoadMap(UnityAction onLoadMap)
        {
            this.onLoadMap += onLoadMap;
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
            camInfo.camDis = currentMap.camDis;
            camInfo.camRot = currentMap.camRot;

            // 해당되는 캐릭터 초기화
            speat.InitializeCharacter();
            oun.InitializeCharacter();
            rau.InitializeCharacter();
            speat_Adolescene.InitializeCharacter();
            speat_Adult.InitializeCharacter();
            speat_Child.InitializeCharacter();

            var temp = currentMap.positionSets;
            for (int k = 0; k < temp.Count; k++)
            {
                if (temp[k].who.Equals(Character.Speat))
                {
                    if (temp[k].isMain)
                        mainChar = speat;
                    speat.SetCharacter(temp[k].startPosition);
                    speat.Emotion = Expression.IDLE;
                }
                else if (temp[k].who.Equals(Character.Oun))
                {
                    if (temp[k].isMain)
                        mainChar = oun;
                    oun.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Rau))
                {
                    if (temp[k].isMain)
                        mainChar = rau;
                    rau.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Adolescene))
                {
                    if (temp[k].isMain)
                        mainChar = speat_Adolescene;
                    speat_Adolescene.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Adult))
                {
                    if (temp[k].isMain)
                        mainChar = speat_Adult;
                    speat_Adult.SetCharacter(temp[k].startPosition);
                }
                else if (temp[k].who.Equals(Character.Speat_Child))
                {
                    if (temp[k].isMain)
                        mainChar = speat_Child;
                    speat_Child.SetCharacter(temp[k].startPosition);
                }
            }

            if (mainChar)
            {
                mainChar.gameObject.layer = LayerMask.NameToLayer("Player");
            }

            //조이스틱 초기화
            JoystickController.instance.Init(currentMap.joystickType);
            JoystickController.instance.InitializeJoyStick(!currentMap.isJoystickNone);


            // CameraMoving 컨트롤
            var cameraMoving = cam.GetComponent<CameraMoving>();

            cameraMoving.Initialize();


            //스카이박스 세팅
            RenderSettings.skybox = currentMap.skyboxSetting;
            DynamicGI.UpdateEnvironment();
            //현재 맵에서 카메라 세팅

            //카메라 orthographic 컨트롤
            cam.orthographic = currentMap.isOrthographic;
            if (currentMap.isOrthographic)
            {
                camOrthgraphicSize = currentMap.orthographicSize;
            }

            // 조작 가능한 상태로 변경 (중력 적용)
            speat.UseJoystickCharacter();
            oun.UseJoystickCharacter();
            rau.UseJoystickCharacter();
            speat_Adolescene.UseJoystickCharacter();
            speat_Child.UseJoystickCharacter();
            speat_Adult.UseJoystickCharacter();
        }

        #endregion

        public void UpdateLikeable(int[] rauLikeables)
        {
            charData.selfEstm += rauLikeables[0];
            charData.intimacySpRau += rauLikeables[1];
            charData.intimacyOunRau += rauLikeables[2];
        }

        public int[] GetLikeable()
        {
            int[] likable = {charData.selfEstm, charData.intimacySpRau, charData.intimacyOunRau};
            return likable;
        }
    }
}