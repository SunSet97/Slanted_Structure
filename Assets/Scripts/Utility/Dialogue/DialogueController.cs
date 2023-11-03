using System;
using System.Collections;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.Interaction;
using Utility.Timeline;
using Utility.UI;
using Utility.Utils;

namespace Utility.Dialogue
{
    [RequireComponent(typeof(Animator))]
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        public GameObject dialoguePanel;

#pragma warning disable 0649
        [Header("UI")] [FormerlySerializedAs("dialogueBox")] [SerializeField]
        private Image dialogueInputArea;

        [SerializeField] private Animator charEmotionAnimator;
        [SerializeField] private Text dialogueNameText;
        [SerializeField] private Text dialogueContentText;
        [FormerlySerializedAs("choiceRoot")] [SerializeField] private Transform choicePanel;

        [Header("Timeline")] public PlayableDirector playableDirector;
        public Animator dialogueBindAnimator;
        
        [Header("Debug")] public TaskData debugTaskData;
        [SerializeField] private DialogueData dialogueData;

        [NonSerialized] public bool IsDialogue;
#pragma warning restore 0649

        private GameObject[] choiceButtons;
        private Text[] choiceTexts;
        private bool isUnfolding;
        private Coroutine printCoroutine;

        private CameraViewType savedCamViewType;
        private CamInfo savedCamInfo;
        private Transform savedTrackTransform;
        
        private static readonly int EmotionHash = Animator.StringToHash("Emotion");

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Init();
            }
        }

        private void Init()
        {
            dialogueData = new DialogueData();
            var choiceCount = choicePanel.childCount;
            choiceButtons = new GameObject[choiceCount];
            choiceTexts = new Text[choiceCount];
            for (var index = 0; index < choicePanel.childCount; index++)
            {
                choiceButtons[index] = choicePanel.GetChild(index).gameObject;
                choiceTexts[index] = choiceButtons[index].GetComponentInChildren<Text>(true);
            }

            dialogueInputArea.alphaHitTestMinimumThreshold = 0.1f;
            var eventTrigger = dialogueInputArea.GetComponent<EventTrigger>();
            var entryPointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entryPointerUp.callback.AddListener(data => { OnInputDialogue(); });
            eventTrigger.triggers.Add(entryPointerUp);
        }

        public void StartConversation(string jsonString, float printSec = 0f)
        {
            if (IsDialogue)
            {
                Debug.Log("대화중임");
                return;
            }
            
            Debug.Log("대화 시작");
            
            JoystickController.Instance.ResetJoyStickState();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            PlayUIController.Instance.SetMenuActive(false);
            
            dialogueData.Init(jsonString, printSec);
            IsDialogue = true;
            isUnfolding = false;
            dialoguePanel.SetActive(true);
            
            savedCamViewType = DataController.Instance.Cam.GetComponent<CameraMoving>().ViewType; 
            savedTrackTransform = DataController.Instance.Cam.GetComponent<CameraMoving>().TrackTransform;
            savedCamInfo = DataController.Instance.camOffsetInfo;

            ProgressDialogue();
        }

        public void StartConversation(DialogueElement dialogueElement, float printSec = 0f)
        {
            if (IsDialogue)
            {
                Debug.Log("대화중임");
                return;
            }
            
            Debug.Log("대화 시작");
            
            JoystickController.Instance.ResetJoyStickState();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            PlayUIController.Instance.SetMenuActive(false);
            
            dialogueData.Init(dialogueElement, printSec);
            IsDialogue = true;
            isUnfolding = false;
            dialoguePanel.SetActive(true);
            
            savedCamViewType = DataController.Instance.Cam.GetComponent<CameraMoving>().ViewType; 
            savedTrackTransform = DataController.Instance.Cam.GetComponent<CameraMoving>().TrackTransform;
            savedCamInfo = DataController.Instance.camOffsetInfo;

            ProgressDialogue();
        }
        
        private void OnInputDialogue()
        {
            if (isUnfolding)
            {
                CompletePrint();
            }
            else
            {
                ProgressDialogue();
            }
        }

        private void ProgressDialogue()
        {
            if (IsDialogueEnd())
            {
                EndConversation();
            }
            else
            {
                DialogueAction();
            }
        }

        private void DialogueAction()
        {
            StartDialoguePrint();

            DialogueOption();
        }

        private void DialogueOption()
        {
            var dialogueDataItem = dialogueData.dialogueElements[dialogueData.dialogueIdx];
            // 캐릭터 표정 업데이트 (애니메이션)
            if (!string.IsNullOrEmpty(dialogueDataItem.anim_name))
            {
                if (Enum.TryParse(dialogueDataItem.anim_name, out Character.CharacterType who))
                {
                    var character = DataController.Instance.GetCharacter(who);
                    character.Emotion = dialogueDataItem.expression;
                }

                if (charEmotionAnimator)
                {
                    string animatorPath = "Character_dialogue/" + dialogueDataItem.anim_name;
                    charEmotionAnimator.runtimeAnimatorController =
                        Resources.Load(animatorPath) as RuntimeAnimatorController;
                    if (charEmotionAnimator.runtimeAnimatorController != null)
                    {
                        charEmotionAnimator.GetComponent<Image>().enabled = true;
                        charEmotionAnimator.SetInteger(EmotionHash, (int)dialogueDataItem.expression);
                    }
                    else
                    {
                        charEmotionAnimator.GetComponent<Image>().enabled = false;
                        // Debug.LogError("Dialogue 애니메이션 세팅 오류");
                    }
                }
                else
                {
                    Debug.LogError("Canvas에 캐릭터 표정 charAnimator 넣으세요");
                }
            }
            else
            {
                charEmotionAnimator.runtimeAnimatorController = null;
                charEmotionAnimator.GetComponent<Image>().enabled = false;
            }

            var settedCharacter = DataController.Instance.CurrentMap.characters.Find(
                item => item.who.ToString().Equals(dialogueDataItem.anim_name));
            if (settedCharacter != null)
            {
                if (settedCharacter.characterAnimator.TryGetComponent(out CinematicCharacter cinematicCharacter))
                {
                    cinematicCharacter.EmotionAnimationSetting((int)dialogueDataItem.expression);
                    cinematicCharacter.ExpressionSetting(dialogueDataItem.expression);
                }
            }

            if (dialogueDataItem.isViewChange)
            {
                // !(XOR & !AND) -> !XOR | AND  -> !(100 010 001)
                if (!(dialogueDataItem.isOriginalCamInfo ^ dialogueDataItem.isTrackMainCharacter ^
                      dialogueDataItem.isTrackTransform) ||
                    (dialogueDataItem.isOriginalCamInfo && dialogueDataItem.isTrackMainCharacter &&
                     dialogueDataItem.isTrackTransform))
                {
                    Debug.LogError($"Setting Error - Dialogue Camera 관련 bool 오류 {dialogueData.dialogueIdx}");
                }

                if (dialogueDataItem.isTrackTransform)
                {
                    DataController.Instance.Cam.GetComponent<CameraMoving>()
                        .Initialize(CameraViewType.FollowCharacter, dialogueDataItem.trackTransform);
                }
                else if (dialogueDataItem.isTrackMainCharacter)
                {
                    DataController.Instance.Cam.GetComponent<CameraMoving>()
                        .Initialize(CameraViewType.FollowCharacter, DataController.Instance.GetCharacter(Character.CharacterType.Main).transform);
                } 
                else if (dialogueDataItem.isOriginalCamInfo)
                {
                    DataController.Instance.Cam.GetComponent<CameraMoving>()
                        .Initialize(savedCamViewType, savedTrackTransform);
                }

                DataController.Instance.camOffsetInfo = dialogueDataItem.isOriginalCamInfo ? savedCamInfo : dialogueDataItem.camOffsetInfo;
            }

            AudioController.Instance.PlayOneShot(dialogueDataItem.sfx);
        }

        private void StartDialoguePrint()
        {
            isUnfolding = true;
            // blinkingIndicator.SetActive(false);
            dialogueNameText.text = "";
            dialogueContentText.text = "";

            if (printCoroutine != null)
            {
                StopCoroutine(printCoroutine);
            }

            printCoroutine = StartCoroutine(DialoguePrint());
        }

        private IEnumerator DialoguePrint()
        {
            var dialogueDataItem = dialogueData.dialogueElements[dialogueData.dialogueIdx];

            Debug.Log("프린트 시작");

            dialogueNameText.text = dialogueDataItem.name;

            var printSec = DataController.Instance.dialoguePrintSec;

            if (!Mathf.Approximately(dialogueDataItem.printSec, 0f))
            {
                printSec = dialogueDataItem.printSec;
            }

            var waitForSec = new WaitForSeconds(printSec);

            foreach (var t in dialogueDataItem.contents)
            {
                dialogueContentText.text += t;

                // if (!t.Equals(' '))
                // {
                //     yield return waitForSec;
                // }

                yield return waitForSec;
            }
            
            CompletePrint();
        }

        private void CompletePrint()
        {
            if (printCoroutine != null)
            {
                StopCoroutine(printCoroutine);
                printCoroutine = null;
            }

            isUnfolding = false;

            var dialogueItem = dialogueData.dialogueElements[dialogueData.dialogueIdx];
            dialogueNameText.text = dialogueItem.name;
            dialogueContentText.text = dialogueItem.contents;
            
            dialogueData.dialogueIdx++;

            if (IsDialogueEnd())
            {
                var taskData = debugTaskData;
                if (taskData != null && taskData.tasks.Count > taskData.taskIndex + 1 && taskData.tasks[taskData.taskIndex + 1].order.Equals(taskData.taskOrder) && taskData.tasks[taskData.taskIndex + 1].taskContentType == TaskContentType.Choice)
                {
                    EndConversation();
                }   
            }
        }

        public void EndConversation()
        {
            Debug.Log($"대화 종료 {IsDialogue}");
            
            if (!IsDialogue)
            {
                return;
            }
            
            IsDialogue = false;
            dialoguePanel.SetActive(false);
            PlayUIController.Instance.SetMenuActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            if (DataController.Instance.CurrentMap)
            {
                foreach (var positionSet in DataController.Instance.CurrentMap.positionSets)
                {
                    DataController.Instance.GetCharacter(positionSet.who).Emotion = Expression.Idle;
                }
            }

            dialogueData.EndDialogue();
        }

        public void OpenChoicePanel(TaskData taskData, UnityAction<int> choiceAction)
        {
            if (!int.TryParse(taskData.tasks[taskData.taskIndex].nextFile, out var choiceCount))
            {
                Debug.LogError(
                    $"선택지 Excel 오류 - taskIndex - {taskData.taskIndex}, {taskData.tasks[taskData.taskIndex].nextFile}");
                return;
            }

            if (taskData.tasks.Count <= taskData.taskIndex + choiceCount)
            {
                Debug.LogError("선택지 개수 오류 - IndexOverFlow");
                return;
            }

            dialogueData.ChooseAction += choiceAction;
            
            var relationshipData = DataController.Instance.GetRelationshipData();

            JoystickController.Instance.StopSaveLoadJoyStick(true);
            choicePanel.gameObject.SetActive(true);

            int choiceIndex;

            // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
            for (choiceIndex = 0; choiceIndex < choiceCount && choiceIndex < choiceButtons.Length; choiceIndex++)
            {
                // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음
                var choiceTask = taskData.tasks[taskData.taskIndex + choiceIndex + 1];

                choiceTask.condition = choiceTask.condition.Replace("m", "-");
                Debug.Log($"{choiceIndex}번째 선택지 조건 - {choiceTask.condition}");
                var conditions = Array.ConvertAll(choiceTask.condition.Split(','), int.Parse);

                //양수인 경우 이상, 음수인 경우 이하
                if (choiceTask.order >= 0)
                {
                    if (conditions[0] < relationshipData[0] || conditions[1] < relationshipData[1] ||
                        conditions[2] < relationshipData[2])
                    {
                        choiceButtons[choiceIndex].SetActive(false);
                        continue;
                    }

                    choiceButtons[choiceIndex].SetActive(true);
                    choiceTexts[choiceIndex].text = choiceTask.name;
                }
                else
                {
                    if (conditions[0] > relationshipData[0] || conditions[1] > relationshipData[1] ||
                        conditions[2] > relationshipData[2])
                    {
                        choiceButtons[choiceIndex].SetActive(false);
                        continue;
                    }

                    choiceButtons[choiceIndex].SetActive(true);
                    choiceTexts[choiceIndex].text = choiceTask.name;
                }
            }

            for (; choiceIndex < choiceButtons.Length; choiceIndex++)
            {
                choiceButtons[choiceIndex].SetActive(false);
            }
        }

        public void AddDialogueEndAction(UnityAction endAction)
        {
            Debug.Log("Add Dialogue EndAction");
            dialogueData.DialogueEndAction += endAction;
        }

        // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
        public void OnClickChoice(int index)
        {
            choicePanel.gameObject.SetActive(false);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            var tAction = dialogueData.ChooseAction;
            dialogueData.ChooseAction = null;
            tAction?.Invoke(index);
        }

        private bool IsDialogueEnd()
        {
            return dialogueData.dialogueIdx >= dialogueData.dialogueElements.Length;
        }

        public void SetText(string subject, string contents)
        {
            dialogueNameText.text = subject;
            dialogueContentText.text = contents;
        }

        public void SetInputEnable(bool isEnable)
        {
            Debug.Log($"Input Enable? {isEnable}");
            dialogueInputArea.raycastTarget = isEnable;
        }

        public static string ConvertPathToJson(string path)
        {
            var directoryPath = path.Split('/')[0];
            var desEp = directoryPath.Last();
            var dialogueName = path.Split('/')[1];
            var assetBundle = AssetBundleMap.GetAssetBundle($"ep{desEp}");
            return assetBundle.LoadAsset<TextAsset>(dialogueName).text;
        }
    }
}