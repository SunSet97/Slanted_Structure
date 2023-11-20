using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Utility.UI;
using Utility.Utils;

namespace Utility.Core
{
    public class DataController : MonoBehaviour
    {
        public static DataController Instance { get; private set; }

#pragma warning disable 0649
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
#pragma warning restore 0649
        
        [NonSerialized] public Camera Cam;
        [NonSerialized] public MapData CurrentMap;
        [NonSerialized] public List<InteractionObject> InteractionObjects;

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

        public void GameStart(SaveData save)
        {
            MobileAdsManager.ADCount++;
            Debug.Log("게임 시작");
            Init();
            ChangeMap(save.saveCoverData.mapCode, save);
        }
        
        public void GameStart(string mapCode = "001010", int desStep = -1)
        {
            MobileAdsManager.ADCount++;
            Debug.Log("게임 시작");
            Init();
            ChangeMap(mapCode, null, desStep);
        }
        
        private MapData LoadMap(string desMapCode, int desStep = -1)
        {
            var mapCode = CurrentMap ? CurrentMap.mapCode : "";
            Debug.Log($"{mapCode} -> {desMapCode}");
            var curEp = CurrentMap ? int.Parse(mapCode.Substring(0, 1)) : -1;
            var desEp = int.Parse(desMapCode.Substring(0, 1));

            var curStep = CurrentMap ? CurrentMap.step : -1;

            if (AssetBundleMap.Contains($"map/ep{curEp}/step{curStep}"))
            {
                var curMapDB = AssetBundleMap.GetAssetBundle($"map/ep{curEp}/step{curStep}");
                var assetNames = curMapDB.GetAllAssetNames();

                // if did not Loaded (destination Map is Next Step or Next Episode)
                if (!assetNames.Any(item => item.Contains(desMapCode)))
                {
                    if (desStep == -1)
                    {
                        // if Next Step
                        if (curEp == desEp)
                        {
                            desStep = curStep + 1;
                        }
                        // if Next Ep
                        else
                        {
                            desStep = 0;
                        }
                    }

                    // LoadAssetWithSubAsset 찾아볼 필요 있음
                    AssetBundleMap.RemoveAssetBundle($"dialogue/ep{curEp}/step{curStep}");
                    AssetBundleMap.AddAssetBundle($"dialogue/ep{desEp}/step{desStep}",
                        $"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}/step{desStep}");

                    AssetBundleMap.RemoveAssetBundle($"map/ep{curEp}/step{curStep}");
                    AssetBundleMap.AddAssetBundle($"map/ep{desEp}/step{desStep}",
                        $"{Application.dataPath}/AssetBundles/map/ep{desEp}/step{desStep}");
                }
                else
                {
                    if (desStep == -1)
                    {
                        desStep = curStep;
                    }
                }
            }
            else
            {
                AssetBundleMap.AddAssetBundle($"dialogue/ep{desEp}/step{desStep}",
                    $"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}/step{desStep}");
                AssetBundleMap.AddAssetBundle($"map/ep{desEp}/step{desStep}",
                    $"{Application.dataPath}/AssetBundles/map/ep{desEp}/step{desStep}");
            }
            
            var mapDB = AssetBundleMap.GetAssetBundle($"map/ep{desEp}/step{desStep}");
            
            var mapDBNames = mapDB.GetAllAssetNames();
            if (mapDBNames.Any(item => Path.GetFileNameWithoutExtension(item).Equals(desMapCode)))
            {
                var mapDataObject = mapDB.LoadAsset<GameObject>($"{desMapCode}").GetComponent<MapData>();

                return mapDataObject;
            }
            else
            {
                var mapDataTable = mapDB.LoadAsset<MapDataTable>("MapData Table");
                var mapDataProps = Array.Find(mapDataTable.mapData,
                    dataProps => { return dataProps.mapCode.Any(codeProps => codeProps.mapCode == desMapCode); });
                
                var mapDataObject = mapDB.LoadAsset<GameObject>(mapDataProps.name).GetComponent<MapData>();
                Debug.Log($"{mapDataObject.mapCode} ->{mapDataObject}");
                
                var mapCodeProps = Array.Find(mapDataProps.mapCode, item => item.mapCode == desMapCode);
                mapDataObject.mapCode = mapCodeProps.mapCode;
                mapDataObject.date = mapCodeProps.date;
                mapDataObject.time = mapCodeProps.time;
                mapDataObject.nextMapcode = mapCodeProps.nextMapCode;
                for (var index = 0; index < mapDataObject.clearBoxList.Count; index++)
                {
                    var checkMapClear = mapDataObject.clearBoxList[index];
                    checkMapClear.nextSelectMapCode = mapCodeProps.clearBoxNextMapCode[index];
                }

                return mapDataObject;
            }
        }

        public CharacterManager GetCharacter(CharacterType characterTypeType)
        {
            if (characterTypeType == CharacterType.Main)
            {
                return mainChar;
            }

            return Array.Find(characters, item => item.who == characterTypeType);
        }

        public CharacterManager[] GetFollowCharacters(List<MapData.CharacterPositionSet> positionSets)
        {
            return positionSets.Where(item => item.isFollow).Select(positionSet => GetCharacter(positionSet.who)).ToArray();
        }

        public void ChangeMap(string desMapCode, SaveData saveData = null, int desStep = -1)
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

            ObjectClicker.Instance.Clear();
            InteractionObjects.Clear();

            if (saveData != null)
            {
                desStep = saveData.saveCoverData.step;
            }
            
            var nextMap = LoadMap(desMapCode, desStep);

            CurrentMap = Instantiate(nextMap, mapGenerate);

            SetByChangedMap(saveData);
            CurrentMap.Init();

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
            JoystickController.Instance.SetJoystickArea(CustomEnum.JoystickAreaType.Default);

            RenderSettings.skybox = CurrentMap.skyboxSetting;
            DynamicGI.UpdateEnvironment();
            
            PlayUIController.Instance.SetMenuActive(!CurrentMap.isMenuInactive);
            
            var cameraMoving = Cam.GetComponent<CameraMoving>();
            cameraMoving.Initialize(CurrentMap.cameraViewType, GetCharacter(CharacterType.Main)?.transform);

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