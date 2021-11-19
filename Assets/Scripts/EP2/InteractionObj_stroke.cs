using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public enum typeOfInteraction
    {
        POTAL,
        animation,
        DIALOGUE,
        camerasetting,
        interact,
        TASK,
        GAME
    }
    public enum TouchOrNot // 인터렉션 오브젝트 터치했는지 안했는지 감지 기능 필요
    {
        yes,
        no
    }
    [Serializable]
    public class InteractionEvent
    {
        public enum TYPE { NONE, CLEAR, ACTIVE, MOVE, PLAY }
        public TYPE eventType;
        [Serializable]
        public struct act
        {
            public GameObject ActiveObject;
            public bool ActiveSelf;
        }
        [Serializable]
        public struct Active
        {
            public act[] actives;
        }
        [ConditionalHideInInspector("eventType", TYPE.CLEAR)]
        public CheckMapClear ClearBox;

        [ConditionalHideInInspector("eventType", TYPE.ACTIVE)]
        public Active ActiveObjs;

        [ConditionalHideInInspector("eventType", TYPE.MOVE)]
        public MovableList Movables;

        [ConditionalHideInInspector("eventType", TYPE.PLAY)]
        public PlayableList playableList;
    }
    [Header("인터렉션 방식")]
    public typeOfInteraction type;

    [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
    public TextAsset jsonFile;
    //디버깅용
    [ConditionalHideInInspector("type", typeOfInteraction.DIALOGUE)]
    public DialogueData dialogueData;
    [ConditionalHideInInspector("type", typeOfInteraction.DIALOGUE)]
    public InteractionEvent[] dialogueEndAction;

    [ConditionalHideInInspector("type", typeOfInteraction.TASK)]
    public List<TaskData> _taskData;

    public Stack<TaskData> jsonTask;

    private UnityAction endDialogueAction;
    private UnityAction startDialogueAction;

    [Header("아웃라인 색 설정")]
    public OutlineColor color;

    [Header("인터렉션 오브젝트 터치 유무 감지 기능 사용할건지 말건지")]
    public TouchOrNot touchOrNot;
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

    [Header("터치 여부 확인")]
    public bool isTouched = false;
    public RaycastHit hit;

    [Header("실제로 터치되는 오브젝트. 만약 스크립트 적용된 오브젝트가 터치될 오브젝트라면 그냥 None인상태로 두기!")]
    public GameObject touchTargetObject;

    private void PushTask(string jsonString)
    {
        TaskData taskData = new TaskData();
        taskData.tasks = JsontoString.FromJsonArray<Task>(jsonString);
        jsonTask.Push(taskData);
        DataController.instance_DataController.taskData = taskData;
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            //Debug.Log(Application.dataPath.json); 에셋 경로    뒤에 타입 붙여야됨
            //Debug.Log(Resources.Load("StoryMap/ep1/Song")); 리소스 경로        뒤에 타입 안붙어도 됨

            currentCharacter = DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main);
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
    void SetBtnPress(int index)
    {
        TaskData currentTaskData = jsonTask.Peek();


        if (currentTaskData.tasks[currentTaskData.taskIndex + index].type.Equals(TYPE.DIALOGUE))
        {
            currentTaskData.isContinue = false;
            string path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
            string jsonString = (Resources.Load(path) as TextAsset).text;
            //string path = Application.dataPath + "/Dialogues/101010/MainStory/Story/" + DataController.instance_DataController.dialogueData.tasks[index].nextFile + ".json";
            //string jsonString = System.IO.File.ReadAllText(path);
            currentTaskData.taskIndex--;
            CanvasControl.instance_CanvasControl.StartConversation(jsonString);
        }
        else if (currentTaskData.tasks[currentTaskData.taskIndex + index].type.Equals(TYPE.TEMP))
        {
            //m3, 1, 2
            currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar = currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar.Replace("m", "-");
            int[] changeVal = Array.ConvertAll(currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar.Split(','), (item) => int.Parse(item));
            {
                DataController.instance_DataController.charData.selfEstm += changeVal[0];
                DataController.instance_DataController.charData.intimacy_spRau += changeVal[1];
                DataController.instance_DataController.charData.intimacy_ounRau += changeVal[2];
            }
            //새로운 task 실행
            string path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
            string jsonString = (Resources.Load(path) as TextAsset).text;
            PushTask(jsonString);
            interactionResponse();
        }

        //index에 따른 task 추가
        //실행
    }


    /// <summary>
    /// 인터랙션 실행하는 함수
    /// </summary>
    public void interactionResponse()
    {
        if (jsonTask == null || jsonTask.Count == 0)
        {
            TaskStart();
        }
        //3가지 1)애니메이션 2)대사 3)카메라 변환(확대라든지) 4)맵포탈
        if (type == typeOfInteraction.animation && this.gameObject.GetComponent<Animator>() != null)//애니메이터가 존재한다면 
        {
            //세팅된 애니메이터 시작
            this.gameObject.GetComponent<Animator>().Play("Start", 0);

        }
        else if (type == typeOfInteraction.DIALOGUE)
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
                foreach (InteractionEvent dialogueEndAction in dialogueEndAction)
                {
                    switch (dialogueEndAction.eventType)
                    {
                        case InteractionEvent.TYPE.CLEAR:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() => dialogueEndAction.ClearBox.Clear());
                            break;
                        case InteractionEvent.TYPE.ACTIVE:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (InteractionEvent.act active in dialogueEndAction.ActiveObjs.actives)
                                {
                                    Debug.Log(active.ActiveSelf);
                                    active.ActiveObject.SetActive(active.ActiveSelf);
                                }
                            });
                            break;
                        case InteractionEvent.TYPE.MOVE:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (MovableObj movable in dialogueEndAction.Movables.movables)
                                {
                                    movable.gameObject.GetComponent<IMovable>().IsMove = movable.isMove;
                                }
                            });
                            break;
                        case InteractionEvent.TYPE.PLAY:
                            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() =>
                            {
                                foreach (PlayableObj playable in dialogueEndAction.playableList.playableObjs)
                                {
                                    playable.gameObject.GetComponent<Playable>().isPlay = playable.isPlay;
                                }
                            });
                            break;
                    }
                }
                CanvasControl.instance_CanvasControl.StartConversation(jsonFile.text);
            }
            else
                Debug.LogError("json 파일 없는 오류");
            //대사활성화
        }
        else if (type == typeOfInteraction.camerasetting)
        {
            //카메라 변환 활성화
        }
        else if (type == typeOfInteraction.POTAL && this.gameObject.GetComponent<CheckMapClear>() != null)
        {
            DataController.instance_DataController.ChangeMap(DataController.instance_DataController.currentMap.nextMapcode);

        }
        //1회성 interaction인 경우 굳이 excel로 할 필요 없이 바로 실행 dialogue도 마찬가지 단순한 잡담이면 typeOfInteraction.dialogue에서 처리
        else if (type == typeOfInteraction.TASK)
        {
            isTouched = true;
            if (jsonTask.Count == 0) { Debug.LogError("jsontask파일 없음 오류오류"); }
            StartCoroutine(TaskCorutine());
        }
        else if (type == typeOfInteraction.interact && TryGetComponent(out CheckMapClear checkMapClear))
        {
            //애니메이션 재생 후 다음 맵으로 넘어가는 등의 인터렉션이 있을 때.
            if (TryGetComponent(out Animator animator))
            {
                animator.SetBool("Interation", true);
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Finish"))
                {
                    checkMapClear.Clear();
                    Debug.Log("터치터치");
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
                    if (touchOrNot == TouchOrNot.yes)
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

                                    interactionResponse();//인터렉션반응 나타남.
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
        else
        {
            if (jsonFile != null)
            {
                if (type == typeOfInteraction.DIALOGUE)
                    dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
                else if (type == typeOfInteraction.TASK)
                    LoadTaskData();
            }
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
        if (jsonFile)
        {
            if (type == typeOfInteraction.TASK)
            {
                jsonTask = new Stack<TaskData>();
                PushTask(jsonFile.text);
                Debug.Log(jsonFile.text);
            }
            else if (type == typeOfInteraction.DIALOGUE)
                DataController.instance_DataController.dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
        }
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

        DataController.instance_DataController.taskData = currentTaskData;

        Debug.Log(currentTaskData.taskIndex);
        Debug.Log(currentTaskData.tasks[currentTaskData.taskIndex].order);
        Debug.Log(currentTaskData.taskOrder);
        while (currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) && currentTaskData.isContinue)     //순서 번호 동일한 것 반복
        {
            int taskIndex = currentTaskData.taskIndex;
            Debug.Log("taskIndex  " + taskIndex + "     " + currentTaskData.tasks[taskIndex].type);
            switch (currentTaskData.tasks[taskIndex].type)
            {
                case TYPE.DIALOGUE:
                    Debug.Log("dialogue");
                    currentTaskData.isContinue = false;
                    string path = currentTaskData.tasks[taskIndex].nextFile;
                    Debug.Log(path);
                    string jsonString = (Resources.Load(path) as TextAsset).text;
                    Debug.Log(path);
                    //string path = Application.dataPath + "/Dialogues/101010/MainStory/Story/" + DataController.instance_DataController.dialogueData.tasks[index].nextFile + ".json";
                    //string jsonString = System.IO.File.ReadAllText(path);
                    CanvasControl.instance_CanvasControl.StartConversation(jsonString);

                    //대화가 끝날 경우 iscontinue 다시 활성화
                    break;
                case TYPE.ANIMATION:
                    //세팅된 애니메이션 실행
                    currentTaskData.taskIndex++;
                    gameObject.GetComponent<Animator>().Play("Start", 0);
                    break;
                case TYPE.TEMP:
                    Debug.Log("temp");
                    currentTaskData.isContinue = false;
                    CanvasControl.instance_CanvasControl.SetChoiceAction(SetBtnPress);
                    CanvasControl.instance_CanvasControl.OpenChoicePanel();
                    break;
                case TYPE.TEMPEND:
                    //Temp Task 끝날 때
                    Debug.Log("tempEnd");
                    DataController.instance_DataController.taskData = null;
                    jsonTask.Pop();
                    //jsonTask.Peek().taskIndex++;
                    jsonTask.Peek().isContinue = true;
                    yield break;
                case TYPE.TASKEND:
                    isTouched = false;
                    jsonTask.Peek().isContinue = true;
                    jsonTask.Peek().taskIndex = 0;
                    jsonTask.Peek().taskOrder = 1;
                    yield break;
                case TYPE.NEW:
                    {
                        //
                        //End가 필요한가
                        break;
                    }
                default:
                    Debug.LogError($"{currentTaskData.tasks[taskIndex].type}은 존재하지 않는 type입니다.");
                    break;
            }
            yield return waitUntil;
            Debug.Log("반복 중" + currentTaskData.taskIndex);
        }
        currentTaskData.taskOrder++;
    }
    //For Debugging
    private void LoadTaskData()
    {
        if (_taskData.Count == 0)
        {
            TaskData data = new TaskData();
            data.tasks = JsontoString.FromJsonArray<Task>(jsonFile.text);
            _taskData.Add(data);
            foreach (TaskData taskData in _taskData)
            {
                for (int i = 0; i < taskData.tasks.Length; i++)
                {
                    if (taskData.tasks[i].type == TYPE.NEW || taskData.tasks[i].type == TYPE.TEMP)
                    {
                        int count = int.Parse(taskData.tasks[i].nextFile);
                        for (int j = 1; j <= count; j++)
                        {
                            string path = taskData.tasks[i + j].nextFile;
                            string jsonString = (Resources.Load(path) as TextAsset).text;


                            data = new TaskData();
                            data.tasks = JsontoString.FromJsonArray<Task>(jsonString);
                            if (_taskData.Count > 50)
                            {
                                return;
                            }
                            _taskData.Add(data);
                        }
                        i += count + 1;
                    }
                }
            }
        }
    }
}