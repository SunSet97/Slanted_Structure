using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using CommonScript;
using Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using IPlayable = Play.IPlayable;
using Task = Data.Task;
using static Data.CustomEnum;

// 인터렉션하는 오브젝트에 컴포넌트로 추가!!
[ExecuteInEditMode]
public class InteractionObj_stroke : MonoBehaviour, IClickable
{
    [Header("인터렉션 방식")] public InteractionPlayType interactionPlayType;

    [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
    public TextAsset jsonFile;

    [Header("타입이 Game인 경우 게임 오브젝트를 넣으세요")] public IPlayable playableGame;

    private Stack<TaskData> jsonTask;

    private UnityAction endDialogueAction;
    private UnityAction startDialogueAction;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
    public DialogueData dialogueData;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
    public InteractionEvent[] dialogueEndAction;

    // [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
    // public InteractionEventMedium newDialogueEndAction;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
    public Vector3 dialogueCameraPos;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Dialogue)]
    public Vector3 dialogueCameraRot;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Task)]
    public InteractionEvent[] taskEndActions;
    // [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Task)]
    // public InteractionEventMedium newTaskEndActions;

    [ConditionalHideInInspector("interactionPlayType", InteractionPlayType.Cinematic)] [SerializeField]
    public InteractionEventMedium cinematicEndAction;

    [ConditionalHideInInspector("InteractionPlayType", InteractionPlayType.Task)] [Header("디버깅 전용 TaskData")]
    public List<TaskData> taskData_Debug;


    [Header("아웃라인 색 설정")] public OutlineColor color;

    [Header("인터렉션 오브젝트 터치 유무 감지 기능 사용할건지 말건지")]
    public InteractionMethod interactionMethod;

    public int radius = 5;

    [Header("#Mark setting")] public GameObject exclamationMark;
    public Vector2 markOffset = Vector2.zero;

    [Header("느낌표 사용할 때 체크")] public bool useExclamationMark = false;
    private bool isInteractionObj = false;

    public Outline outline;
    private bool isCharIn;

    [Header("카메라 뷰")] public bool isViewChange = false;

    [Header("타임라인")] public PlayableDirector timeline;

    public GameObject[] cinematics;
    public GameObject[] inGames;

    //[Header("터치 여부 확인")]
    private bool isTouched;
    private RaycastHit hit;

    [Header("실제로 터치되는 오브젝트. 만약 스크립트 적용된 오브젝트가 터치될 오브젝트라면 그냥 None인상태로 두기!")]
    public GameObject touchTargetObject;

    public bool isLoopDialogue;

    private int playerLayer;
    private void PushTask(string jsonString)
    {
        Debug.Log("Push Task");
        TaskData taskData = new TaskData
        {
            tasks = JsontoString.FromJsonArray<Task>(jsonString)
        };
        jsonTask.Push(taskData);
        DataController.instance.taskData = taskData;
    }

    protected virtual void Start()
    {
        if (!Application.isPlaying)
        {
            if (jsonFile != null)
            {
                if (interactionPlayType == InteractionPlayType.Dialogue)
                    dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
                else if (interactionPlayType == InteractionPlayType.Task)
                    LoadTaskData();
            }
        }
        else
        {
            playerLayer = 1 << LayerMask.NameToLayer("Player");
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");

            if (timeline)
            {
                var timelineAsset = timeline.playableAsset as TimelineAsset;
                var tracks = timelineAsset.GetOutputTracks();
                foreach (var temp in tracks)
                {
                    if (temp is CinemachineTrack)
                        timeline.SetGenericBinding(temp, DataController.instance.cam.GetComponent<CinemachineBrain>());
                }
            }

            //딱히 필요 없음
            // gameObject.tag = "obj_interaction";
            // if (!Application.isPlaying)
            // {
            //     Debug.Log("ㅎㅇ");
            //     if (TryGetComponent(out Outline t))
            //     {
            //         outline = t;
            //     }
            //     else
            //     {
            //         outline = gameObject.AddComponent<Outline>();
            //     }
            //
            //     outline.OutlineMode = Outline.Mode.OutlineAll;
            //     outline.OutlineWidth = 8f; // 아웃라인 두께 설정
            //     outline.enabled = false; // 우선 outline 끄기
            //     // 아웃라인 색깔 설정
            //     if (color == OutlineColor.red) outline.OutlineColor = Color.red;
            //     else if (color == OutlineColor.magenta) outline.OutlineColor = Color.magenta;
            //     else if (color == OutlineColor.yellow) outline.OutlineColor = Color.yellow;
            //     else if (color == OutlineColor.green) outline.OutlineColor = Color.green;
            //     else if (color == OutlineColor.blue) outline.OutlineColor = Color.blue;
            //     else if (color == OutlineColor.grey) outline.OutlineColor = Color.grey;
            //     else if (color == OutlineColor.black) outline.OutlineColor = Color.black;
            //     else if (color == OutlineColor.white) outline.OutlineColor = Color.white;
            // }

            if (useExclamationMark && exclamationMark.gameObject != null) exclamationMark.SetActive(false); // 느낌표 끄기
        }
    }

    /// <summary>
    /// Dialogue가 시작할 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
    /// </summary>
    /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
    public void SetDialogueStartEvent(UnityAction unityAction)
    {
        startDialogueAction += unityAction;
    }

    /// <summary>
    /// Dialogue가 끝날 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
    /// </summary>
    /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
    public void SetDialogueEndEvent(UnityAction unityAction)
    {
        endDialogueAction += unityAction;
    }

    /// <summary>
    /// 선택지를 눌렀을 때 부르는 함수, Task에서는 실행이 된다
    /// </summary>
    /// <param name="index">선택지 번호, 1번부터 시작</param>
    void ChoiceEvent(int index)
    {
        Debug.Log(jsonTask);
        Debug.Log(jsonTask.Count);
        TaskData currentTaskData = jsonTask.Peek();
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
                jsonString = (Resources.Load(path) as TextAsset)?.text;
                // currentTaskData.taskIndex--;    // 현재 taskIndex는 선택지이며 선택지 다음 인덱스가 된다. 그런데 대화 종료시 Index가 1 증가하기에 1을 줄여준다.
                DialogueController.instance.StartConversation(jsonString);
                //다음 인덱스의 타입
                break;
            case TaskContentType.TEMP:
                //새로운 task 실행
                path = curTask.nextFile;
                jsonString = (Resources.Load(path) as TextAsset)?.text;
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
        if (jsonTask == null)
        {
            TaskStart();
        }

        //3가지 1)애니메이션 2)대사 3)카메라 변환(확대라든지) 4)맵포탈
        if (interactionPlayType == InteractionPlayType.Animation &&
            gameObject.GetComponent<Animator>() != null) //애니메이터가 존재한다면 
        {
            //세팅된 애니메이터 시작
            gameObject.GetComponent<Animator>().Play("Start", 0);

        }
        else if (interactionPlayType == InteractionPlayType.Dialogue)
        {
            if (CanvasControl.instance.isInConverstation) return;
            
            if (!isLoopDialogue)
                isTouched = true;
            if (jsonFile)
            {
                if (startDialogueAction != null)
                {
                    DialogueController.instance.SetDialouguePrevAction(startDialogueAction);
                }

                if (endDialogueAction != null)
                {
                    DialogueController.instance.SetDialougueEndAction(endDialogueAction);
                }

                foreach (InteractionEvent endAction in dialogueEndAction)
                {
                    switch (endAction.eventType)
                    {
                        case InteractionEvent.EventType.CLEAR:
                            DialogueController.instance.SetDialougueEndAction(() =>
                                endAction.ClearEvent());
                            break;
                        case InteractionEvent.EventType.ACTIVE:
                            DialogueController.instance.SetDialougueEndAction(() => { endAction.ActiveEvent(); });
                            break;
                        case InteractionEvent.EventType.MOVE:
                            DialogueController.instance.SetDialougueEndAction(() => { endAction.MoveEvent(); });
                            break;
                        case InteractionEvent.EventType.PLAY:
                            DialogueController.instance.SetDialougueEndAction(() => { endAction.PlayEvent(); });
                            break;
                    }
                }

                DialogueController.instance.StartConversation(jsonFile.text);
            }
            else
                Debug.LogError("json 파일 없는 오류");
        }
        else if (interactionPlayType == InteractionPlayType.Potal &&
                 gameObject.TryGetComponent(out CheckMapClear mapClear))
        {
            mapClear.Clear();
        }
        //1회성 interaction인 경우 굳이 excel로 할 필요 없이 바로 실행 dialogue도 마찬가지 단순한 잡담이면 typeOfInteraction.dialogue에서 처리
        else if (interactionPlayType == InteractionPlayType.Task)
        {
            isTouched = true;
            if (jsonTask.Count == 0)
            {
                Debug.LogError("task파일 없음 오류오류");
            }

            StartCoroutine(TaskCoroutine());
        }
        else if (interactionPlayType == InteractionPlayType.Game)
        {
            playableGame.Play();
        }
        else if (interactionPlayType == InteractionPlayType.Cinematic)
        {
            timeline.Play();
            timeline.stopped += director =>
            {
                Debug.Log("타임라인 끝");
                foreach (var endAction in cinematicEndAction.interactionEvents)
                {
                    Debug.Log(endAction.eventType);
                    switch (endAction.eventType)
                    {
                        case InteractionEvent.EventType.CLEAR:
                            endAction.ClearEvent();
                            break;
                        case InteractionEvent.EventType.ACTIVE:
                            endAction.ActiveEvent();
                            break;
                        case InteractionEvent.EventType.MOVE:
                            endAction.MoveEvent();
                            break;
                        case InteractionEvent.EventType.PLAY:
                            endAction.PlayEvent();
                            break;
                    }
                }
            };
        }
    }

    // private void InteractionEvent
    void Update()
    {
        if (Application.isPlaying)
        {
            var cam = DataController.instance.cam;
            //Update말고 Collider로 체크
            var isAround = CheckAroundCharacter(); // 일정 범위 안에 선택된 캐릭터 있는지 확인
            if (exclamationMark && exclamationMark.activeSelf)
                exclamationMark.transform.position =
                    (Vector3) markOffset + cam.WorldToScreenPoint(transform.position); // 마크 위치 설정
            if (isAround && !isTouched)
            {
                if (!isCharIn)
                {
                    ((IClickable) this).ActiveObjectClicker(true);
                    // 아웃라인 켜기
                    if (outline)
                    {
                        outline.enabled = true;
                    }

                    isCharIn = true;
                    if (useExclamationMark)
                    {
                        // 느낌표 보이게
                        exclamationMark.gameObject.SetActive(true);
                    }
                }
            }
            else if (!isAround && isCharIn || isTouched) // 범위 밖이면서 아웃라인이 켜져있거나 눌렀을 경우
            {
                if (!isCharIn) return;
                // 아웃라인 끄기
                isCharIn = false;
                if (outline)
                {
                    outline.enabled = false;
                }

                ((IClickable) this).ActiveObjectClicker(false);
                if (useExclamationMark)
                {
                    // 느낌표 안보이게
                    exclamationMark.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool CheckAroundCharacter()
    {
        return Physics.CheckSphere(transform.position, radius, playerLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void TaskStart()
    {
        if (!jsonFile) return;
        if (interactionPlayType != InteractionPlayType.Task) return;
        Debug.Log("Task Stack 시작");
        jsonTask = new Stack<TaskData>();
        PushTask(jsonFile.text);
    }

    private void StackNewTask(string jsonRoute)
    {
        DataController.instance.taskData = null;
        jsonTask = new Stack<TaskData>();
        GC.Collect();
        PushTask(jsonRoute);
        StopAllCoroutines();
        StartInteraction();
    }

    private IEnumerator TaskCoroutine()
    {
        TaskData currentTaskData = jsonTask.Peek();
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
            DataController.instance.taskData = currentTaskData;
            Task currentTask = currentTaskData.tasks[currentTaskData.taskIndex];
            Debug.Log("taskIndex - " + currentTaskData.taskIndex + "\nInteractionType - " +
                      currentTask.taskContentType);
            switch (currentTask.taskContentType)
            {
                case TaskContentType.DIALOGUE:
                    Debug.Log("대화 시작");
                    currentTaskData.isContinue = false;
                    string path = currentTask.nextFile;
                    string jsonString = (Resources.Load(path) as TextAsset)?.text;
                    Debug.Log($"대화 경로 - {path}");
                    DialogueController.instance.StartConversation(jsonString);
                    break;
                case TaskContentType.ANIMATION:
                    //세팅된 애니메이션 실행
                    GetComponent<Animator>().Play("Start", 0);
                    break;
                case TaskContentType.Play:
                    currentTaskData.isContinue = false;
                    IPlayable playable = GameObject.Find(currentTask.nextFile).GetComponent<IPlayable>();
                    playable.Play();
                    yield return new WaitUntil(() => playable.IsPlay);
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
                    jsonTask.Peek().isContinue = true;
                    yield break;
                // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                // 애매하네
                //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                case TaskContentType.TaskReset:
                    isTouched = false;
                    jsonTask.Pop();
                    if (jsonTask.Count > 0) Debug.LogError("Task 엑셀 관련 오류");
                    foreach (var taskEndAction in taskEndActions)
                    {
                        switch (taskEndAction.eventType)
                        {
                            case InteractionEvent.EventType.CLEAR:
                                taskEndAction.ClearEvent();
                                break;
                            case InteractionEvent.EventType.ACTIVE:
                                taskEndAction.ActiveEvent();
                                break;
                            case InteractionEvent.EventType.MOVE:
                                taskEndAction.MoveEvent();
                                break;
                            case InteractionEvent.EventType.PLAY:
                                taskEndAction.PlayEvent();
                                break;
                        }
                    }

                    yield break;
                case TaskContentType.NEW:
                {
                    StackNewTask(currentTask.nextFile);
                    yield break;
                }
                case TaskContentType.FadeOut:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeOut());
                    yield return new WaitUntil(() => FadeEffect.instance.isFadeOver);
                    FadeEffect.instance.isFadeOver = false;
                    currentTaskData.isContinue = true;
                    break;
                }
                case TaskContentType.FadeIn:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeIn());
                    yield return new WaitUntil(() => FadeEffect.instance.isFadeOver);
                    FadeEffect.instance.isFadeOver = false;
                    currentTaskData.isContinue = true;
                    break;
                }
                case TaskContentType.THEEND:
                    //게임 엔딩
                    break;
                case TaskContentType.Cinematic:
                    currentTaskData.isContinue = false;
                    foreach (var cinematic in cinematics)
                    {
                        cinematic.SetActive(true);
                    }

                    foreach (var inGame in inGames)
                    {
                        inGame.SetActive(false);
                    }

                    DataController.instance.StopSaveLoadJoyStick(true);
                    timeline.Pause();
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

                    
                    yield return new WaitUntil(() => timeline.state == PlayState.Paused && !CanvasControl.instance.isInConverstation);
                    DataController.instance.StopSaveLoadJoyStick(false);
                    currentTaskData.isContinue = true;
                    foreach (var cinematic in cinematics)
                    {
                        cinematic.SetActive(false);
                    }

                    foreach (var inGame in inGames)
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
            if (jsonTask.Count > 1)
            {
                //선택지인 경우
                DataController.instance.taskData = null; // null이 아닌 상태에서 모든 task가 끝나면 없어야되는데 남아있음
                jsonTask.Pop();
                jsonTask.Peek().isContinue = true;
            }
            else
            {
                foreach (var taskEndAction in taskEndActions)
                {
                    switch (taskEndAction.eventType)
                    {
                        case InteractionEvent.EventType.CLEAR:
                            taskEndAction.ClearEvent();
                            break;
                        case InteractionEvent.EventType.ACTIVE:
                            taskEndAction.ActiveEvent();
                            break;
                        case InteractionEvent.EventType.MOVE:
                            taskEndAction.MoveEvent();
                            break;
                        case InteractionEvent.EventType.PLAY:
                            taskEndAction.PlayEvent();
                            break;
                    }
                }
            }
        }
    }

    //For Debugging
    private void LoadTaskData()
    {
        if (taskData_Debug.Count != 0) return;
        taskData_Debug = new List<TaskData>
        {
            new TaskData
            {
                tasks = JsontoString.FromJsonArray<Task>(jsonFile.text)
            }
        };
        foreach (TaskData taskData in taskData_Debug)
        {
            Debug.Log("task 길이" + taskData.tasks.Length);
            for (int i = 0; i < taskData.tasks.Length; i++)
            {
                if (taskData.tasks[i].taskContentType == TaskContentType.NEW ||
                    taskData.tasks[i].taskContentType == TaskContentType.TEMP)
                {
                    Debug.Log("디버그 Task 추가");
                    var count = int.Parse(taskData.tasks[i].nextFile);
                    for (var j = 0; j < count; j++)
                    {
                        i++;
                        string path = taskData.tasks[i].nextFile;
                        string jsonString = (Resources.Load(path) as TextAsset)?.text;

                        if (taskData_Debug.Count > 50)
                        {
                            Debug.Log("무한 task 디버깅");
                            return;
                        }

                        taskData_Debug.Add(new TaskData
                        {
                            tasks = JsontoString.FromJsonArray<Task>(jsonString)
                        });
                    }

                    Debug.Log("디버그 Task 추가" + taskData_Debug.Count);
                }
            }
        }
    }

    bool IClickable.IsClickEnable
    {
        get => interactionMethod == InteractionMethod.Touch && !isTouched;
        set => interactionMethod = InteractionMethod.No;
    }

    bool IClickable.IsClicked
    {
        get => isTouched;
        set => isTouched = value;
    }

    void IClickable.ActiveObjectClicker(bool isActive)
    {
        if(interactionMethod == InteractionMethod.Touch)
            ObjectClicker.instance.UpdateClick(this, isActive);
    }

    bool IClickable.GetIsClicked()
    {
        return isTouched;
    }

    void IClickable.Click()
    {
        Debug.Log(((IClickable) this).IsClickEnable);
        if (!((IClickable) this).IsClickEnable) return;
        StartInteraction(); //인터렉션반응 나타남.
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (!ObjectClicker.instance) return;
        ObjectClicker.instance.UpdateClick(this, false);
    }

    void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (!ObjectClicker.instance) return;
        ObjectClicker.instance.UpdateClick(this, false);
    }
}