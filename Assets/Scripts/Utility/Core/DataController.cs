﻿using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using Utility.Interaction;
using Utility.Interaction.Click;
using Utility.Utils;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        public static DataController Instance { get; private set; }

        [Header("캐릭터")] [SerializeField] private CharacterManager[] characters;

        [Header("맵")] public Transform mapGenerate;

        [Header("Camera")] public float defaultFieldOfView;
        [FormerlySerializedAs("defaultfarClipPlane")] [FormerlySerializedAs("defaultFarDistance")] public float defaultFarClipPlane;

        [Header("Dialogue")] public float dialoguePrintSec = .1f;

        [Header("For Debug")] public CamInfo camInfo;
        public CamInfo camOffsetInfo;
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
            Debug.Log("현재 맵: " + mapCode);
            Debug.Log("다음 맵: " + desMapCode);
            var curEp = int.Parse(mapCode.Substring(0, 1));
            var desEp = int.Parse(desMapCode.Substring(0, 1));

            var curDay = int.Parse(mapCode.Substring(1, 2));
            var desDay = int.Parse(desMapCode.Substring(1, 2));

            Debug.Log($"Cur ep: {curEp}, day: {curDay}");
            Debug.Log($"Des ep: {desEp}, day: {desDay}");
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

        public CharacterManager GetCharacter(Character characterType)
        {
            if (characterType == Character.Main)
            {
                return mainChar;
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
            Debug.Log($"Change Map -> {desMapCode}");

            if (MobileAdsManager.ADCount >= MobileAdsManager.CountPerAds)
            {
                MobileAdsManager.ShowAd();
                MobileAdsManager.ADCount = 0;
            }

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

            SetByChangedMap(saveData);

            CurrentMap.Initialize();

            LoadData(saveData);

            if (FadeEffect.Instance.IsAlreadyFadeOut)
            {
                FadeEffect.Instance.OnFadeOver.AddListener(() =>
                {
                    FadeEffect.Instance.OnFadeOver.RemoveAllListeners();
                    Debug.Log("??");
                    onLoadMap?.Invoke();
                    onLoadMap = () => { };
                });

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

            camInfo.camDis = CurrentMap.camDis;
            camInfo.camRot = CurrentMap.camRot;

            foreach (var posSet in CurrentMap.positionSets)
            {
                var character = Array.Find(characters, item => item.who == posSet.who);
                if (saveData != null)
                {
                    var charData = saveData.charDatas.Find(item => item.character == character.who);
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
            cameraMoving.Initialize(CurrentMap.cameraViewType, GetCharacter(Character.Main)?.transform);

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

        public int[] GetLikeable()
        {
            int[] likable =
            {
                charRelationshipData.selfEstm, charRelationshipData.intimacySpRau, charRelationshipData.intimacyOunRau
            };
            return likable;
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

                    interactionObject.Load(interactionSaveData);
                }
            }
        }
    }
}