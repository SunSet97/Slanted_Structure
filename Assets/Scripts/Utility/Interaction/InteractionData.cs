using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using Utility.Character;
using Utility.Core;
using Utility.Dialogue;
using Utility.Game;
using Utility.Utils.Property;
using Utility.Utils.Serialize;

namespace Utility.Interaction
{
    public enum TaskContentType
    {
        None = 0,
        Animation = 1,
        Dialogue = 2,
        Choice = 3,
        DialogueElement = 4,
        TaskReset = 5,
        New = 6,
        TheEnd = 7,
        Play = 8,
        FadeIn = 9,
        FadeOut = 10,
        Timeline = 11,
        ImmediateChoice = 12,
        ClearMap = 13,
        ChoiceDialogue = 99
    }
        
    public enum OutlineColor
    {
        Red,
        Magenta,
        Yellow,
        Green,
        Blue,
        Grey,
        Black,
        White
    }

    public enum InteractionPlayType
    {
        None = 0,
        Portal = 1,
        Animation = 2,
        Dialogue = 4,
        Task = 8,
        Game = 16,
        Cinematic = 32,
        // FadeOut
    }

    public enum InteractionMethod
    {
        Touch,
        Trigger,
        No,
        OnChangeMap
    }
    
    [Serializable]
    public class WaitInteractionData
    {
        public WaitInteraction[] waitInteractions;
    }
    
    [Serializable]
    public struct WaitInteraction
    {
        public InteractionObject waitInteraction;
        public int interactionIndex;
    }
    
    [Serializable]
    public class SerializedInteractionData
    {
        public int id;
        public bool isInteractable = true;

        [NonSerialized] public Stack<TaskData> JsonTask;

        [Header("디버깅용")] [SerializeField] internal bool isInteracted;
    }

    [Serializable]
    public struct InteractionSaveData
    {
        public string id;
        
        public SerializableVector3 pos;

        public SerializableQuaternion rot;

        public List<SerializedInteractionData> serializedInteractionData;
        
        public int interactIndex;
    }
    
    [Serializable]
    public class InteractionData
    {
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Task | InteractionPlayType.Dialogue, false, true)]
        public TextAsset jsonFile;

        [Header("인터렉션 방식")] public InteractionPlayType interactionPlayType;

        [FormerlySerializedAs("game")] [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public MiniGame miniGame;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public float dialoguePrintSec;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Animation)]
        public Animator animator;

        [Header("Interaction Event")] [Space(10)]
        public InteractionEvents interactionStartActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game, true)]
        public InteractionEvents interactionEndActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public InteractionEvents onSuccessEndAction;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public InteractionEvents onFailEndAction;

        [Header("인터랙션 방법")] public InteractionMethod interactionMethod;

        [Header("카메라 뷰")] public bool isViewChange;

        [FormerlySerializedAs("isFocusObject")] [ConditionalHideInInspector("isViewChange")]
        public bool isTrackTransform;

        [FormerlySerializedAs("isMain")] [ConditionalHideInInspector("isViewChange")]
        public bool isTrackMainCharacter;

        [ConditionalHideInInspector("isTrackTransform")]
        public Transform trackTransform;

        [ConditionalHideInInspector("isViewChange")]
        public CamInfo interactionCamera;

        public bool isCameraHold;

        [Header("캐릭터 이동")] public bool isTeleport;

        [ConditionalHideInInspector("isTeleport")]
        public Transform teleportTarget;
        
        [Header("타임라인")]
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Cinematic)]
        public PlayableDirector playableDirector;
        public GameObject[] cinematics;
        public GameObject[] inGames;

        [Header("Interaction State")] [Space(10)]
        public bool isLoop;
        [FormerlySerializedAs("isContinue")] [ConditionalHideInInspector("isNextInteract", true)]
        public bool isNextInteractable;
        [ConditionalHideInInspector("isNextInteractable", true)] [FormerlySerializedAs("isInteract")]
        public bool isNextInteract;

        // Save 관련 체크 필요
        [Space(10)] public bool isWait;
        [ConditionalHideInInspector("isWait")] public WaitInteractionData waitInteractionData;

        [Header("Serialized Data")] [Space(10)]
        public SerializedInteractionData serializedInteractionData;

        [Header("디버깅용")] [Space(10)] public List<TaskData> debugTaskData;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public DialogueData dialogueData;

        private CamInfo savedCamInfo;
        private CameraViewType savedCamViewType;
        private Transform savedTrackTransform;

        private bool savedIsFreezeX;
        private bool savedIsFreezeY;
        private bool savedIsFreezeZ;

        public InteractionData()
        {
            interactionStartActions = new InteractionEvents
            {
                interactionEvents = new List<InteractionEvent>()
            };
            interactionEndActions = new InteractionEvents
            {
                interactionEvents = new List<InteractionEvent>()
            };
        }

        public void StartAction(Transform transform)
        {
            serializedInteractionData.isInteracted = true;

            var cameraMoving = DataController.Instance.Cam.GetComponent<CameraMoving>();
            savedCamViewType = cameraMoving.ViewType;
            savedTrackTransform = cameraMoving.TrackTransform;
            savedCamInfo = DataController.Instance.camOffsetInfo;

            // savedIsFreezeX = cameraMoving.;
            // savedIsFreezeY = false;
            // savedIsFreezeZ = false;

            Debug.LogWarning($"Save Camera - {savedCamViewType}, {savedTrackTransform} {savedCamInfo}");

            if (isViewChange)
            {
                if (isTrackTransform)
                {
                    if (trackTransform == null)
                    {
                        DataController.Instance.Cam.GetComponent<CameraMoving>()
                            .Initialize(CameraViewType.FollowCharacter, transform);
                    }
                    else
                    {
                        DataController.Instance.Cam.GetComponent<CameraMoving>()
                            .Initialize(CameraViewType.FollowCharacter, trackTransform);

                    }
                }
                else if (isTrackMainCharacter)
                {
                    DataController.Instance.Cam.GetComponent<CameraMoving>()
                        .Initialize(CameraViewType.FollowCharacter,
                            DataController.Instance.GetCharacter(CharacterType.Main).transform);
                }
                else
                {
                    DataController.Instance.Cam.GetComponent<CameraMoving>()
                        .Initialize(CameraViewType.FollowCharacter, transform);
                }

                DataController.Instance.camOffsetInfo = interactionCamera;
            }

            Debug.Log($"텔레포트: {isTeleport}");
            if (isTeleport)
            {
                var character = DataController.Instance.GetCharacter(CharacterType.Main);
                character.Teleport(teleportTarget);
            }

            foreach (var interactionEvent in interactionStartActions.interactionEvents)
            {
                interactionEvent.Action();
            }
        }

        public void EndAction(bool isSuccess = false)
        {
            if (!isCameraHold)
            {
                var cameraMoving = DataController.Instance.Cam.GetComponent<CameraMoving>();

                cameraMoving.Initialize(savedCamViewType, savedTrackTransform);
                DataController.Instance.camOffsetInfo = savedCamInfo;

                Debug.LogWarning($"Load Camera - {savedCamViewType}, {savedTrackTransform} {savedCamInfo}");
                // if (savedIsFreezeX)
                // {
                //     cameraMoving.Freeze(FreezeType.X);
                // }
                //
                // if (savedIsFreezeY)
                // {
                //     cameraMoving.Freeze(FreezeType.Y);
                // }
                //
                // if (savedIsFreezeZ)
                // {
                //     cameraMoving.Freeze(FreezeType.Z);
                // }
            }

            if (interactionPlayType == InteractionPlayType.Game)
            {
                if (isSuccess)
                {
                    foreach (var action in onSuccessEndAction.interactionEvents)
                    {
                        action.Action();
                    }
                }
                else
                {
                    foreach (var action in onFailEndAction.interactionEvents)
                    {
                        action.Action();
                    }
                }
            }
            else
            {
                foreach (var endAction in interactionEndActions.interactionEvents)
                {
                    endAction.Action();
                }
            }
        }
    }
}