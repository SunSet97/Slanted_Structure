using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using CommonScript;
using Data;
using Play;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Core;
using Utility.Json;
using Utility.Property;
using Task = Data.Task;
using static Data.CustomEnum;

namespace Utility.Interaction
{
    [Serializable]
    public class Interaction
    {
        [Header("인터렉션 방식")] 
        public InteractionPlayType interactionPlayType;

        [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
        public TextAsset jsonFile;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public GameObject gamePlayableGame;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public DialogueData dialogueData;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public InteractionEvents dialogueStartActions;
        
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public InteractionEvents dialogueEndActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Task)]
        public InteractionEvents taskEndActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Cinematic)] 
        [SerializeField]
        public InteractionEvents cinematicEndAction;
        
        [Header("인터랙션 방법")]
        public InteractionMethod interactionMethod;

        [Header("카메라 뷰")] public bool isViewChange;
        [ConditionalHideInInspector("isViewChange")]
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public CamInfo dialogueCamera;

        [Header("시네마틱")]
        public PlayableDirector[] timelines;

        public GameObject[] cinematics;
        public GameObject[] inGames;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public bool isLoopDialogue;
        
        public bool isInteractable = true;
    
        [Header("디버깅 전용 TaskData")]
        public List<TaskData> debugTaskData;
        
        [NonSerialized] public Stack<TaskData> JsonTask;
        
        [NonSerialized] public bool IsInteracted;
    }
    [ExecuteInEditMode]
    public class InteractionObject : MonoBehaviour, IClickable
    {
        public Interaction[] interactions;
        public Interaction interaction;
        
        [Header("인터렉션 방식")] [Space(40)]
        public InteractionPlayType interactionPlayType;

        [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
        public TextAsset jsonFile;
        
        // Stack<TaskData>, pos, rot
        // public InteractionEvents taskEndActions;
        // public InteractionEvents dialogueEndActions;
        
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public GameObject gamePlayableGame;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public InteractionEvent[] dialogueEndAction;
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public InteractionEvents dialogueEndActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Task)]
        public InteractionEvents taskEndActions;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Cinematic)] 
        [SerializeField]
        public InteractionEvents cinematicEndAction;
        
        [Header("인터랙션 방법")]
        public InteractionMethod interactionMethod;

        [Header("카메라 뷰")] [SerializeField] private bool isViewChange;
        [ConditionalHideInInspector("isViewChange")]
        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public CamInfo dialogueCamera;

        [Header("시네마틱")]
        public PlayableDirector[] timelines;

        public GameObject[] cinematics;
        public GameObject[] inGames;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public bool isLoopDialogue;

        [Header("디버깅 전용 TaskData")] public List<TaskData> debugTaskData;
        
        private bool isInteracted;
        

        [Header("아웃라인 설정")]
        [SerializeField] private bool useOutline;
        [ConditionalHideInInspector("useOutline")]
        [SerializeField] private OutlineColor color;
        [ConditionalHideInInspector("useOutline")]
        [SerializeField] private Outline outline;
        [ConditionalHideInInspector("useOutline")] 
        [SerializeField] private int outlineRadius = 5;

        [NonSerialized]
        public GameObject ExclamationMark;

        private int interactIndex;

        private void PushTask(string jsonString)
        {
            Debug.Log("Push Task");
            TaskData taskData = new TaskData
            {
                tasks = JsontoString.FromJsonArray<Task>(jsonString)
            };
            var tInteraction = GetInteraction();
            tInteraction.JsonTask.Push(taskData);
            DialogueController.instance.taskData = taskData;
        }

        private void Awake()
        {
            interaction.interactionPlayType = interactionPlayType;
            interaction.jsonFile = jsonFile;

            interaction.isLoopDialogue = isLoopDialogue;

            interaction.interactionMethod = interactionMethod;

            if (dialogueEndActions.interactionEvents.Count > 0)
            {
                interaction.dialogueEndActions = dialogueEndActions;
            }
            else
            {
                interaction.dialogueEndActions = new InteractionEvents
                {
                    interactionEvents = dialogueEndAction.ToList()
                };
            }

            interaction.gamePlayableGame = gamePlayableGame;
            interaction.taskEndActions = taskEndActions;
            interaction.cinematicEndAction = cinematicEndAction;

            interaction.isViewChange = isViewChange;
            interaction.dialogueCamera = dialogueCamera;

            interaction.cinematics = cinematics;
            interaction.timelines = timelines;
            interaction.inGames = inGames;

            if (!Application.isPlaying)
            {
                return;
            }

            DataController.instance.AddInteractor(this);

            var tIteraction = GetInteraction();
            if (tIteraction.jsonFile)
            {
                if (tIteraction.interactionMethod == InteractionMethod.OnChangeMap)
                {
                    DataController.instance.AddOnLoadMap(StartInteraction);
                }
            }
        }

        protected virtual void Start()
        {
            if (!Application.isPlaying)
            {
                var tInteraction = GetInteraction();
                if (tInteraction.interactionPlayType == InteractionPlayType.Dialogue)
                {
                    tInteraction.dialogueData = new DialogueData
                    {
                        dialogues =
                            JsontoString.FromJsonArray<Dialogue>(tInteraction.jsonFile.text)
                    };
                }
                else if (tInteraction.interactionPlayType == InteractionPlayType.Task)
                {
                    tInteraction.debugTaskData = LoadTaskData();
                }
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            }

            if (useOutline)
            {
                if (TryGetComponent(out Outline t))
                {
                    outline = t;
                }
                else
                {
                    outline = gameObject.AddComponent<Outline>();
                }
            
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineWidth = 8f; // 아웃라인 두께 설정
                outline.enabled = false; // 우선 outline 끄기
                // 아웃라인 색깔 설정
                if (color == OutlineColor.red) outline.OutlineColor = Color.red;
                else if (color == OutlineColor.magenta) outline.OutlineColor = Color.magenta;
                else if (color == OutlineColor.yellow) outline.OutlineColor = Color.yellow;
                else if (color == OutlineColor.green) outline.OutlineColor = Color.green;
                else if (color == OutlineColor.blue) outline.OutlineColor = Color.blue;
                else if (color == OutlineColor.grey) outline.OutlineColor = Color.grey;
                else if (color == OutlineColor.black) outline.OutlineColor = Color.black;
                else if (color == OutlineColor.white) outline.OutlineColor = Color.white;   
            }
        }

        /// <summary>
        /// Dialogue가 시작할 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetDialogueStartEvent(UnityAction unityAction, int index = -1)
        {
            GetInteraction().dialogueStartActions.AddInteraction(unityAction);
        }

        /// <summary>
        /// Dialogue가 끝날 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetDialogueEndEvent(UnityAction unityAction, int index = 0)
        {
            GetInteraction().dialogueEndActions.AddInteraction(unityAction);
        }

        /// <summary>
        /// 선택지를 눌렀을 때 부르는 함수, Task에서는 실행이 된다
        /// </summary>
        /// <param name="index">선택지 번호, 1번부터 시작</param>
        private void ChoiceEvent(int index)
        {
            TaskData currentTaskData = GetInteraction().JsonTask.Peek();
            var tempTaskIndex = currentTaskData.taskIndex;
            var choiceLen = int.Parse(currentTaskData.tasks[tempTaskIndex].nextFile);
            Debug.Log(choiceLen);
            currentTaskData.taskIndex += choiceLen;
            Debug.Log(currentTaskData.taskIndex);
            Task curTask = currentTaskData.tasks[tempTaskIndex + index];
            //변화값
            curTask.increaseVar = curTask.increaseVar.Replace("m", "-");
            int[] changeVal =
                Array.ConvertAll(curTask.increaseVar.Split(','),
                    int.Parse);
            DataController.instance.UpdateLikeable(changeVal);

            string path, jsonString;

            //다음 맵 코드 변경
            if (curTask.order != 0)
            {
                DataController.instance.currentMap.SetNextMapCode(
                    $"{curTask.order,000000}");
            }

            switch (curTask.taskContentType)
            {
                case TaskContentType.DIALOGUE:
                    var newLen = currentTaskData.tasks.Length + 1;
                    Task[] array1 = new Task[newLen];
                    Array.Copy(currentTaskData.tasks, 0, array1, 0, currentTaskData.taskIndex + 1);
                    array1[currentTaskData.taskIndex + 1] = new Task
                    {
                        taskContentType = TaskContentType.TempDialogue,
                        order = array1[tempTaskIndex].order
                    };
                    Debug.Log(currentTaskData.tasks.Length - currentTaskData.taskIndex - 1);
                    Array.Copy(currentTaskData.tasks, currentTaskData.taskIndex + 1, array1, currentTaskData.taskIndex + 2,
                        currentTaskData.tasks.Length - currentTaskData.taskIndex - 1);
                    //dispose gc로 바로 하긴 힘들다
                    currentTaskData.tasks = array1;

                    currentTaskData.isContinue = false;
                    path = curTask.nextFile;
                    jsonString = DialogueController.instance.ConvertPathToJson(path);
                    // currentTaskData.taskIndex--;    // 현재 taskIndex는 선택지이며 선택지 다음 인덱스가 된다. 그런데 대화 종료시 Index가 1 증가하기에 1을 줄여준다.
                    DialogueController.instance.StartConversation(jsonString);
                    //다음 인덱스의 타입
                    break;
                case TaskContentType.TEMP:
                    //새로운 task 실행
                    path = curTask.nextFile;
                    jsonString = DialogueController.instance.ConvertPathToJson(path);
                    PushTask(jsonString);
                    StartInteraction();
                    break;
                case TaskContentType.NEW:
                    StackNewTask(curTask.nextFile);
                    break;
                case TaskContentType.EndingChoice:
                    // curTask.nextFile - 엔딩 이름
                    // 엔딩을 저장할 곳을 찾자 scriptable
                    break;
            }
        }


        /// <summary>
        /// 인터랙션을 처음으로 실행할 때 (ex - 터치)
        /// </summary>
        protected void StartInteraction()
        {
            Interaction tInteraction = GetInteraction();
            
            if (tInteraction.JsonTask == null)
            {
                TaskStart(tInteraction.jsonFile.text);
            }
            
            if (tInteraction.interactionPlayType == InteractionPlayType.Animation && gameObject.GetComponent<Animator>())
            {
                gameObject.GetComponent<Animator>().Play("Start", 0);
            }
            else if (tInteraction.interactionPlayType == InteractionPlayType.Dialogue)
            {
                if (DialogueController.instance.IsTalking)
                {
                    return;
                }

                foreach (var interactionEvent in tInteraction.dialogueStartActions.interactionEvents)
                {
                    DialogueController.instance.SetDialouguePrevAction(interactionEvent.Action);
                }

                foreach (var interactionEvent in tInteraction.dialogueEndActions.interactionEvents)
                {
                    DialogueController.instance.SetDialougueEndAction(interactionEvent.Action);
                }

                DialogueController.instance.StartConversation(tInteraction.jsonFile.text);
            }
            else if (tInteraction.interactionPlayType == InteractionPlayType.Potal &&
                     gameObject.TryGetComponent(out CheckMapClear mapClear))
            {
                mapClear.Clear();
            }
            else if (tInteraction.interactionPlayType == InteractionPlayType.Task)
            {
                if (tInteraction.timelines.Length > 0)
                {
                    foreach (var playableDirector in tInteraction.timelines)
                    {
                        if (!playableDirector)
                        {
                            continue;
                        }

                        var timelineAsset = playableDirector.playableAsset as TimelineAsset;
                        if (timelineAsset != null)
                        {
                            var tracks = timelineAsset.GetOutputTracks();
                            foreach (var temp in tracks)
                            {
                                if (temp is CinemachineTrack)
                                    playableDirector.SetGenericBinding(temp, DataController.instance.cam.GetComponent<CinemachineBrain>());
                            }
                        }
                        else
                        {
                            Debug.LogError("Task 타임라인 오류");
                        }
                    }
                }
                if (tInteraction.JsonTask != null && tInteraction.JsonTask.Count == 0)
                {
                    Debug.LogError("task파일 없음 오류오류");
                }

                StartCoroutine(TaskCoroutine());
            }
            else if (tInteraction.interactionPlayType == InteractionPlayType.Game)
            {
                tInteraction.gamePlayableGame.GetComponent<IGamePlayable>().Play();
            }
            else if (tInteraction.interactionPlayType == InteractionPlayType.Cinematic)
            {
                if (tInteraction.timelines.Length == 1)
                {
                    tInteraction.timelines[0].Play();
                    tInteraction.timelines[0].stopped += director =>
                    {
                        Debug.Log("타임라인 끝");
                        foreach (var endAction in tInteraction.cinematicEndAction.interactionEvents)
                        {
                            Debug.Log(endAction.eventType);
                            endAction.Action();
                        }
                    };
                }
                else
                {
                    Debug.LogError("타임라인 세팅 오류");
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!useOutline)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, outlineRadius);
        }

        private void TaskStart(string json)
        {
            var tInteraction = GetInteraction();
            if (tInteraction.jsonFile && tInteraction.interactionPlayType != InteractionPlayType.Task)
            {
                return;
            }
            Debug.Log("Task Stack 시작");
            tInteraction.JsonTask = new Stack<TaskData>();
            PushTask(json);
        }

        private void StackNewTask(string jsonRoute)
        {
            DialogueController.instance.taskData = null;
            GetInteraction().JsonTask = new Stack<TaskData>();
            GC.Collect();
            PushTask(jsonRoute);
            StopAllCoroutines();
            StartInteraction();
        }

        private IEnumerator TaskCoroutine()
        {
            var tInteractor = GetInteraction();
            TaskData currentTaskData = tInteractor.JsonTask.Peek();
            if (!currentTaskData.isContinue)
            {
                Debug.LogError("멈춘 상태에서 실행 시도함");
                yield break;
            }

            // 진행이 가능하면 true이며 true일 경우 같은 순서번호를 진행
            // 진행 중 멈췄다가 바로 실행되는 경우를 위해서
            // 진행 후 멈췄다가 한 번 더 클릭해야되는 경우 번호를 다음 번호로 하도록한다
            WaitUntil waitUntil = new WaitUntil(() => currentTaskData.isContinue);

            while (currentTaskData.tasks.Length > currentTaskData.taskIndex &&
                   currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) &&
                   currentTaskData.isContinue) //순서 번호 동일한 것 반복
            {
                DialogueController.instance.taskData = currentTaskData;
                Task currentTask = currentTaskData.tasks[currentTaskData.taskIndex];
                Debug.Log(currentTaskData.tasks.Length);
                foreach (var task in currentTaskData.tasks)
                {
                    Debug.Log(task.taskContentType);
                }

                Debug.Log("taskIndex - " + currentTaskData.taskIndex + "\nInteractionType - " +
                          currentTask.taskContentType);
                switch (currentTask.taskContentType)
                {
                    case TaskContentType.DIALOGUE:
                        Debug.Log("대화 시작");
                        currentTaskData.isContinue = false;
                        string path = currentTask.nextFile;
                        Debug.Log($"대화 경로 - {path}");
                        string jsonString = DialogueController.instance.ConvertPathToJson(path);
                        DialogueController.instance.StartConversation(jsonString);
                        break;
                    case TaskContentType.ANIMATION:
                        //세팅된 애니메이션 실행
                        GetComponent<Animator>().Play("Start", 0);
                        break;
                    case TaskContentType.Play:
                        currentTaskData.isContinue = false;
                        IGamePlayable gamePlayable = GameObject.Find(currentTask.nextFile).GetComponent<IGamePlayable>();
                        gamePlayable.Play();
                        yield return new WaitUntil(() => gamePlayable.IsPlay);
                        currentTaskData.isContinue = true;
                        break;
                    case TaskContentType.TEMP:
                        Debug.Log("선택지 열기");
                        currentTaskData.isContinue = false;
                        DialogueController.instance.SetChoiceAction(ChoiceEvent);
                        DialogueController.instance.OpenChoicePanel();
                        break;
                    case TaskContentType.TempDialogue:
                        //선택지에서 대화를 고른 경우
                        Debug.Log("선택지 선택 - 단순 대화");
                        tInteractor.JsonTask.Peek().isContinue = true;
                        yield break;
                    // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                    // 애매하네
                    //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                    case TaskContentType.TaskReset:
                        if (tInteractor.interactionMethod == InteractionMethod.Touch)
                        {
                            tInteractor.IsInteracted = false;
                        }
                    
                        tInteractor.JsonTask.Pop();
                        if (tInteractor.JsonTask.Count > 0)
                        {
                            Debug.LogError("Task 엑셀 관련 오류");
                        }
                        foreach (var taskEndAction in tInteractor.taskEndActions.interactionEvents)
                        {
                            taskEndAction.Action();
                        }

                        yield break;
                    case TaskContentType.NEW:
                    {
                        StackNewTask(currentTask.nextFile);
                        yield break;
                    }
                    case TaskContentType.FadeOut:
                    {
                        if (!FadeEffect.instance.isFaded)
                        {
                            currentTaskData.isContinue = false;
                            FadeEffect.instance.FadeOut();
                            yield return new WaitUntil(() => FadeEffect.instance.isFadeOver);
                            FadeEffect.instance.isFadeOver = false;
                            currentTaskData.isContinue = true;
                        }
                        break;
                    }
                    case TaskContentType.FadeIn:
                    {
                        if (!FadeEffect.instance.isFaded)
                        {
                            currentTaskData.isContinue = false;
                            FadeEffect.instance.FadeIn();
                            yield return new WaitUntil(() => FadeEffect.instance.isFadeOver);
                            FadeEffect.instance.isFadeOver = false;
                            currentTaskData.isContinue = true;
                        }

                        break;
                    }
                    case TaskContentType.THEEND:
                        //게임 엔딩
                        break;
                    case TaskContentType.Cinematic:
                        currentTaskData.isContinue = false;
                        foreach (var cinematic in tInteractor.cinematics)
                        {
                            cinematic.SetActive(true);
                        }

                        foreach (var inGame in tInteractor.inGames)
                        {
                            inGame.SetActive(false);
                        }

                        JoystickController.instance.StopSaveLoadJoyStick(true);

                        PlayableDirector timeline = null;
                        foreach (var t in tInteractor.timelines)
                        {
                            if (currentTask.name == t.playableAsset.name)
                            {
                                timeline = t;
                            }
                        }

                        if (timeline == null && tInteractor.timelines.Length == 1)
                        {
                            timeline = tInteractor.timelines[0];
                        }
                        else
                        {
                            Debug.LogError("타임라인 세팅 오류");
                        }

                        // timeline.Pause();
                        timeline.Play();

                        // Debug.Log(timeline.);
                        // timeline.playableAsset
                        // var temp = timeline.playableAsset.outputs;
                        // foreach (var playableBinding in temp)
                        // {
                        //     Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
                        //     // Debug.Log(playableBinding.sourceObject);
                        //     Debug.Log(playableBinding);
                        //     Debug.Log(playableBinding.outputTargetType);
                        //     if(playableBinding.sourceObject == null) continue;
                        //     Debug.Log(playableBinding.streamName);
                        //     Debug.Log(playableBinding.sourceObject);
                        //     timeline.playableAsset.
                        //     var source = playableBinding.sourceObject;
                        //     if (playableBinding.sourceObject.GetType() == typeof(AnimationTrack))
                        //     {
                        //         // Debug.Log(((AnimationTrack) playableBinding.sourceObject));
                        //         ((AnimationTrack)playableBinding). = DataController.instance.GetCharacter(Character.Main).anim;
                        //         Debug.Log(((AnimationTrack) playableBinding.sourceObject).name);
                        //     }
                        //     Debug.Log(playableBinding.sourceObject.name);
                        //     Debug.Log(playableBinding.sourceObject.hideFlags);
                        //     // playableBinding.
                        //     Debug.Log(timeline.GetGenericBinding(playableBinding.sourceObject));
                        // }
                        yield return new WaitUntil(() =>
                            timeline.state == PlayState.Paused && !timeline.playableGraph.IsValid() &&
                            !DialogueController.instance.IsTalking);
                        // while (true)
                        // {
                        //     if (timeline.duration - timeline.time < 0.04f)
                        //     {
                        //         break;
                        //     }
                        //     yield return null;
                        // }
                        JoystickController.instance.StopSaveLoadJoyStick(false);
                        currentTaskData.isContinue = true;
                        foreach (var cinematic in tInteractor.cinematics)
                        {
                            cinematic.SetActive(false);
                        }

                        foreach (var inGame in tInteractor.inGames)
                        {
                            inGame.SetActive(true);
                        }

                        break;
                    case TaskContentType.Clear:
                        DataController.instance.currentMap.MapClear();
                        break;
                    default:
                    {
                        Debug.LogError($"{currentTask.taskContentType}은 존재하지 않는 type입니다.");
                        break;
                    }
                }

                Debug.Log("Task 종료 대기 중 - " + currentTask.taskContentType + ", Index - " + currentTaskData.taskIndex);
                yield return waitUntil;
                currentTaskData.taskIndex++;
                // Debug.Log("Task 종료 - " + currentTask.taskContentType + ", Index - " + currentTaskData.taskIndex);
                Debug.Log(currentTaskData.tasks.Length + " " + currentTaskData.taskIndex);
                Debug.Log(currentTaskData.tasks.Length > currentTaskData.taskIndex &&
                          currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) &&
                          currentTaskData.isContinue);
            }

            currentTaskData.taskOrder++;

            if (currentTaskData.tasks.Length == currentTaskData.taskIndex)
            {
                if (tInteractor.JsonTask.Count > 1)
                {
                    //선택지인 경우
                    DialogueController.instance.taskData = null; // null이 아닌 상태에서 모든 task가 끝나면 없어야되는데 남아있음
                    tInteractor.JsonTask.Pop();
                    tInteractor.JsonTask.Peek().isContinue = true;
                }
                else
                {
                    foreach (var taskEndAction in tInteractor.taskEndActions.interactionEvents)
                    {
                        taskEndAction.Action();
                    }
                }
            }
        }

        //For Debugging
        private List<TaskData> LoadTaskData()
        {
            var taskDataDebug = new List<TaskData>
            {
                new TaskData
                {
                    tasks = JsontoString.FromJsonArray<Task>(GetInteraction().jsonFile.text)
                }
            };

            return taskDataDebug;
            // if (Application.isPlaying && Application.isEditor)
            // {
            //     foreach (TaskData taskData in taskDataDebug)
            //     {
            //         for (int i = 0; i < taskData.tasks.Length; i++)
            //         {
            //             if (taskData.tasks[i].taskContentType == TaskContentType.NEW ||
            //                 taskData.tasks[i].taskContentType == TaskContentType.TEMP)
            //             {
            //                 var nextFile = int.Parse(taskData.tasks[i].nextFile);
            //                 i++;
            //                 string path = taskData.tasks[i].nextFile;
            //                 Debug.Log(path);
            //
            //                 string dialogueName = path.Split('/')[1];
            //                 Debug.Log("변환 후: " + dialogueName);
            //
            //                 var desEp = int.Parse(dialogueName.Substring(0, 1));
            //                 var dialogueDB =
            //                     AssetBundle.LoadFromFile($"{Application.dataPath}/AssetBundles/dialogue/ep{desEp}");
            //
            //                 var jsonString = dialogueDB.LoadAsset<TextAsset>(dialogueName).text;
            //
            //                 if (taskDataDebug.Count > 50)
            //                 {
            //                     Debug.Log("무한 task 디버깅");
            //                     return;
            //                 }
            //
            //                 taskDataDebug.Add(new TaskData
            //                 {
            //                     tasks = JsontoString.FromJsonArray<Task>(jsonString)
            //                 });
            //
            //
            //                 Debug.Log("디버그 Task 추가" + taskDataDebug.Count);
            //             }
            //         }
            //     }
            // }
        }
        
        public Interaction GetInteraction(int index)
        {
            if (interactions?.Length > index)
            {
                return interactions[index];
            }
            if (interaction.jsonFile)
            {
                return interaction;
            }
            Debug.LogError("인터랙션 데이터 설정 오류");
            return null;
        }

        public Interaction GetInteraction()
        {
            if (interactions?.Length > 0 && interactions.Length > interactIndex)
            {
                return interactions[interactIndex];
            }
            if (interaction.jsonFile)
            {
                return interaction;
            }
            Debug.LogError("인터랙션 데이터 설정 오류");
            return null;
        }

        bool IClickable.IsClickEnable
        {
            get
            {
                var tInteractor = GetInteraction();
                if (tInteractor.jsonFile)
                {
                    return tInteractor.isInteractable && enabled && tInteractor.interactionMethod == InteractionMethod.Touch && !tInteractor.IsInteracted;    
                }

                return false;
            }
            set
            {
                if (value)
                {
                    var tInteractor = GetInteraction();
                    if (tInteractor.jsonFile)
                    {
                        tInteractor.interactionMethod = InteractionMethod.Touch;
                        tInteractor.IsInteracted = false;
                        tInteractor.isInteractable = true;
                    }
                }
                else
                {
                    var tInteractor = GetInteraction();
                    if (tInteractor.jsonFile)
                    {
                        tInteractor.interactionMethod = InteractionMethod.No;
                    }
                }
            }
        }

        bool IClickable.IsClicked
        {
            get
            {
                var tInteractor = GetInteraction();
                if (tInteractor.jsonFile)
                {
                    return tInteractor.IsInteracted;
                }
                return false;
            }
            set
            {
                var tInteractor = GetInteraction();
                if (tInteractor.jsonFile)
                {
                    tInteractor.IsInteracted = value;
                }
            }
        }

        void IClickable.ActiveObjectClicker(bool isActive)
        {
            if (outline)
            {
                outline.enabled = isActive;
            }
            
            if (ExclamationMark)
            {
                ExclamationMark.SetActive(isActive);
            }
            
            if (GetInteraction().interactionMethod == InteractionMethod.Touch)
            {
                ObjectClicker.instance.UpdateClick(this, isActive);
            }
        }

        bool IClickable.GetIsClicked()
        {
            var tInteractor = GetInteraction();
            if (tInteractor.jsonFile)
            {
                return tInteractor.IsInteracted;
            }
            return false;
        }

        void IClickable.Click()
        {
            if (!((IClickable) this).IsClickEnable)
            {
                return;
            }
            var tInteractor = GetInteraction();
            if (tInteractor.jsonFile)
            {
                tInteractor.IsInteracted = !tInteractor.isLoopDialogue;   
            }

            ((IClickable) this).ActiveObjectClicker(false);

            StartInteraction();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || !ObjectClicker.instance)
            {
                return;
            }

            ObjectClicker.instance.UpdateClick(this, false);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying || !ObjectClicker.instance)
            {
                return;
            }

            ObjectClicker.instance.UpdateClick(this, false);
        }

        public void OnEnter()
        {
            if (((IClickable) this).IsClickEnable)
            {
                ((IClickable) this).ActiveObjectClicker(true);
            }
        }

        public void OnExit()
        {
            if (((IClickable) this).IsClickEnable)
            {
                ((IClickable) this).ActiveObjectClicker(false);   
            }
        }
    }
}