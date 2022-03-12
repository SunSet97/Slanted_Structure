using System;
using System.Collections;
using System.Collections.Generic;
using Move;
using Play;
using UnityEngine;
using UnityEngine.Events;
using IPlayable = Play.IPlayable;


[Serializable]
public class InteractionEvent
{
    public enum EventType { NONE, CLEAR, ACTIVE, MOVE, PLAY }
    public EventType eventType;
    [Serializable]
    public struct Act
    {
        public GameObject activeObject;
        public bool activeSelf;
    }
    [Serializable]
    public struct Active
    {
        public Act[] actives;
    }
    [ConditionalHideInInspector("eventType", EventType.CLEAR)]
    public CheckMapClear clearBox;

    [ConditionalHideInInspector("eventType", EventType.ACTIVE)]
    public Active activeObjs;

    [ConditionalHideInInspector("eventType", EventType.MOVE)]
    public MovableList movables;
        
    [ConditionalHideInInspector("eventType", EventType.PLAY)]
    public PlayableList playableList;
}

// 인터렉션하는 오브젝트에 컴포넌트로 추가!!
[ExecuteInEditMode]
public class InteractionObj_stroke : MonoBehaviour
{
    public enum OutlineColor // 일단 내비둬
    {
        red,
        magenta,
        yellow,
        green,
        blue,
        grey,
        black,
        white
    }

    public enum InteractionPlayType
    {
        None = -1,
        Potal,
        Animation,
        Dialogue,
        CameraSetting,
        interact,
        Task,
        GAME
    }

    public enum InteractionMethod // 인터렉션 오브젝트 터치했는지 안했는지 감지 기능 필요
    {
        Touch,
        Trigger,
        No
    }

    [Header("인터렉션 방식")]
    public InteractionPlayType interactionPlayType;

    [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
    public TextAsset jsonFile;
    
    private Stack<TaskData> jsonTask;

    private UnityAction endDialogueAction;
    private UnityAction startDialogueAction;
    
    [ConditionalHideInInspector("interactionType", InteractionPlayType.Dialogue)]
    public DialogueData dialogueData;
    [ConditionalHideInInspector("interactionType", InteractionPlayType.Dialogue)]
    public InteractionEvent[] dialogueEndAction;

    [ConditionalHideInInspector("interactionType", InteractionPlayType.Task)]
    public InteractionEvent[] taskEndActions;

    [Header("디버깅 전용 TaskData")]
    [ConditionalHideInInspector("interactionType", InteractionPlayType.Task)]
    public List<TaskData> taskData_Debug;

    [Header("아웃라인 색 설정")]
    public OutlineColor color;

    [Header("인터렉션 오브젝트 터치 유무 감지 기능 사용할건지 말건지")]
    public InteractionMethod interactionMethod;
    public bool onOutline = false; // 아웃라인 켜져있는 안켜져 있는지
    public bool isCharacterInRange = false; // obj_interaction 오브젝트 기준으로 일정 범위 내에 캐릭터가 있는지 확인
    public int radius = 5;

    [Header("#Mark setting")]
    public GameObject exclamationMark;
    public Vector2 markOffset = Vector2.zero;

    [Header("느낌표 사용할 때 체크")]
    public bool useExclamationMark = false;
    private bool isInteractionObj = false;

    public Outline outline;
    private CharacterManager currentCharacter;
    public Camera cam;

    [Header("카메라 뷰")]
    public bool isViewChange = false;

    [Header("대화가 진행될 때의 카메라 세팅")]
    public Vector3 CameraPos;
    public Vector3 CameraRot;

    //[Header("터치 여부 확인")]
    private bool isTouched;
    private RaycastHit hit;

    [Header("실제로 터치되는 오브젝트. 만약 스크립트 적용된 오브젝트가 터치될 오브젝트라면 그냥 None인상태로 두기!")]
    public GameObject touchTargetObject;

    private void PushTask(string jsonString)
    {
        TaskData taskData = new TaskData
        {
            tasks = JsontoString.FromJsonArray<Task>(jsonString)
        };
        jsonTask.Push(taskData);
        DataController.instance_DataController.taskData = taskData;
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
            return;
        }
        currentCharacter = DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main);
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
    void SetBtnPress(int index)
    {
        TaskData currentTaskData = jsonTask.Peek();

        //변화값
        currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar = currentTaskData
            .tasks[currentTaskData.taskIndex + index].increaseVar.Replace("m", "-");
        int[] changeVal =
            Array.ConvertAll(currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar.Split(','),
                int.Parse);
        DataController.instance_DataController.charData.selfEstm += changeVal[0];
        DataController.instance_DataController.charData.intimacy_spRau += changeVal[1];
        DataController.instance_DataController.charData.intimacy_ounRau += changeVal[2];
        
        string path, jsonString;
        switch (currentTaskData.tasks[currentTaskData.taskIndex + index].type)
        {
            case CustomEnum.TYPE.DIALOGUE:
                int choiceLen = int.Parse(currentTaskData.tasks[currentTaskData.taskIndex].nextFile);
                currentTaskData.tasks[currentTaskData.taskIndex + choiceLen + 1].type = CustomEnum.TYPE.TempDialogueEnd;
                
                currentTaskData.isContinue = false;
                path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
                jsonString = (Resources.Load(path) as TextAsset)?.text;
                currentTaskData.taskIndex--;    // 현재 taskIndex는 선택지이며 선택지 다음 인덱스가 된다. 그런데 대화 종료시 Index가 1 증가하기에 1을 줄여준다.
                CanvasControl.instance_CanvasControl.StartConversation(jsonString);
                //다음 인덱스의 타입 변경
                break;
            case CustomEnum.TYPE.TEMP:
                //새로운 task 실행
                path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
                jsonString = (Resources.Load(path) as TextAsset)?.text;
                if (currentTaskData.tasks[currentTaskData.taskIndex].order != 0)
                {
                    DataController.instance_DataController.currentMap.mapCode =
                        $"{currentTaskData.tasks[currentTaskData.taskIndex].order, 000000}";
                }
                PushTask(jsonString);
                StartInteraction();
                break;
            case CustomEnum.TYPE.NEW:
                break;
        }
    }


    /// <summary>
    /// 인터랙션을 처음으로 실행할 때 (ex - 터치)
    /// </summary>
    protected void StartInteraction()
    {
        TaskStart();
        //3가지 1)애니메이션 2)대사 3)카메라 변환(확대라든지) 4)맵포탈
        if (interactionPlayType == InteractionPlayType.Animation && this.gameObject.GetComponent<Animator>() != null)//애니메이터가 존재한다면 
        {
            //세팅된 애니메이터 시작
            this.gameObject.GetComponent<Animator>().Play("Start", 0);

        }
        else if (interactionPlayType == InteractionPlayType.Dialogue)
        {
            isTouched = true;
            if (jsonFile)
            {
                if (startDialogueAction != null)
                {
                    CanvasControl.instance_CanvasControl.SetDialougueStartAction(startDialogueAction);
                }
                if (endDialogueAction != null)
                {
                    CanvasControl.instance_CanvasControl.SetDialougueEndAction(endDialogueAction);
                }
                foreach (InteractionEvent endAction in dialogueEndAction)
                {
                    switch (endAction.eventType)
                    {
                        case InteractionEvent.EventType.CLEAR:
                            Debug.Log($"DialogueEndAction {endAction.eventType}");
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() => endAction.clearBox.Clear());
                            break;
                        case InteractionEvent.EventType.ACTIVE:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (InteractionEvent.Act active in endAction.activeObjs.actives)
                                {
                                    Debug.Log($"DialogueEndAction {endAction.eventType} - " + active.activeSelf);
                                    active.activeObject.SetActive(active.activeSelf);
                                }
                            });
                            break;
                        case InteractionEvent.EventType.MOVE:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (MovableObj movable in endAction.movables.movables)
                                {
                                    Debug.Log($"DialogueEndAction {endAction.eventType} - " + movable.isMove);
                                    movable.gameObject.GetComponent<IMovable>().IsMove = movable.isMove;
                                }
                            });
                            break;
                        case InteractionEvent.EventType.PLAY:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (PlayableObj playable in endAction.playableList.playableObjs)
                                {
                                    Debug.Log($"DialogueEndAction {endAction.eventType} - " + playable.isPlay);
                                    playable.gameObject.GetComponent<IPlayable>().IsPlay = playable.isPlay;
                                }
                            });
                            break;
                    }
                }
                CanvasControl.instance_CanvasControl.StartConversation(jsonFile.text);
            }
            else
                Debug.LogError("json 파일 없는 오류");
        }
        else if (interactionPlayType == InteractionPlayType.CameraSetting)
        {
            //카메라 변환 활성화
        }
        else if (interactionPlayType == InteractionPlayType.Potal && this.gameObject.GetComponent<CheckMapClear>() != null)
        {
            DataController.instance_DataController.ChangeMap(DataController.instance_DataController.currentMap.nextMapcode);

        }
        //1회성 interaction인 경우 굳이 excel로 할 필요 없이 바로 실행 dialogue도 마찬가지 단순한 잡담이면 typeOfInteraction.dialogue에서 처리
        else if (interactionPlayType == InteractionPlayType.Task)
        {
            isTouched = true;
            if (jsonTask.Count == 0) { Debug.LogError("task파일 없음 오류오류"); }

            StartCoroutine(TaskCorutine());
        }
        else if (interactionPlayType == InteractionPlayType.interact && TryGetComponent(out CheckMapClear checkMapClear))
        {
            //애니메이션 재생 후 다음 맵으로 넘어가는 등의 인터렉션이 있을 때.
            if (TryGetComponent(out Animator animator))
            {
                animator.SetBool("Interation", true);
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Finish"))
                {
                    checkMapClear.Clear();
                }
            }
        }
    }
    void Update()
    {
        if (Application.isPlaying)
        {
            if (!isInteractionObj)
            {
                gameObject.tag = "obj_interaction";
                if (gameObject.tag.Equals("obj_interaction"))
                {
                    isInteractionObj = true;
                    outline = gameObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineWidth = 8f; // 아웃라인 두께 설정
                    outline.enabled = false; // 우선 outline 끄기

                    cam = DataController.instance_DataController.cam;

                    if (useExclamationMark && exclamationMark.gameObject != null) exclamationMark.SetActive(false); // 느낌표 끄기


                    if (!touchTargetObject)
                    {
                        touchTargetObject = gameObject;
                    }

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
            else
            {
                CheckAroundCharacter(); // 일정 범위 안에 선택된 캐릭터 있는지 확인
                if (exclamationMark && exclamationMark.activeSelf)
                    exclamationMark.transform.position = (Vector3)markOffset + DataController.instance_DataController.cam.WorldToScreenPoint(transform.position); // 마크 위치 설정
                if (isCharacterInRange && !isTouched)
                {
                    // 플레이어의 인터렉션 오브젝트 터치 감지
                    if (interactionMethod == InteractionMethod.Touch)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 13))
                            {
                                Debug.Log(hit.transform.gameObject);
                                if (hit.collider.gameObject.Equals(touchTargetObject))
                                {
                                    if (hit.collider.TryGetComponent(out CheckMapClear checkMapClear))

                                        if (!checkMapClear.nextSelectMapcode.Equals("000000"))
                                            DataController.instance_DataController.currentMap.nextMapcode = checkMapClear.nextSelectMapcode;

                                    StartInteraction();//인터렉션반응 나타남.
                                }

                            }
                        }
                    }

                    if (!outline.enabled)
                    {
                        // 아웃라인 켜기
                        onOutline = true;
                        outline.enabled = true;

                        if (useExclamationMark)
                        {
                            // 느낌표 보이게
                            exclamationMark.gameObject.SetActive(true);
                        }
                    }
                }
                else if ((!isCharacterInRange && onOutline) || isTouched) // 범위 밖이면서 아웃라인이 켜져있거나 눌렀을 경우
                {
                    // 아웃라인 끄기
                    onOutline = false;
                    outline.enabled = false;

                    if (useExclamationMark)
                    {
                        // 느낌표 안보이게
                        exclamationMark.gameObject.SetActive(false);
                    }
                }
            }
            /*
            Vector3 myScreenPos = cam.WorldToScreenPoint(transform.position);
            exclamationMark.transform.position = myScreenPos + new Vector3(x, y, 0);
            */
        }
    }
    private void CheckAroundCharacter()
    {
        //Layer 추가
        RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position, radius, Vector3.up, 0f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.Equals(currentCharacter.gameObject))
            {
                isCharacterInRange = true;

            }
            else
            {
                isCharacterInRange = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }

    private void TaskStart()
    {
        if (!jsonFile) return;
        if (interactionPlayType != InteractionPlayType.Task) return;
        if (jsonTask != null && jsonTask.Count != 0) return;
        
        jsonTask = new Stack<TaskData>();
        PushTask(jsonFile.text);
    }

    private IEnumerator TaskCorutine()
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

        while (currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) && currentTaskData.isContinue)     //순서 번호 동일한 것 반복
        {
            DataController.instance_DataController.taskData = currentTaskData;
            Task currentTask = currentTaskData.tasks[currentTaskData.taskIndex];
            Debug.Log("taskIndex - " + currentTaskData.taskIndex + "\nInteractionType - " + currentTask.type);
            switch (currentTask.type)
            {
                case CustomEnum.TYPE.DIALOGUE:
                    Debug.Log("대화 시작");
                    currentTaskData.isContinue = false;
                    string path = currentTask.nextFile;
                    string jsonString = (Resources.Load(path) as TextAsset)?.text;
                    Debug.Log($"대화 경로 - {path}");
                    CanvasControl.instance_CanvasControl.StartConversation(jsonString);
                    break;
                case CustomEnum.TYPE.ANIMATION:
                    //세팅된 애니메이션 실행
                    currentTaskData.taskIndex++;
                    gameObject.GetComponent<Animator>().Play("Start", 0);
                    break;
                case CustomEnum.TYPE.Play:
                    currentTaskData.isContinue = false; //디버깅용
                    IPlayable playable = GameObject.Find(currentTask.nextFile).GetComponent<IPlayable>();
                    playable.Play();
                    yield return new WaitUntil(() => playable.IsPlay);
                    currentTaskData.isContinue = true;  //디버깅용
                    break;
                case CustomEnum.TYPE.TEMP:
                    Debug.Log("선택지 열기");
                    currentTaskData.isContinue = false;
                    CanvasControl.instance_CanvasControl.SetChoiceAction(SetBtnPress);
                    CanvasControl.instance_CanvasControl.OpenChoicePanel();
                    break;
                case CustomEnum.TYPE.TEMPEND:
                    //Temp Task 끝날 때
                    Debug.Log("선택지 종료");
                    DataController.instance_DataController.taskData = null;
                    jsonTask.Pop();
                    jsonTask.Peek().isContinue = true;
                    yield break;
                case CustomEnum.TYPE.TempDialogueEnd:
                    //선택지에서 대화를 고른 경우
                    Debug.Log("선택지 종료 - 단순 대화");
                    //DataController.instance_DataController.taskData = null;
                    jsonTask.Peek().isContinue = true;
                    yield break;
                // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                // 애매하네
                //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                case CustomEnum.TYPE.TaskReset:
                    isTouched = false;
                    jsonTask.Pop();
                    if (jsonTask.Count > 0) Debug.LogError("Task 엑셀 관련 오류");
                    foreach (var taskEndAction in taskEndActions)
                    {
                        switch (taskEndAction.eventType)
                        {
                            case InteractionEvent.EventType.CLEAR:
                                taskEndAction.clearBox.Clear();
                                break;
                            case InteractionEvent.EventType.ACTIVE:
                                foreach (var t in taskEndAction.activeObjs.actives)
                                {
                                    Debug.Log($"DialogueEndAction {taskEndAction.eventType} - " + t.activeSelf);
                                    t.activeObject.SetActive(t.activeSelf);
                                }
                                break;
                            case InteractionEvent.EventType.MOVE:
                                foreach (var t in taskEndAction.movables.movables)
                                {
                                    Debug.Log($"DialogueEndAction {taskEndAction.eventType} - " + t.isMove);
                                    t.gameObject.GetComponent<IMovable>().IsMove = t.isMove;
                                }

                                break;
                            case InteractionEvent.EventType.PLAY:
                                foreach (var t in taskEndAction.playableList.playableObjs)
                                {
                                    Debug.Log($"DialogueEndAction {taskEndAction.eventType} - " + t.isPlay);
                                    t.gameObject.GetComponent<IPlayable>().IsPlay = t.isPlay;
                                }
                                break;
                        }
                    }
                    yield break;
                case CustomEnum.TYPE.NEW:
                    {
                        //
                        //End가 필요한가
                        break;
                    }
                case CustomEnum.TYPE.FadeOut:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeOut());
                    yield return new WaitUntil(() => FadeEffect.instance.isFade);
                    currentTaskData.isContinue = true;  //디버깅용

                    break;
                }
                case CustomEnum.TYPE.FadeIn:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeIn());
                    yield return new WaitWhile(() => FadeEffect.instance.isFade);
                    currentTaskData.isContinue = true;
                    break;
                }
                default:
                    Debug.LogError($"{currentTask.type}은 존재하지 않는 type입니다.");
                    break;
            }
            Debug.Log("Task 종료 대기 중 - " + currentTask.type + ", Index - " + currentTaskData.taskIndex);
            yield return waitUntil;
            Debug.Log("Task 종료 - " + currentTask.type + ", Index - " + currentTaskData.taskIndex);
        }
        currentTaskData.taskOrder++;
    }
    //For Debugging
    private void LoadTaskData()
    {
        if (taskData_Debug.Count != 0) return;

        taskData_Debug.Add(new TaskData
        {
            tasks = JsontoString.FromJsonArray<Task>(jsonFile.text)
        });
        foreach (TaskData taskData in taskData_Debug)
        {
            for (int i = 0; i < taskData.tasks.Length; i++)
            {
                if (taskData.tasks[i].type == CustomEnum.TYPE.NEW || taskData.tasks[i].type == CustomEnum.TYPE.TEMP)
                {
                    int count = int.Parse(taskData.tasks[i].nextFile);
                    for (int j = 1; j <= count; j++)
                    {
                        string path = taskData.tasks[i + j].nextFile;
                        string jsonString = (Resources.Load(path) as TextAsset)?.text;

                        TaskData data = new TaskData
                        {
                            tasks = JsontoString.FromJsonArray<Task>(jsonString)
                        };
                        if (taskData_Debug.Count > 50)
                        {
                            return;
                        }
                        taskData_Debug.Add(data);
                    }
                    i += count + 1;
                }
            }
        }
    }
}