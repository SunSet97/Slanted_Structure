using System;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Cinematic;
using Utility.Preference;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        public GameObject dialoguePanel;

        public Image dialogueBox;

        public Animator charEmotionAnimator;

        public Text dialogueNameText;
        public Text dialogueContentText;

        public Transform choiceRoot;

        public TaskData taskData;
        public DialogueData dialogueData;

        [NonSerialized] public bool IsTalking;

        private GameObject[] choiceBtns;
        private Text[] choiceTexts;

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
                // DontDestroyOnLoad(Instance);
            }
        }

        private void Start()
        {
            var childCount = choiceRoot.childCount;
            choiceBtns = new GameObject[childCount];
            choiceTexts = new Text[childCount];
            for(int i = 0; i < childCount; i++)
            {
                choiceBtns[i] = choiceRoot.GetChild(i).gameObject;
                choiceTexts[i] = choiceBtns[i].GetComponentInChildren<Text>(true);
            }

            dialogueBox.alphaHitTestMinimumThreshold = 0.1f;
            EventTrigger eventTrigger = dialogueBox.GetComponent<EventTrigger>();
            EventTrigger.Entry entryPointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entryPointerUp.callback.AddListener(data => { UpdateWord(); });
            eventTrigger.triggers.Add(entryPointerUp);
        }

        public void Initialize()
        {
            dialogueData = new DialogueData();
            dialoguePanel.SetActive(false);
            IsTalking = false;
        }

        public string ConvertPathToJson(string path)
        {
            string dialogueName = path.Split('/')[1];
            return DataController.Instance.DialogueDB.LoadAsset<TextAsset>(dialogueName).text;
        }

        public void StartConversation(string jsonString)
        {
            Debug.Log("대화 시작");
            if (taskData != null)
            {
                taskData.isContinue = false;
            }

            JoystickController.Instance.InitializeJoyStick();
            JoystickController.Instance.StopSaveLoadJoyStick(true);

            dialogueData.Init(jsonString);
            IsTalking = true;

            dialoguePanel.SetActive(true);

            PlayUIController.Instance.SetMenuActive(false);

            UpdateWord();
        }

        private void EndConversation()
        {
            Debug.Log("대화 종료");
            IsTalking = false;
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

        private void UpdateWord()
        {
            int dialogueLen = dialogueData.dialogues.Length;

            if (dialogueData.dialogueIdx >= dialogueLen)
            {
                EndConversation();
            }
            else
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

                dialogueNameText.text = dialogueData.dialogues[dialogueData.dialogueIdx].name;
                dialogueContentText.text = dialogueData.dialogues[dialogueData.dialogueIdx].contents;
                AudioController.Instance.PlayOneShot(dialogueData.dialogues[dialogueData.dialogueIdx].sfx);
                dialogueData.dialogueIdx++;
            }
        }

        public void OpenChoicePanel()
        {
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            var choiceLen = int.Parse(taskData.tasks[taskData.taskIndex].nextFile);

            var likeable = DataController.Instance.GetLikeable();

            choiceBtns[0].transform.parent.gameObject.SetActive(true);
            if (taskData.tasks.Length <= taskData.taskIndex + choiceLen)
            {
                Debug.LogError("선택지 개수 오류 - IndexOverFlow");
            }

            // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
            for(var choiceIndex = 0; choiceIndex < choiceLen && choiceIndex < choiceBtns.Length; choiceIndex++)
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
                        continue;
                    }
                    choiceBtns[choiceIndex].SetActive(true);
                    choiceTexts[choiceIndex].text = choiceTask.name;
                }
                else
                {
                    if (condition[0] > likeable[0] || condition[1] > likeable[1] || condition[2] > likeable[2])
                    {
                        continue;
                    }
                    choiceBtns[choiceIndex].SetActive(true);
                    choiceTexts[choiceIndex].text = choiceTask.name;
                }
            }
        }

        public void SetChoiceAction(UnityAction<int> choiceAction)
        {
            dialogueData.ChooseAction += choiceAction;
        }

        public void SetDialougueEndAction(UnityAction endAction)
        {
            dialogueData.DialogueEndAction += endAction;
        }

        private void RemoveChoice()
        {
            choiceBtns[0].transform.parent.gameObject.SetActive(false);
        }

        // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
        public void PressChoice(int index)
        {
            RemoveChoice();
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            dialogueData.ChooseAction?.Invoke(index);
            dialogueData.ChooseAction = null;
        }
    }
}