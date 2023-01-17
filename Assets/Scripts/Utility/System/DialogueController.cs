using System;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Data.CustomEnum;

namespace Utility.System
{
    public class DialogueController : MonoBehaviour
    {
        private static DialogueController _instance;
    
        public static DialogueController instance => _instance;

        public GameObject dialoguePanel;

        public Animator charEmotionAnimator;

        public Text dialogueNameText;
        public Text dialogueContentText;

        public Transform choiceRoot;
        
        public TaskData taskData;
        public DialogueData dialogueData;
        
        [NonSerialized]
        public bool IsTalking;
    
        private GameObject[] choiceBtns;
        private Text[] choiceTexts;

        private static readonly int Emotion = Animator.StringToHash("Emotion");

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

        private void Start()
        {
            var childCount = choiceRoot.childCount;
            choiceBtns = new GameObject[childCount];
            choiceTexts = new Text[childCount];
            for (int i = 0; i < childCount; i++)
            {
                choiceBtns[i] = choiceRoot.GetChild(i).gameObject;
                choiceTexts[i] = choiceBtns[i].GetComponentInChildren<Text>(true);
            }

            EventTrigger eventTrigger = dialoguePanel.GetComponent<EventTrigger>();
            EventTrigger.Entry entryPointerUp = new EventTrigger.Entry();
            entryPointerUp.eventID = EventTriggerType.PointerUp;
            entryPointerUp.callback.AddListener(data =>
            {
                UpdateWord(); 
            
            });
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
            return DataController.instance.dialogueDB.LoadAsset<TextAsset>(dialogueName).text;
        }

        public void StartConversation(string jsonString)
        {
            Debug.Log("대화 시작");
            if (taskData != null)
            {
                taskData.isContinue = false;
            }

            JoystickController.instance.StopSaveLoadJoyStick(true);

            dialogueData.Init(jsonString);
            IsTalking = true;
            
            dialoguePanel.SetActive(true);
            
            UpdateWord();
        }

        private void UpdateWord()
        {
            int dialogueLen = dialogueData.dialogues.Length;
            
            if (dialogueData.dialogueIdx >= dialogueLen)
            {
                Debug.Log("대화 종료");
                IsTalking = false;
                dialoguePanel.SetActive(false);
                JoystickController.instance.StopSaveLoadJoyStick(false);
                foreach (var positionSet in DataController.instance.currentMap.positionSets)
                {
                    DataController.instance.GetCharacter(positionSet.who).Emotion = Expression.IDLE;   
                }
                foreach (var positionSet in DataController.instance.currentMap.characters)
                {
                    // if (Character.TryParse(dialogueData.dialogues[dialogueIdx].anim_name, out Character who))
                    DataController.instance.GetCharacter(positionSet.who).Emotion = Expression.IDLE;   
                }
                dialogueData.Reset();
                
                if (taskData != null)
                {
                    taskData.isContinue = true;
                }
            }
            else
            {
                // 캐릭터 표정 업데이트 (애니메이션)
                if (!string.IsNullOrEmpty(dialogueData.dialogues[dialogueData.dialogueIdx].anim_name))
                {
                    if (charEmotionAnimator)
                    {
                        // Debug.Log(dialogueData.dialogues[dialogueData.dialogueIdx].anim_name);
                        string path = "Character_dialogue/" + dialogueData.dialogues[dialogueData.dialogueIdx].anim_name;
                        charEmotionAnimator.runtimeAnimatorController =
                            Resources.Load(path) as RuntimeAnimatorController;
                        if (charEmotionAnimator.runtimeAnimatorController != null)
                        {
                            charEmotionAnimator.GetComponent<Image>().enabled = true;
                            // Debug.Log((int) dialogueData.dialogues[dialogueData.dialogueIdx].expression + "  " +
                            //           dialogueData.dialogues[dialogueData.dialogueIdx].expression);
                            charEmotionAnimator.SetInteger(Emotion, ((int) dialogueData.dialogues[dialogueData.dialogueIdx].expression));

                            if (Enum.TryParse(dialogueData.dialogues[dialogueData.dialogueIdx].anim_name, out Character who))
                            {
                                DataController.instance.GetCharacter(who).Emotion = dialogueData.dialogues[dialogueData.dialogueIdx].expression;   
                            }
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

                // 대화 텍스트 업데이트

                // 현재 실행 중인 캐릭터에 대해서
                // 현재 맵에 기준???    햄버거 집 스핏의 경우 임시로 존재하는 거니까
                // MapData에 inspector window에서 setting, position setting되어있는 캐릭터들도 추가
                // 대화 시 무슨 캐릭터인지 anim_name으로 Find (who를 사용)
                MapData.AnimationCharacterSet animator = DataController.instance.currentMap.characters.Find(
                    item => item.who.ToString().Equals(dialogueData.dialogues[dialogueData.dialogueIdx].anim_name));
                // DataController.instance.GetCharacter(animator.who).emotion =(int) dialogueData.dialogues[dialogueIdx].expression 
                animator?.characterAnimator.SetInteger(Emotion, (int)dialogueData.dialogues[dialogueData.dialogueIdx].expression);
            
                dialogueNameText.text = dialogueData.dialogues[dialogueData.dialogueIdx].name;
                dialogueContentText.text = dialogueData.dialogues[dialogueData.dialogueIdx].contents;
                dialogueData.dialogueIdx++;
            }
        }
    
        public void OpenChoicePanel()
        {
            JoystickController.instance.StopSaveLoadJoyStick(true);
            int index = taskData.taskIndex;
            int choiceLen = int.Parse(taskData.tasks[index].nextFile);

            int[] likeable = DataController.instance.GetLikeable();

            choiceBtns[0].transform.parent.gameObject.SetActive(true);
            if (taskData.tasks.Length <= index + choiceLen)
            {
                Debug.LogError("선택지 개수 오류 - IndexOverFlow");
            }
            // 선택지 개수와 조건에 맞게 선택지가 나올 수 있도록 함.
            for (int i = 0; i < choiceBtns.Length; i++)
            {
                if (choiceLen <= i)
                {
                    choiceBtns[i].SetActive(false);
                    continue;
                }
                // 친밀도와 자존감이 기준보다 낮으면 일부 선택지가 나오지 않을 수 있음
                var choiceIndex = index + i + 1;
                var choiceTask = taskData.tasks[choiceIndex];
            
                choiceTask.condition = choiceTask.condition.Replace("m", "-");
                Debug.Log($"{i}번째 선택지 조건 - {choiceTask.condition}");
                int[] condition = Array.ConvertAll(choiceTask.condition.Split(','), int.Parse);

                //양수인 경우 이상, 음수인 경우 이하
                if (choiceTask.order >= 0)
                {
                    if (condition[0] >= likeable[0] && condition[1] >= likeable[1] && condition[2] >= likeable[2])
                    {
                        choiceBtns[i].SetActive(true);
                        choiceTexts[i].text = choiceTask.name;
                    }    
                }
                else
                {
                    if (condition[0] <= likeable[0] && condition[1] <= likeable[1] && condition[2] <= likeable[2])
                    {
                        choiceBtns[i].SetActive(true);
                        choiceTexts[i].text = choiceTask.name;
                    }    
                }
            }
        }
    
        public void SetChoiceAction(UnityAction<int> choiceAction)
        {
            dialogueData.ChooseAction += choiceAction;
        }
        public void SetDialouguePrevAction(UnityAction prevAction)
        {
            dialogueData.DialoguePrevAction += prevAction;
        }
        public void SetDialougueEndAction(UnityAction endAction)
        {
            dialogueData.DialogueEndAction += endAction;
        }
        /// <summary>
        /// 대화 선택지를 삭제하는 함수
        /// </summary>
        void RemoveChoice()
        {
            choiceBtns[0].transform.parent.gameObject.SetActive(false);
        }

        // 선택지를 눌렀을 때 불리는 함수, Index 1부터 시작
        public void PressChoice(int index)
        {
            RemoveChoice();
            JoystickController.instance.StopSaveLoadJoyStick(false);
            dialogueData.ChooseAction?.Invoke(index);
            dialogueData.ChooseAction = null;
        }
    }
}
