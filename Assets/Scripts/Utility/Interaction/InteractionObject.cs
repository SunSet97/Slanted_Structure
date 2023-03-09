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
using Utility.Ending;
using Utility.Interaction.Click;
using Utility.Json;
using Utility.Preference;
using Utility.Property;
using Task = Data.Task;
using static Data.CustomEnum;

namespace Utility.Interaction
{
    [Serializable]
    public class SerializedInteractionData
    {
        public int id;
        public bool isInteractable = true;

        [NonSerialized] public Stack<TaskData> JsonTask;

        [Header("디버깅용")]
        [SerializeField] internal bool isInteracted;
    }

    [Serializable]
    public class Interaction
    {
        [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
        public TextAsset jsonFile;

        [Header("인터렉션 방식")] public InteractionPlayType interactionPlayType;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Game)]
        public GameObject gamePlayableGame;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
        public DialogueData dialogueData;

        [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Animation)]
        public Animator animator;

        // [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.FadeOut)]
        // public float fadeSec;

        [Space(10)]

        public InteractionEvents interactionStartActions;

        public InteractionEvents interactionEndActions;

        [Header("인터랙션 방법")] public InteractionMethod interactionMethod;

        [Header("카메라 뷰")] public bool isViewChange;

        [ConditionalHideInInspector("isViewChange")]
        public CamInfo interactionCamera;
        
        [Header("시네마틱")] public PlayableDirector[] timelines;
        public GameObject[] cinematics;
        public GameObject[] inGames;

        [Space(10)]
        public bool isLoop;

        [Space(10)] public bool isContinue;

        [Space(10)] public SerializedInteractionData serializedInteractionData;

        [Header("디버깅 전용 TaskData")] [Space(10)]
        public List<TaskData> debugTaskData;

        public Interaction()
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

        public void StartAction()
        {
            serializedInteractionData.isInteracted = true;

            if (isViewChange)
            {
                DataController.Instance.CamOffsetInfo.camDis = interactionCamera.camDis;
                DataController.Instance.CamOffsetInfo.camRot = interactionCamera.camRot;
            }

            foreach (var interactionEvent in interactionStartActions.interactionEvents)
            {
                interactionEvent.Action();
            }
        }

        public void EndAction()
        {
            if (isViewChange)
            {
                DataController.Instance.CamOffsetInfo.camDis = Vector3.zero;
                DataController.Instance.CamOffsetInfo.camRot = Vector3.zero;
            }

            foreach (var endAction in interactionEndActions.interactionEvents)
            {
                endAction.Action();
            }
        }
    }

    [ExecuteInEditMode]
    public class InteractionObject : MonoBehaviour, IClickable
    {
        public string id;
        public List<Interaction> interactions;

        [Header("아웃라인 설정")] [Space(20)] [SerializeField]
        private bool useOutline;

        [ConditionalHideInInspector("useOutline")] [SerializeField]
        private OutlineColor color;

        [ConditionalHideInInspector("useOutline")] [SerializeField]
        private Outline outline;

        [Header("기즈모")] [Space(20)] [SerializeField]
        private bool useGizmos;

        [ConditionalHideInInspector("useGizmos")] [SerializeField]
        private int gizmosRadius = 5;

        [ConditionalHideInInspector("useGizmos")] [SerializeField]
        private Color gizmosColor;

        [NonSerialized] public GameObject ExclamationMark;

        [NonSerialized] public int InteractIndex;

        private void PushTask(string jsonString)
        {
            Debug.Log("Push Task");
            TaskData taskData = new TaskData
            {
                tasks = JsontoString.FromJsonArray<Task>(jsonString)
            };
            var interaction = GetInteraction();
            interaction.serializedInteractionData.JsonTask.Push(taskData);
            DialogueController.Instance.taskData = taskData;
        }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DataController.Instance.AddInteractor(this);

            if (GetInteraction().interactionMethod == InteractionMethod.OnChangeMap)
            {
                Debug.Log("Add OnChange Map");
                DataController.Instance.AddOnLoadMap(StartInteraction);
            }
        }

        protected virtual void Start()
        {
            if (!Application.isPlaying)
            {
                foreach (var interaction in interactions)
                {
                    if (interaction.interactionPlayType == InteractionPlayType.Dialogue)
                    {
                        interaction.dialogueData = new DialogueData
                        {
                            dialogues =
                                JsontoString.FromJsonArray<Dialogue>(interaction.jsonFile.text)
                        };
                    }
                    else if (interaction.interactionPlayType == InteractionPlayType.Task)
                    {
                        interaction.debugTaskData = LoadTaskData();
                    }
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
                outline.OutlineWidth = 8f;
                outline.enabled = false;

                if (color == OutlineColor.Red) outline.OutlineColor = Color.red;
                else if (color == OutlineColor.Magenta) outline.OutlineColor = Color.magenta;
                else if (color == OutlineColor.Yellow) outline.OutlineColor = Color.yellow;
                else if (color == OutlineColor.Green) outline.OutlineColor = Color.green;
                else if (color == OutlineColor.Blue) outline.OutlineColor = Color.blue;
                else if (color == OutlineColor.Grey) outline.OutlineColor = Color.grey;
                else if (color == OutlineColor.Black) outline.OutlineColor = Color.black;
                else if (color == OutlineColor.White) outline.OutlineColor = Color.white;
            }
        }

        /// <summary>
        /// Dialogue가 시작할 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetInteractionStartEvent(UnityAction unityAction, int index = -1)
        {
            if (index != -1)
            {
                GetInteraction(index).interactionStartActions.AddInteraction(unityAction);
            }
            else
            {
                GetInteraction().interactionStartActions.AddInteraction(unityAction);
            }
        }

        /// <summary>
        /// Dialogue가 끝날 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetInteractionEndEvent(UnityAction unityAction, int index = -1)
        {
            if (index != -1)
            {
                GetInteraction(index).interactionEndActions.AddInteraction(unityAction);
            }
            else
            {
                GetInteraction().interactionEndActions.AddInteraction(unityAction);
            }
        }

        /// <summary>
        /// 선택지를 눌렀을 때 부르는 함수, Task에서는 실행이 된다
        /// </summary>
        /// <param name="index">선택지 번호, 1번부터 시작</param>
        private void ChoiceEvent(int index)
        {
            TaskData currentTaskData = GetInteraction().serializedInteractionData.JsonTask.Peek();
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
            DataController.Instance.UpdateLikeable(changeVal);

            string jsonString;

            //다음 맵 코드 변경
            if (curTask.order != 0)
            {
                DataController.Instance.CurrentMap.SetNextMapCode(
                    $"{curTask.order,000000}");
            }

            switch(curTask.taskContentType)
            {
                case TaskContentType.Dialogue:
                    var newLen = currentTaskData.tasks.Length + 1;
                    Task[] array1 = new Task[newLen];
                    Array.Copy(currentTaskData.tasks, 0, array1, 0, currentTaskData.taskIndex + 1);
                    array1[currentTaskData.taskIndex + 1] = new Task
                    {
                        taskContentType = TaskContentType.TempDialogue,
                        order = array1[tempTaskIndex].order
                    };
                    Debug.Log(currentTaskData.tasks.Length - currentTaskData.taskIndex - 1);
                    Array.Copy(currentTaskData.tasks, currentTaskData.taskIndex + 1, array1,
                        currentTaskData.taskIndex + 2,
                        currentTaskData.tasks.Length - currentTaskData.taskIndex - 1);
                    //dispose gc로 바로 하긴 힘들다
                    currentTaskData.tasks = array1;

                    currentTaskData.isContinue = false;
                    jsonString = DialogueController.Instance.ConvertPathToJson(curTask.nextFile);
                    // currentTaskData.taskIndex--;    // 현재 taskIndex는 선택지이며 선택지 다음 인덱스가 된다. 그런데 대화 종료시 Index가 1 증가하기에 1을 줄여준다.
                    DialogueController.Instance.StartConversation(jsonString);
                    //다음 인덱스의 타입
                    break;
                case TaskContentType.Temp:
                    //새로운 task 실행
                    jsonString = DialogueController.Instance.ConvertPathToJson(curTask.nextFile);
                    PushTask(jsonString);
                    StartInteraction();
                    break;
                case TaskContentType.New:
                    StackNewTask(curTask.nextFile);
                    break;
                case TaskContentType.EndingChoice:
                    jsonString = DialogueController.Instance.ConvertPathToJson(curTask.nextFile);
                    DialogueController.Instance.SetDialougueEndAction(() =>
                    {
                        var endingType = (EndingType)curTask.order;
                        EndingHelper.Instance.StartEnd(endingType);
                        //Ennding 번호 - curTask.order

                    });
                    DialogueController.Instance.StartConversation(jsonString);
                    break;
            }
        }

        /// <summary>
        /// 인터랙션을 처음으로 실행할 때 (ex - 터치)
        /// </summary>
        public void StartInteraction()
        {
            Debug.Log("Start Interaction");
            var interaction = GetInteraction();

            if (!interaction.serializedInteractionData.isInteractable || interaction.serializedInteractionData.isInteracted && !interaction.isLoop)
            {
                Debug.Log($"인터랙션 시작 전 중지 {interaction.serializedInteractionData.isInteractable} {interaction.serializedInteractionData.isInteracted} {interaction.isLoop}");
                return;
            }
            interaction.StartAction();

            if (interaction.jsonFile && interaction.serializedInteractionData.JsonTask == null)
            {
                TaskStart(interaction.jsonFile.text);
            }

            if (interaction.interactionPlayType == InteractionPlayType.Animation)
            {
                interaction.animator.SetTrigger("Start");
                Debug.Log("애니메이션 스타트");
                StartCoroutine(WaitAnimationEnd(interaction));
            }
            else if (interaction.interactionPlayType == InteractionPlayType.Dialogue)
            {
                if (!interaction.jsonFile)
                {
                    Debug.LogError("오류");
                }
                if (DialogueController.Instance.IsTalking)
                {
                    return;
                }

                    DialogueController.Instance.SetDialougueEndAction(interaction.EndAction);
                

                DialogueController.Instance.StartConversation(interaction.jsonFile.text);
            }
            else if (interaction.interactionPlayType == InteractionPlayType.Potal &&
                     gameObject.TryGetComponent(out CheckMapClear mapClear))
            {
                interaction.EndAction();
                mapClear.Clear();
            }
            else if (interaction.interactionPlayType == InteractionPlayType.Task)
            {
                if (interaction.timelines != null && interaction.timelines.Length > 0)
                {
                    var timelineAsset = interaction.timelines[0].playableAsset as TimelineAsset;
                    if (timelineAsset != null)
                    {
                        var tracks = timelineAsset.GetOutputTracks();
                        foreach (var temp in tracks)
                        {
                            if (temp is CinemachineTrack)
                                interaction.timelines[0].SetGenericBinding(temp,
                                    DataController.Instance.Cam.GetComponent<CinemachineBrain>());
                        }
                    }
                    else
                    {
                        Debug.LogError("Task 타임라인 오류");
                    }
                }

                if (interaction.serializedInteractionData.JsonTask != null && interaction.serializedInteractionData.JsonTask.Count == 0)
                {
                    Debug.LogError("task파일 없음 오류오류");
                }

                StartCoroutine(TaskCoroutine(InteractIndex));
            }
            else if (interaction.interactionPlayType == InteractionPlayType.Game)
            {
                var game = interaction.gamePlayableGame.GetComponent<IGamePlayable>();
                game.Play();
                game.OnEndPlay = () =>
                {
                    PlayUIController.Instance.SetMenuActive(false);
                    interaction.EndAction();
                };
            }
            else if (interaction.interactionPlayType == InteractionPlayType.Cinematic)
            {
                if (interaction.timelines != null && interaction.timelines.Length > 0)
                {
                    var timelineAsset = interaction.timelines[0].playableAsset as TimelineAsset;
                    if (timelineAsset != null)
                    {
                        var trackAssets = timelineAsset.GetOutputTracks();
                        foreach (var trackAsset in trackAssets)
                        {
                            if (trackAsset is CinemachineTrack)
                                interaction.timelines[0].SetGenericBinding(trackAsset,
                                    DataController.Instance.Cam.GetComponent<CinemachineBrain>());
                        }
                    }
                    else
                    {
                        Debug.LogError("Task 타임라인 오류");
                    }

                    PlayUIController.Instance.SetMenuActive(false);
                    JoystickController.Instance.StopSaveLoadJoyStick(true);
                    foreach (var interactionInGame in interaction.inGames)
                    {
                        interactionInGame.SetActive(false);
                    }
                    foreach (var interactionCinematic in interaction.cinematics)
                    {
                        interactionCinematic.SetActive(true);
                    }

                    interaction.timelines[0].Play();

                    var waitUntil = new WaitUntil(() => Math.Abs(interaction.timelines[0].time - interaction.timelines[0].duration) <=
                                                        1 / ((TimelineAsset)interaction.timelines[0].playableAsset)
                                                        .editorSettings.fps ||
                                                        interaction.timelines[0].state == PlayState.Paused &&
                                                        !interaction.timelines[0].playableGraph.IsValid() &&
                                                        !DialogueController.Instance.IsTalking);
                    StartCoroutine(WaitTimeline(waitUntil, () =>
                    {
                        JoystickController.Instance.StopSaveLoadJoyStick(false);
                        PlayUIController.Instance.SetMenuActive(true);
                        Debug.Log("타임라인 끝");

                        interaction.EndAction();

                        foreach (var interactionInGame in interaction.inGames)
                        {
                            interactionInGame.SetActive(true);
                        }

                        foreach (var interactionCinematic in interaction.cinematics)
                        {
                            interactionCinematic.SetActive(false);
                        }

                        if (interactions.Count > 0 && interaction.isContinue)
                        {
                            InteractIndex = (InteractIndex + 1) % interactions.Count;
                        }
                    }));

                    return;
                }
                else
                {
                    Debug.LogError("타임라인 세팅 오류");
                }
            }


            if (interactions.Count > 0 && interaction.isContinue)
            {
                InteractIndex = (InteractIndex + 1) % interactions.Count;
            }
        }

        private void OnDrawGizmos()
        {
            if (!useGizmos)
            {
                return;
            }

            Gizmos.color = gizmosColor;
            Gizmos.DrawWireSphere(transform.position, gizmosRadius);
        }

        private void TaskStart(string json)
        {
            var interaction = GetInteraction();
            if (interaction.jsonFile && interaction.interactionPlayType != InteractionPlayType.Task)
            {
                return;
            }

            Debug.Log("Task Stack 시작");
            interaction.serializedInteractionData.JsonTask = new Stack<TaskData>();
            PushTask(json);
        }

        private void StackNewTask(string jsonRoute)
        {
            DialogueController.Instance.taskData = null;
            GetInteraction().serializedInteractionData.JsonTask = new Stack<TaskData>();
            GC.Collect();
            PushTask(jsonRoute);
            StopAllCoroutines();
            StartInteraction();
        }

        private IEnumerator TaskCoroutine(int index = 0)
        {
            var interaction = GetInteraction(index);
            TaskData currentTaskData = interaction.serializedInteractionData.JsonTask.Peek();
            if (!currentTaskData.isContinue)
            {
                Debug.LogError("멈춘 상태에서 실행 시도함");
                yield break;
            }

            // 진행이 가능하면 true이며 true일 경우 같은 순서번호를 진행
            // 진행 중 멈췄다가 바로 실행되는 경우를 위해서
            // 진행 후 멈췄다가 한 번 더 클릭해야되는 경우 번호를 다음 번호로 하도록한다
            WaitUntil waitUntil = new WaitUntil(() => currentTaskData.isContinue);

            while(currentTaskData.tasks.Length > currentTaskData.taskIndex &&
                  currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) &&
                  currentTaskData.isContinue) //순서 번호 동일한 것 반복
            {
                DialogueController.Instance.taskData = currentTaskData;
                Task currentTask = currentTaskData.tasks[currentTaskData.taskIndex];
                Debug.Log("Task 길이: " + currentTaskData.tasks.Length);
                foreach (var task in currentTaskData.tasks)
                {
                    Debug.Log(task.taskContentType);
                }

                Debug.Log("taskIndex - " + currentTaskData.taskIndex + "\nInteractionType - " +
                          currentTask.taskContentType);
                switch(currentTask.taskContentType)
                {
                    case TaskContentType.Dialogue:
                        Debug.Log("대화 시작");
                        currentTaskData.isContinue = false;
                        string path = currentTask.nextFile;
                        Debug.Log($"대화 경로 - {path}");
                        string jsonString = DialogueController.Instance.ConvertPathToJson(path);
                        DialogueController.Instance.StartConversation(jsonString);
                        break;
                    case TaskContentType.Animation:
                        //세팅된 애니메이션 실행
                        interaction.animator.SetTrigger("Start");
                        Debug.Log("애니메이션 스타트");
                        StartCoroutine(WaitAnimationEnd(interaction));
                        break;
                    case TaskContentType.Play:
                        currentTaskData.isContinue = false;
                        IGamePlayable gamePlayable =
                            GameObject.Find(currentTask.nextFile).GetComponent<IGamePlayable>();
                        PlayUIController.Instance.SetMenuActive(false);
                        gamePlayable.OnEndPlay = () =>
                        {
                            PlayUIController.Instance.SetMenuActive(true);
                        };
                        gamePlayable.Play();
                        yield return new WaitUntil(() => gamePlayable.IsPlay);
                        PlayUIController.Instance.SetMenuActive(true);
                        currentTaskData.isContinue = true;
                        break;
                    case TaskContentType.Temp:
                        Debug.Log("선택지 열기");
                        currentTaskData.isContinue = false;
                        DialogueController.Instance.SetChoiceAction(ChoiceEvent);
                        DialogueController.Instance.OpenChoicePanel();
                        break;
                    case TaskContentType.TempDialogue:
                        //선택지에서 대화를 고른 경우
                        Debug.Log("선택지 선택 - 단순 대화");
                        interaction.serializedInteractionData.JsonTask.Peek().isContinue = true;
                        yield break;
                    // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                    // 애매하네
                    //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                    case TaskContentType.TaskReset:
                        if (interaction.interactionMethod == InteractionMethod.Touch)
                        {
                            interaction.serializedInteractionData.isInteracted = false;
                        }

                        interaction.serializedInteractionData.JsonTask.Pop();
                        if (interaction.serializedInteractionData.JsonTask.Count > 0)
                        {
                            Debug.LogError("Task 엑셀 관련 오류");
                        }

                        yield break;
                    case TaskContentType.New:
                    {
                        StackNewTask(currentTask.nextFile);
                        yield break;
                    }
                    case TaskContentType.FadeOut:
                    {
                        currentTaskData.isContinue = false;
                        FadeEffect.Instance.FadeOut();
                        yield return new WaitUntil(() => FadeEffect.Instance.IsFadeOver);
                        currentTaskData.isContinue = true;
                        break;
                    }
                    case TaskContentType.FadeIn:
                    {
                        currentTaskData.isContinue = false;
                        FadeEffect.Instance.FadeIn();
                        yield return new WaitUntil(() => FadeEffect.Instance.IsFadeOver);
                        currentTaskData.isContinue = true;

                        break;
                    }
                    case TaskContentType.TheEnd:
                        //게임 엔딩
                        if (Enum.TryParse(currentTask.nextFile, out EndingType endingType))
                        {
                            EndingHelper.Instance.StartEnd(endingType);
                        }
                        else
                        {
                            Debug.LogError($"엔딩 세팅 오류, {currentTask.nextFile}을 변경해주세요.");
                        }
                        break;
                    case TaskContentType.Cinematic:
                        if (interaction.timelines.Length == 0 || !interaction.timelines[0])
                        {
                            Debug.LogError("타임라인 오류");
                        }
                        currentTaskData.isContinue = false;

                        foreach (var interactionInGame in interaction.inGames)
                        {
                            interactionInGame.SetActive(false);
                        }
                        foreach (var interactionCinematic in interaction.cinematics)
                        {
                            interactionCinematic.SetActive(true);
                        }
                        interaction.timelines[0].Play();
                        JoystickController.Instance.StopSaveLoadJoyStick(true);
                        PlayUIController.Instance.SetMenuActive(false);

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
                        yield return new WaitUntil(() => Math.Abs(interaction.timelines[0].time - interaction.timelines[0].duration) <=
                                                         1 / ((TimelineAsset)interaction.timelines[0].playableAsset)
                                                         .editorSettings.fps ||
                                                         interaction.timelines[0].state == PlayState.Paused &&
                                                         !interaction.timelines[0].playableGraph.IsValid() &&
                                                         !DialogueController.Instance.IsTalking);

                        PlayUIController.Instance.SetMenuActive(true);
                        JoystickController.Instance.StopSaveLoadJoyStick(false);
                        currentTaskData.isContinue = true;
                        foreach (var cinematic in interaction.cinematics)
                        {
                            cinematic.SetActive(false);
                        }

                        foreach (var inGame in interaction.inGames)
                        {
                            inGame.SetActive(true);
                        }

                        break;
                    case TaskContentType.Clear:
                        DataController.Instance.CurrentMap.MapClear();
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
                Debug.Log(currentTaskData.tasks.Length > currentTaskData.taskIndex &&
                          currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) &&
                          currentTaskData.isContinue ? $"이어서 진행할 Index: {currentTaskData.taskIndex}" : "종료");
            }

            currentTaskData.taskOrder++;

            if (currentTaskData.tasks.Length == currentTaskData.taskIndex)
            {
                if (interaction.serializedInteractionData.JsonTask.Count > 1)
                {
                    //선택지인 경우
                    DialogueController.Instance.taskData = null; // null이 아닌 상태에서 모든 task가 끝나면 없어야되는데 남아있음
                    interaction.serializedInteractionData.JsonTask.Pop();
                    interaction.serializedInteractionData.JsonTask.Peek().isContinue = true;
                }
                else
                {
                    interaction.EndAction();
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


        private IEnumerator WaitAnimationEnd(Interaction interaction)
        {
            Debug.Log("기다리기");
            yield return new WaitUntil(() => interaction.animator.GetCurrentAnimatorStateInfo(0).IsName("Finish"));
            Debug.Log("기다리기 끝");
            var clipInfos = interaction.animator.GetCurrentAnimatorClipInfo(0);
            var alreadyDone = clipInfos.Any(item =>
            {
                return item.clip.events.Any(t => t.functionName == nameof(OnEndAnimation));
            });
            if (alreadyDone)
            {
                yield break;
            }
            foreach (var animatorClipInfo in clipInfos)
            {
                var animationEvent = new AnimationEvent
                {
                    time = animatorClipInfo.clip.length,
                    intParameter = interactions.FindIndex(item => item == interaction),
                    functionName = nameof(OnEndAnimation)
                };
                animatorClipInfo.clip.AddEvent(animationEvent);
            }
        }

        public void OnEndAnimation(int interactionIndex)
        {
            Debug.Log("실행");
            GetInteraction(interactionIndex).EndAction();
        }

        private static IEnumerator WaitTimeline(WaitUntil waitUntil, Action action)
        {
            yield return waitUntil;
            action?.Invoke();
        }

        public void Load(InteractionSaveData saveData)
        {
            Debug.Log(saveData);
            transform.position = saveData.pos;
            transform.rotation = saveData.rot;
            InteractIndex = saveData.interactIndex;

            foreach (var interaction in interactions)
            {
                var serializedData = saveData.serializedInteractionDatas.Find(item =>
                    item.id == interaction.serializedInteractionData.id);

                interaction.serializedInteractionData = serializedData;
            }
        }

        public Interaction GetInteraction(int index)
        {
            if (interactions?.Count > index)
            {
                return interactions[index];
            }

            Debug.LogError("인터랙션 데이터 설정 오류");
            return null;
        }

        public Interaction GetInteraction()
        {
            if (interactions?.Count > 0 && interactions.Count > InteractIndex)
            {
                return interactions[InteractIndex];
            }

            Debug.LogError("인터랙션 데이터 설정 오류");
            return null;
        }

        public InteractionSaveData GetInteractionSaveData()
        {
            var interactionSaveData = new InteractionSaveData
            {
                id = id,
                pos = transform.position,
                rot = transform.rotation,
                interactIndex = InteractIndex
            };
            interactionSaveData.serializedInteractionDatas = new List<SerializedInteractionData>();
            for(var index = 0; index < interactions.Count; index++)
            {
                var interaction = interactions[index];
                interaction.serializedInteractionData.id = index;
                interactionSaveData.serializedInteractionDatas.Add(interaction.serializedInteractionData);
            }

            return interactionSaveData;
        }

        bool IClickable.IsClickEnable
        {
            get
            {
                var interaction = GetInteraction();
                if ((interaction.interactionPlayType == InteractionPlayType.Dialogue ||
                     interaction.interactionPlayType == InteractionPlayType.Task) && !interaction.jsonFile)
                {
                    Debug.LogWarning("Json 세팅 오류");
                    return false;
                }
                Debug.Log($"Interactable: {interaction.serializedInteractionData.isInteractable}\n" +
                          $"Enable:  {enabled}\n" +
                          $"Method: {interaction.interactionMethod}\n" +
                          $"Is Interacted: {interaction.serializedInteractionData.isInteracted}");

                return interaction.serializedInteractionData.isInteractable && enabled &&
                       interaction.interactionMethod == InteractionMethod.Touch &&
                       !interaction.serializedInteractionData.isInteracted;
            }
            set
            {
                if (value)
                {
                    var interaction = GetInteraction();
                    interaction.interactionMethod = InteractionMethod.Touch;
                    interaction.serializedInteractionData.isInteracted = false;
                    interaction.serializedInteractionData.isInteractable = true;
                }
                else
                {
                    var interaction = GetInteraction();
                    interaction.interactionMethod = InteractionMethod.No;
                }
            }
        }

        bool IClickable.IsClicked
        {
            get
            {
                var interaction = GetInteraction();
                return interaction.serializedInteractionData.isInteracted;
            }
            set
            {
                var interaction = GetInteraction();
                interaction.serializedInteractionData.isInteracted = value;
            }
        }

        void IClickable.ActiveObjectClicker(bool isActive)
        {
            if (useOutline && outline)
            {
                outline.enabled = isActive;
            }

            Debug.Log(ExclamationMark);
            if (ExclamationMark)
            {
                ExclamationMark.SetActive(isActive);
            }

            if (GetInteraction().interactionMethod == InteractionMethod.Touch)
            {
                ObjectClicker.Instance.UpdateClick(this, isActive);
            }
        }

        bool IClickable.GetIsClicked()
        {
            var interaction = GetInteraction();
            return interaction.serializedInteractionData.isInteracted;
        }

        void IClickable.Click()
        {
            if (!((IClickable)this).IsClickEnable)
            {
                return;
            }

            ((IClickable)this).ActiveObjectClicker(false);

            StartInteraction();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || !ObjectClicker.Instance)
            {
                return;
            }

            ObjectClicker.Instance.UpdateClick(this, false);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying || !ObjectClicker.Instance)
            {
                return;
            }

            ObjectClicker.Instance.UpdateClick(this, false);
        }

        public void OnEnter()
        {
            Debug.Log("Enter");
            if (((IClickable)this).IsClickEnable)
            {
                ((IClickable)this).ActiveObjectClicker(true);
            }

            if (GetInteraction().interactionMethod == InteractionMethod.Trigger)
            {
                StartInteraction();
            }
        }

        public void OnExit()
        {
            Debug.Log("Exit");
            if (((IClickable)this).IsClickEnable)
            {
                ((IClickable)this).ActiveObjectClicker(false);
            }
        }
    }
}