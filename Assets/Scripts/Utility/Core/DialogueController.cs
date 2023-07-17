using System;
using System.Collections;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Cinematic;
using Utility.Preference;
using Utility.Utils;
using static Data.CustomEnum;

namespace Utility.Core
{
    [RequireComponent(typeof(Animator))]
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        public GameObject dialoguePanel;

        [Header("UI")] [FormerlySerializedAs("dialogueBox")] [SerializeField]
        private Image dialogueInputArea;

        [SerializeField] private Animator charEmotionAnimator;
        [SerializeField] private Text dialogueNameText;
        [SerializeField] private Text dialogueContentText;
        [SerializeField] private Transform choiceRoot;

        [Header("Debug")] public TaskData taskData;
        public DialogueData dialogueData;

        [NonSerialized] public bool IsDialogue;

        private GameObject[] choiceButtons;
        private Text[] choiceTexts;
        private bool isUnfolding;
        private Coroutine printCoroutine;

        private static readonly int Emotion = Animator.StringToHash("Emotion");

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
            var childCount = choiceRoot.childCount;
            choiceButtons = new GameObject[childCount];
            choiceTexts = new Text[childCount];
            for (var index = 0; index < choiceRoot.childCount; index++)
            {
                choiceButtons[index] = choiceRoot.GetChild(index).gameObject;
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
            Debug.Log("대화 시작");
            if (taskData != null)
            {
                taskData.isContinue = false;
            }

            JoystickController.Instance.ResetJoyStickState();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            dialogueData.Init(jsonString, printSec);
            IsDialogue = true;
            dialoguePanel.SetActive(true);
            PlayUIController.Instance.SetMenuActive(false);
            isUnfolding = false;

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
            var dialogueDataItem = dialogueData.dialogues[dialogueData.dialogueIdx];
            // 캐릭터 표정 업데이트 (애니메이션)
            if (!string.IsNullOrEmpty(dialogueDataItem.anim_name))
            {
                if (Enum.TryParse(dialogueDataItem.anim_name, out Character who))
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
                        charEmotionAnimator.SetInteger(Emotion, (int)dialogueDataItem.expression);
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

            AudioController.Instance.PlayOneShot(dialogueData.dialogues[dialogueData.dialogueIdx].sfx);
        }

        private void StartDialoguePrint()
        {
            isUnfolding = true;
            // blinkingIndicator.SetActive(false);
            dialogueNameText.text = "";
            dialogueContentText.text = "";
            printCoroutine = StartCoroutine(DialoguePrint());
        }

        private IEnumerator DialoguePrint()
        {
            var dialogueDataItem = dialogueData.dialogues[dialogueData.dialogueIdx];

            Debug.Log("프린트 시작");

            dialogueNameText.text = dialogueDataItem.name;

            var printSec = DataController.Instance.dialoguePrintSec;

            if (!Mathf.Approximately(dialogueDataItem.PrintSec, 0f))
            {
                printSec = dialogueDataItem.PrintSec;
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

            Debug.Log("프린트 끝");
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

            var dialogueItem = dialogueData.dialogues[dialogueData.dialogueIdx];
            dialogueNameText.text = dialogueItem.name;
            dialogueContentText.text = dialogueItem.contents;

            // if (currentDialogueData.dialogueElements.Length > currentDialogueData.index + 1 &&
            //     currentDialogueData.dialogueElements[currentDialogueData.index + 1].dialogueType == DialogueType.Choice)
            // {
            //     currentDialogueData.index++;
            //     InitChoice();
            // }
            // else
            // {
            //     blinkingIndicator.SetActive(true);
            //     _onComplete?.Invoke();
            // }
            dialogueData.dialogueIdx++;
        }

        private void EndConversation()
        {
            Debug.Log("대화 종료");
            IsDialogue = false;
            dialoguePanel.SetActive(false);
            PlayUIController.Instance.SetMenuActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            foreach (var positionSet in DataController.Instance.CurrentMap.positionSets)
            {
                DataController.Instance.GetCharacter(positionSet.who).Emotion = Expression.IDLE;
            }

            dialogueData.Reset();

            if (taskData != null)
            {
                taskData.isContinue = true;
            }
        }

        public void OpenChoicePanel()
        {
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            var choiceLen = int.Parse(taskData.tasks[taskData.taskIndex].nextFile);

            var likeable = DataController.Instance.GetLikeable();

            choiceRoot.gameObject.SetActive(true);
            if (taskData.tasks.Length <= taskData.taskIndex + choiceLen)
            {
                Debug.LogError("선택지 개수 오류 - IndexOverFlow");
            }

            int choiceIndex;

            // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
            for (choiceIndex = 0; choiceIndex < choiceLen && choiceIndex < choiceButtons.Length; choiceIndex++)
            {
                // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음
                var choiceTask = taskData.tasks[taskData.taskIndex + choiceIndex + 1];

                choiceTask.condition = choiceTask.condition.Replace("m", "-");
                Debug.Log($"{choiceIndex}번째 선택지 조건 - {choiceTask.condition}");
                var condition = Array.ConvertAll(choiceTask.condition.Split(','), int.Parse);

                //양수인 경우 이상, 음수인 경우 이하
                if (choiceTask.order >= 0)
                {
                    if (condition[0] < likeable[0] || condition[1] < likeable[1] || condition[2] < likeable[2])
                    {
                        choiceButtons[choiceIndex].SetActive(false);
                        continue;
                    }

                    choiceButtons[choiceIndex].SetActive(true);
                    choiceTexts[choiceIndex].text = choiceTask.name;
                }
                else
                {
                    if (condition[0] > likeable[0] || condition[1] > likeable[1] || condition[2] > likeable[2])
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

        public void SetChoiceAction(UnityAction<int> choiceAction)
        {
            dialogueData.ChooseAction += choiceAction;
        }

        public void SetDialogueEndAction(UnityAction endAction)
        {
            dialogueData.DialogueEndAction += endAction;
        }

        private void RemoveChoice()
        {
            choiceButtons[0].transform.parent.gameObject.SetActive(false);
        }

        // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
        public void OnClickChoice(int index)
        {
            RemoveChoice();
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            dialogueData.ChooseAction?.Invoke(index);
            dialogueData.ChooseAction = null;
        }

        private bool IsDialogueEnd()
        {
            return dialogueData.dialogueIdx >= dialogueData.dialogues.Length;
        }

        public void SetText(string subject, string contents)
        {
            dialogueNameText.text = subject;
            dialogueContentText.text = contents;
        }

        public void SetInputEnable(bool isEnable)
        {
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