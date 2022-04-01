﻿using System;
using System.Collections;
using System.Collections.Generic;
using Data;
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

    public void ClearEvent()
    {
        Debug.Log($"Clear Event - {clearBox.gameObject}");
        clearBox.Clear();
    }
    
    public void ActiveEvent()
    {
        foreach (var t in activeObjs.actives)
        {
            Debug.Log($"ACTIVE Event - {t.activeObject}: {t.activeSelf}");
            t.activeObject.SetActive(t.activeSelf);
        }
    }

    public void MoveEvent()
    {
        foreach (MovableObj t in movables.movables)
        {
            Debug.Log($"Move Event - {t.gameObject}: {t.isMove}");
            t.gameObject.GetComponent<IMovable>().IsMove = t.isMove;
        }
    }

    public void PlayEvent()
    {
        foreach (var t in playableList.playableObjs)
        {
            Debug.Log($"Play Event - {t.gameObject}: {t.isPlay} ");
            t.gameObject.GetComponent<IPlayable>().IsPlay = t.isPlay;
        }
    }
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

    [Header("인터렉션 방식")]
    public CustomEnum.InteractionPlayType interactionPlayType;

    [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
    public TextAsset jsonFile;
    
    private Stack<TaskData> jsonTask;

    private UnityAction endDialogueAction;
    private UnityAction startDialogueAction;
    
    [ConditionalHideInInspector("interactionType", CustomEnum.InteractionPlayType.Dialogue)]
    public DialogueData dialogueData;
    [ConditionalHideInInspector("interactionType", CustomEnum.InteractionPlayType.Dialogue)]
    public InteractionEvent[] dialogueEndAction;

    [ConditionalHideInInspector("interactionType", CustomEnum.InteractionPlayType.Task)]
    public InteractionEvent[] taskEndActions;

    [Header("디버깅 전용 TaskData")]
    [ConditionalHideInInspector("interactionType", CustomEnum.InteractionPlayType.Task)]
    public List<TaskData> taskData_Debug;

    [Header("아웃라인 색 설정")]
    public OutlineColor color;

    [Header("인터렉션 오브젝트 터치 유무 감지 기능 사용할건지 말건지")]
    public CustomEnum.InteractionMethod interactionMethod;
    public int radius = 5;

    [Header("#Mark setting")]
    public GameObject exclamationMark;
    public Vector2 markOffset = Vector2.zero;

    [Header("느낌표 사용할 때 체크")]
    public bool useExclamationMark = false;
    private bool isInteractionObj = false;

    public Outline outline;

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
        DataController.instance.taskData = taskData;
    }

    protected virtual void Start()
    {
        if (!Application.isPlaying)
        {
            if (jsonFile != null)
            {
                if (interactionPlayType == CustomEnum.InteractionPlayType.Dialogue)
                    dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
                else if (interactionPlayType == CustomEnum.InteractionPlayType.Task)
                    LoadTaskData();
            }
        }
        else
        {
            //딱히 필요 없음
            gameObject.tag = "obj_interaction";
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

            if (useExclamationMark && exclamationMark.gameObject != null) exclamationMark.SetActive(false); // 느낌표 끄기


            // if (!touchTargetObject)
            // {
            //     touchTargetObject = gameObject;
            // }

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
        TaskData currentTaskData = jsonTask.Peek();

        //변화값
        currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar = currentTaskData
            .tasks[currentTaskData.taskIndex + index].increaseVar.Replace("m", "-");
        int[] changeVal =
            Array.ConvertAll(currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar.Split(','),
                int.Parse);
        DataController.instance.charData.selfEstm += changeVal[0];
        DataController.instance.charData.intimacy_spRau += changeVal[1];
        DataController.instance.charData.intimacy_ounRau += changeVal[2];
        
        string path, jsonString;
        switch (currentTaskData.tasks[currentTaskData.taskIndex + index].taskContentType)
        {
            case CustomEnum.TaskContentType.DIALOGUE:
                int choiceLen = int.Parse(currentTaskData.tasks[currentTaskData.taskIndex].nextFile);
                currentTaskData.tasks[currentTaskData.taskIndex + choiceLen + 1].taskContentType = CustomEnum.TaskContentType.TempDialogue;
                
                currentTaskData.isContinue = false;
                path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
                jsonString = (Resources.Load(path) as TextAsset)?.text;
                currentTaskData.taskIndex--;    // 현재 taskIndex는 선택지이며 선택지 다음 인덱스가 된다. 그런데 대화 종료시 Index가 1 증가하기에 1을 줄여준다.
                CanvasControl.instance.StartConversation(jsonString);
                //다음 인덱스의 타입 변경
                break;
            case CustomEnum.TaskContentType.TEMP:
                //새로운 task 실행
                path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
                jsonString = (Resources.Load(path) as TextAsset)?.text;
                if (currentTaskData.tasks[currentTaskData.taskIndex].order != 0)
                {
                    DataController.instance.currentMap.mapCode =
                        $"{currentTaskData.tasks[currentTaskData.taskIndex].order, 000000}";
                }
                PushTask(jsonString);
                StartInteraction();
                break;
            case CustomEnum.TaskContentType.NEW:
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
        if (interactionPlayType == CustomEnum.InteractionPlayType.Animation && this.gameObject.GetComponent<Animator>() != null)//애니메이터가 존재한다면 
        {
            //세팅된 애니메이터 시작
            gameObject.GetComponent<Animator>().Play("Start", 0);

        }
        else if (interactionPlayType == CustomEnum.InteractionPlayType.Dialogue)
        {
            isTouched = true;
            if (jsonFile)
            {
                if (startDialogueAction != null)
                {
                    CanvasControl.instance.SetDialougueStartAction(startDialogueAction);
                }

                if (endDialogueAction != null)
                {
                    CanvasControl.instance.SetDialougueEndAction(endDialogueAction);
                }

                foreach (InteractionEvent endAction in dialogueEndAction)
                {
                    switch (endAction.eventType)
                    {
                        case InteractionEvent.EventType.CLEAR:
                            CanvasControl.instance.SetDialougueEndAction(() => endAction.ClearEvent());
                            break;
                        case InteractionEvent.EventType.ACTIVE:
                            CanvasControl.instance.SetDialougueEndAction(() =>
                            {
                                endAction.ActiveEvent();
                            });
                            break;
                        case InteractionEvent.EventType.MOVE:
                            CanvasControl.instance.SetDialougueEndAction(() =>
                            {
                                endAction.MoveEvent();
                            });
                            break;
                        case InteractionEvent.EventType.PLAY:
                            CanvasControl.instance.SetDialougueEndAction(() =>
                            {
                                endAction.PlayEvent();
                            });
                            break;
                    }
                }
                CanvasControl.instance.StartConversation(jsonFile.text);
            }
            else
                Debug.LogError("json 파일 없는 오류");
        }
        else if (interactionPlayType == CustomEnum.InteractionPlayType.CameraSetting)
        {
            //카메라 변환 활성화
        }
        else if (interactionPlayType == CustomEnum.InteractionPlayType.Potal && gameObject.TryGetComponent(out CheckMapClear mapClear))
        {
            mapClear.Clear();
        }
        //1회성 interaction인 경우 굳이 excel로 할 필요 없이 바로 실행 dialogue도 마찬가지 단순한 잡담이면 typeOfInteraction.dialogue에서 처리
        else if (interactionPlayType == CustomEnum.InteractionPlayType.Task)
        {
            isTouched = true;
            if (jsonTask.Count == 0) { Debug.LogError("task파일 없음 오류오류"); }

            StartCoroutine(TaskCoroutine());
        }
        else if (interactionPlayType == CustomEnum.InteractionPlayType.Interact && TryGetComponent(out CheckMapClear checkMapClear))
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
    
    // private void InteractionEvent
    void Update()
    {
        if (Application.isPlaying)
        {
            var cam = DataController.instance.cam;
            var isCharacterInRange = CheckAroundCharacter(); // 일정 범위 안에 선택된 캐릭터 있는지 확인
            if (exclamationMark && exclamationMark.activeSelf)
                exclamationMark.transform.position =
                    (Vector3) markOffset + cam.WorldToScreenPoint(transform.position); // 마크 위치 설정
            if (isCharacterInRange && !isTouched)
            {
                // 플레이어의 인터렉션 오브젝트 터치 감지
                if (interactionMethod == CustomEnum.InteractionMethod.Touch)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 13))
                        {
                            Debug.Log($"터치된 오브젝트: {hit.transform.gameObject}");
                            StartInteraction(); //인터렉션반응 나타남.
                        }
                    }
                }
                if (!outline.enabled)
                {
                    // 아웃라인 켜기
                    outline.enabled = true;

                    if (useExclamationMark)
                    {
                        // 느낌표 보이게
                        exclamationMark.gameObject.SetActive(true);
                    }
                }
            }
            else if ((!isCharacterInRange && outline.enabled) || isTouched) // 범위 밖이면서 아웃라인이 켜져있거나 눌렀을 경우
            {
                // 아웃라인 끄기
                outline.enabled = false;

                if (useExclamationMark)
                {
                    // 느낌표 안보이게
                    exclamationMark.gameObject.SetActive(false);
                }
            }
            /*
            Vector3 myScreenPos = cam.WorldToScreenPoint(transform.position);
            exclamationMark.transform.position = myScreenPos + new Vector3(x, y, 0);
            */
        }
    }
    private bool CheckAroundCharacter()
    {
        var curChar = DataController.instance.GetCharacter(MapData.Character.Main);
        //Layer 추가
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.Equals(curChar.gameObject))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void TaskStart()
    {
        if (!jsonFile) return;
        if (interactionPlayType != CustomEnum.InteractionPlayType.Task) return;
        if (jsonTask != null && jsonTask.Count != 0) return;
        
        jsonTask = new Stack<TaskData>();
        PushTask(jsonFile.text);
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

        while (currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) && currentTaskData.isContinue)     //순서 번호 동일한 것 반복
        {
            DataController.instance.taskData = currentTaskData;
            Task currentTask = currentTaskData.tasks[currentTaskData.taskIndex];
            Debug.Log("taskIndex - " + currentTaskData.taskIndex + "\nInteractionType - " + currentTask.taskContentType);
            switch (currentTask.taskContentType)
            {
                case CustomEnum.TaskContentType.DIALOGUE:
                    Debug.Log("대화 시작");
                    currentTaskData.isContinue = false;
                    string path = currentTask.nextFile;
                    string jsonString = (Resources.Load(path) as TextAsset)?.text;
                    Debug.Log($"대화 경로 - {path}");
                    CanvasControl.instance.StartConversation(jsonString);
                    break;
                case CustomEnum.TaskContentType.ANIMATION:
                    //세팅된 애니메이션 실행
                    currentTaskData.taskIndex++;
                    GetComponent<Animator>().Play("Start", 0);
                    break;
                case CustomEnum.TaskContentType.Play:
                    currentTaskData.isContinue = false;
                    IPlayable playable = GameObject.Find(currentTask.nextFile).GetComponent<IPlayable>();
                    playable.Play();
                    yield return new WaitUntil(() => playable.IsPlay);
                    currentTaskData.isContinue = true;
                    break;
                case CustomEnum.TaskContentType.TEMP:
                    Debug.Log("선택지 열기");
                    currentTaskData.isContinue = false;
                    CanvasControl.instance.SetChoiceAction(ChoiceEvent);
                    CanvasControl.instance.OpenChoicePanel();
                    break;
                case CustomEnum.TaskContentType.TEMPEND:
                    //Temp Task 끝날 때
                    Debug.Log("선택지 종료");
                    DataController.instance.taskData = null;     // null이 아닌 상태에서 모든 task가 끝나면 없어야되는데 남아있음 
                    jsonTask.Pop();
                    jsonTask.Peek().isContinue = true;
                    yield break;
                case CustomEnum.TaskContentType.TempDialogue:
                    //선택지에서 대화를 고른 경우
                    Debug.Log("선택지 선택 - 단순 대화");
                    jsonTask.Peek().isContinue = true;
                    yield break;
                // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                // 애매하네
                //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                case CustomEnum.TaskContentType.TaskReset:
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
                case CustomEnum.TaskContentType.NEW:
                {
                    //
                    //End가 필요한가
                    break;
                }
                case CustomEnum.TaskContentType.FadeOut:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeOut());
                    yield return new WaitUntil(() => FadeEffect.instance.isFade);
                    currentTaskData.isContinue = true;
                    break;
                }
                case CustomEnum.TaskContentType.FadeIn:
                {
                    currentTaskData.isContinue = false;
                    StartCoroutine(FadeEffect.instance.FadeIn());
                    yield return new WaitUntil(() => FadeEffect.instance.isFade);
                    currentTaskData.isContinue = true;
                    break;
                }
                default:
                {
                    Debug.LogError($"{currentTask.taskContentType}은 존재하지 않는 type입니다.");
                    break;
                }
            }
            Debug.Log("Task 종료 대기 중 - " + currentTask.taskContentType + ", Index - " + currentTaskData.taskIndex);
            yield return waitUntil;
            Debug.Log("Task 종료 - " + currentTask.taskContentType + ", Index - " + currentTaskData.taskIndex);
        }
        currentTaskData.taskOrder++;    //인터랙션을 두 차례 비연속적으로 진행하는 경우
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
            for (int i = 0; i < taskData.tasks.Length; i++)
            {
                if (taskData.tasks[i].taskContentType == CustomEnum.TaskContentType.NEW || taskData.tasks[i].taskContentType == CustomEnum.TaskContentType.TEMP)
                {
                    int count = int.Parse(taskData.tasks[i].nextFile);
                    for (int j = 1; j <= count; j++)
                    {
                        string path = taskData.tasks[i + j].nextFile;
                        string jsonString = (Resources.Load(path) as TextAsset)?.text;

                        if (taskData_Debug.Count > 50)
                        {
                            return;
                        }
                        taskData_Debug.Add(new TaskData
                        {
                            tasks = JsontoString.FromJsonArray<Task>(jsonString)
                        });
                    }
                    i += count + 1;
                }
            }
        }
    }
}