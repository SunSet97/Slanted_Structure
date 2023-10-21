using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using Utility.Character;
using Utility.Dialogue;
using Utility.Interaction;
using Utility.Interaction.Click;
using Utility.Map;
using Utility.Save;
using Utility.Utils;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        public static DataController Instance { get; private set; }

        [Header("캐릭터")] [SerializeField] private CharacterManager[] characters;
        public float jumpForce;
        public float gravityScale;
            
        [Header("맵")] public Transform mapGenerate;

        [Header("Camera")] public float defaultFieldOfView;
        public float defaultFarClipPlane;

        [Header("Dialogue")] public float dialoguePrintSec = .1f;
        public float dialogueNextSec = .05f;

        /// <summary>
        /// Camera Offset, whenever work, affect by all
        /// </summary>
        [Header("For Debug")] public CamInfo camOffsetInfo;
        
        public float camOrthographicSize;
        public CharRelationshipData charRelationshipData;

        [NonSerialized] public Camera Cam;
        [NonSerialized] public MapData CurrentMap;
        [NonSerialized] public List<InteractionObject> InteractionObjects;

        private MapData[] storyMaps;
        private CharacterManager mainChar;

        private UnityAction onLoadMap;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Init()
        {
            foreach (var character in characters)
            {
                character.Init();
            }

            mapGenerate = GameObject.Find("MapGenerate").transform;

            InteractionObjects = new List<InteractionObject>();

            AssetBundleMap.AddAssetBundle("module", $"{Application.dataPath}/AssetBundles/modulematerials");

            Cam = Camera.main;
            camOffsetInfo = new CamInfo();
        }

        public void GameStart(string mapCode = "001010", SaveData save = null)
        {
            MobileAdsManager.ADCount++;
            Debug.Log("게임 시작");
            Init();
            ChangeMap(mapCode, save);
        }


        private MapData[] LoadMap(string desMapCode)
        {
            var mapCode = CurrentMap ? CurrentMap.mapCode : "999999";
            Debug.Log($"{mapCode} -> {desMapCode}");
            var curEp = int.Parse(mapCode.Substring(0, 1));
            var desEp = int.Parse(desMapCode.Substring(0, 1));

            var curDay = int.Parse(mapCode.Substring(1, 2));
            var desDay = int.Parse(desMapCode.Substring(1, 2));

            // Debug.Log($"Cur ep: {curEp}, day: {curDay}");
            // Debug.Log($"Des ep: {desEp}, day: {desDay}");
            if (curEp == desEp && curDay == desDay)
            {
                Debug.Log("이미 있음");
                return storyMaps;
            }

            AssetBundleMap.RemoveAssetBundle($"ep{curEp}/day{curDay}");
            AssetBundleMap.AddAssetBundle($"ep{desEp}/day{desDay}",
                $"{Application.dataPath}/AssetBundles/map/ep{desEp}/day{desDay}");

            if (curEp != desEp)
            {
                AssetBundleMap.RemoveAssetBundle($"ep{curEp}");
                AssetBundleMap.AddAssetBundle($"ep{desEp}", $"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}");
            }

            var mapDB = AssetBundleMap.GetAssetBundle($"ep{desEp}/day{desDay}");
            var mapDataObjects = mapDB.LoadAllAssets<GameObject>();
            var mapData = new MapData[mapDataObjects.Length];
            for (var i = 0; i < mapDataObjects.Length; i++)
            {
                mapData[i] = mapDataObjects[i].GetComponent<MapData>();
                Debug.Log(mapData[i].mapCode);
            }

            return mapData;
        }

        public CharacterManager GetCharacter(Character.CharacterType characterTypeType)
        {
            if (characterTypeType == Character.CharacterType.Main)
            {
                return mainChar;
            }

            return Array.Find(characters, item => item.who == characterTypeType);
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
            Debug.Log($"Change Map {(CurrentMap != null ? CurrentMap.mapCode : "")} -> {desMapCode}");

            if (MobileAdsManager.ADCount >= MobileAdsManager.CountPerAds)
            {
                MobileAdsManager.ShowAd();
                MobileAdsManager.ADCount = 0;
            }
            
            DialogueController.Instance.EndConversation();

            foreach (var character in characters)
            {
                character.WaitInRoom();
            }

            if (CurrentMap != null)
            {
                CurrentMap.gameObject.SetActive(false);
                CurrentMap.DestroyMap();
            }

            storyMaps = LoadMap(desMapCode);

            ObjectClicker.Instance.Reset();

            var nextMap = Array.Find(storyMaps, mapData => mapData.mapCode.Equals(desMapCode));
            InteractionObjects.Clear();

            CurrentMap = Instantiate(nextMap, mapGenerate);
            Debug.Log($"Instantiate -> {nextMap.mapCode}");

            SetByChangedMap(saveData);

            CurrentMap.Initialize();

            LoadData(saveData);

            if (FadeEffect.Instance.IsAlreadyFadeOut)
            {
                FadeEffect.Instance.OnFadeOver += () =>
                {
                    Debug.Log("FadeOver -> LoadMap");
                    onLoadMap?.Invoke();
                    onLoadMap = () => { };
                };

                FadeEffect.Instance.FadeIn(CurrentMap.fadeInSec);
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

        private void SetByChangedMap(SaveData saveData)
        {
            foreach (var character in characters)
            {
                character.PickUpCharacter();
                character.InitializeCharacter();
            }

            var mainPosSetIndex = CurrentMap.positionSets.FindIndex(item => item.isMain);
            if (mainPosSetIndex != -1)
            {
                mainChar = Array.Find(characters, item => item.who == CurrentMap.positionSets[mainPosSetIndex].who);
            }
            else
            {
                mainChar = null;
            }

            camOffsetInfo.camDis = CurrentMap.camDis;
            camOffsetInfo.camRot = CurrentMap.camRot;

            foreach (var posSet in CurrentMap.positionSets)
            {
                var character = Array.Find(characters, item => item.who == posSet.who);
                if (saveData != null)
                {
                    var charData = saveData.charDatas.Find(item => item.characterType == character.who);
                    character.SetCharacter(posSet, charData);
                }
                else
                {
                    character.SetCharacter(posSet);
                }
            }

            JoystickController.Instance.Initialize(CurrentMap.joystickType);
            JoystickController.Instance.SetJoyStickState(!CurrentMap.isJoystickNone);

            RenderSettings.skybox = CurrentMap.skyboxSetting;
            DynamicGI.UpdateEnvironment();
            
            var cameraMoving = Cam.GetComponent<CameraMoving>();
            cameraMoving.Initialize(CurrentMap.cameraViewType, GetCharacter(Character.CharacterType.Main)?.transform);

            Cam.farClipPlane = CurrentMap.useClippingPlanes ? CurrentMap.farClipPlane : defaultFarClipPlane;
            Cam.fieldOfView = CurrentMap.useFieldOfView ? CurrentMap.fieldOfView : defaultFieldOfView;
            Cam.GetComponent<PostProcessLayer>().enabled = CurrentMap.usePostProcessing;
            Cam.orthographic = CurrentMap.isOrthographic;
            
            if (Cam.orthographic)
            {
                camOrthographicSize = CurrentMap.orthographicSize;
            }

            foreach (var character in characters)
            {
                character.PutDownCharacter();
            }
        }

        public void UpdateLikeable(int[] rauLikeables)
        {
            charRelationshipData.selfEstm += rauLikeables[0];
            charRelationshipData.intimacySpRau += rauLikeables[1];
            charRelationshipData.intimacyOunRau += rauLikeables[2];
        }

        public int[] GetRelationshipData()
        {
            int[] relationshipData =
            {
                charRelationshipData.selfEstm, charRelationshipData.intimacySpRau, charRelationshipData.intimacyOunRau
            };
            return relationshipData;
        }

        private void LoadData(SaveData saveData)
        {
            var mapCode = CurrentMap.mapCode;
            Debug.Log($"Load 시도 {mapCode}");
            if (saveData != null)
            {
                Debug.Log($"Load {mapCode}");
                Debug.Log($"Interaction 개수: {InteractionObjects.Count}, Save 개수: {saveData.interactionDatas.Count}");
                Debug.Log($"PosSet 개수: {CurrentMap.positionSets.Count}, Save 개수: {saveData.charDatas.Count}");

                charRelationshipData = saveData.charRelationshipData;
                foreach (var interactionObject in InteractionObjects)
                {
                    var interactionSaveData = saveData.interactionDatas.Find(item => item.id == interactionObject.id);

                    interactionObject.LoadData(interactionSaveData);
                }
            }
        }
    }
}