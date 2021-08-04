﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 인터렉션하는 오브젝트에 컴포넌트로 추가!!
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
        portal,
        animation,
        dialogue,
        camerasetting,
        interact,
        continuous
    }

    public enum TouchOrNot // 인터렉션 오브젝트 터치했는지 안했는지 감지 기능 필요
    {
        yes,
        no
    }

    [Header("인터렉션 방식")]
    public typeOfInteraction type;

    [Header("Continuous 혹은 Dialogue인 경우에만 값을 넣으시오")]
    public TextAsset jsonFile;
    public Stack<TaskData> jsonTask;

    [Header("아웃라인 색 설정")]
    public OutlineColor color;

    [Header("인터렉션 오브젝트 터치 유무 감지 기능 사용할건지 말건지")]
    public TouchOrNot touchOrNot;
    public bool onOutline = false; // 아웃라인 켜져있는 안켜져 있는지
    public bool isCharacterInRange = false; // obj_interaction 오브젝트 기준으로 일정 범위 내에 캐릭터가 있는지 확인
    public int radius = 5;
    public GameObject exclamationMark;

    [Header("느낌표 사용할 때 체크")]
    public bool useExclamationMark = false;
    public float x; // 느낌표 위치(x좌표) 조절
    public float y; // 느낌표 위치(y좌표) 조절
    private bool isInteractionObj = false;
    public Outline outline;
    private CharacterManager character;
    public Camera cam;

    [Header("터치 여부 확인")]
    public bool isTouched = false;
    public RaycastHit hit;

    [Header("터치될 오브젝트. 만약 스크립트 적용된 오브젝트가 터치될 오브젝트라면 그냥 None인상태로 두기!")]
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
        //Debug.Log(Application.dataPath.json); 에셋 경로    뒤에 타입 붙여야됨
        //Debug.Log(Resources.Load("StoryMap/ep1/Song")); 리소스 경로        뒤에 타입 안붙어도 됨
        if (jsonFile)
        {
            if (type == typeOfInteraction.continuous)
            {
                jsonTask = new Stack<TaskData>();
                PushTask(jsonFile.text);
            }
            else if (type == typeOfInteraction.dialogue)
                DataController.instance_DataController.dialogueData.dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
        }
    }
    
    void SetBtnPress(int index) {
        TaskData currentTaskData = jsonTask.Peek();
        //호감도, 자존감 값

        int[] changeVal = Array.ConvertAll(currentTaskData.tasks[currentTaskData.taskIndex + index].increaseVar.Split(','), (item) => int.Parse(item));


        if (currentTaskData.tasks[currentTaskData.taskIndex + index].type == TYPE.DIALOGUE)
        {
            //대화만 실행
        }
        else if(currentTaskData.tasks[currentTaskData.taskIndex + index].type.Equals(TYPE.TEMP))
        {
            //새로운 task 실행
            string path = currentTaskData.tasks[currentTaskData.taskIndex + index].nextFile;
            string jsonString = (Resources.Load(path) as TextAsset).text;
            PushTask(jsonString);
            interactionResponse();
        }

        //index에 따른 task 추가
        //실행
    }


    void interactionResponse() 
    {
        //3가지 1)애니메이션 2)대사 3)카메라 변환(확대라든지) 4)맵포탈
        if (type == typeOfInteraction.animation && this.gameObject.GetComponent<Animator>() != null)//애니메이터가 존재한다면 
        {
            //세팅된 애니메이터 시작
            this.gameObject.GetComponent<Animator>().Play("Start", 0);

        }
        else if (type == typeOfInteraction.dialogue)
        {
            CanvasControl.instance_CanvasControl.StartConversation(jsonFile.text);
            //대사활성화
        }
        else if (type == typeOfInteraction.camerasetting)
        {
            //카메라 변환 활성화
        }
        else if (type == typeOfInteraction.portal && this.gameObject.GetComponent<CheckMapClear>() != null)
        {
            isTouched = true;
        }
        //1회성 interaction인 경우 굳이 excel로 할 필요 없이 바로 실행 dialogue도 마찬가지 단순한 잡담이면 typeOfInteraction.dialogue에서 처리
        else if (type == typeOfInteraction.continuous)
        {
            if (jsonTask == null) { Debug.LogError("jsontask파일 없음 오류오류"); }
            StartCoroutine(TaskCorutine());
        }
        else if (type == typeOfInteraction.interact && this.gameObject.GetComponent<CheckMapClear>() != null)
        {
            //애니메이션 재생 후 다음 맵으로 넘어가는 등의 인터렉션이 있을 때.
            if (this.gameObject.GetComponent<Animator>() != null)
            {
                this.gameObject.GetComponent<Animator>().SetBool("Interation", true);
                if (this.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Finish"))
                {
                    isTouched = true;
                    Debug.Log("터치터치");
                }
            }


        }
    }
    void Update()
    {
        if (!character)
        {
            character = DataController.instance_DataController.currentChar;
        }

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

                if (exclamationMark.gameObject != null) exclamationMark.gameObject.SetActive(false); // 느낌표 끄기

                character = DataController.instance_DataController.currentChar;

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
            if (isCharacterInRange && !onOutline) // 범위 내로 들어옴
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
            else if (!isCharacterInRange && onOutline) // 범위 밖
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

            // 플레이어의 인터렉션 오브젝트 터치 감지
            if (touchOrNot == TouchOrNot.yes)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        // 오브젝트 터치 감지
                        if (hit.collider.gameObject.Equals(touchTargetObject))
                        {
                            interactionResponse();//인터렉션반응 나타남.
                        }

                    }
                }
                else
                {
                    isTouched = false;
                }
            }

        }
        /*
        Vector3 myScreenPos = cam.WorldToScreenPoint(transform.position);
        exclamationMark.transform.position = myScreenPos + new Vector3(x, y, 0);
        */
    }

    void CheckAroundCharacter()
    {
        RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position, radius, Vector3.up, 0f);
        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != null)
                {
                    if (hit.collider.gameObject == character.gameObject) isCharacterInRange = true;

                }
                else
                {
                    isCharacterInRange = false;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
    private IEnumerator TaskCorutine()
    {
        TaskData currentTaskData = jsonTask.Peek();
        Debug.Log(currentTaskData.tasks[0].name);
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

        
        while (currentTaskData.tasks[currentTaskData.taskIndex].order.Equals(currentTaskData.taskOrder) && currentTaskData.isContinue)     //순서 번호 동일한 것 반복
        {
            int taskIndex = currentTaskData.taskIndex;
            Debug.Log("taskIndex  " + taskIndex  + "     " + currentTaskData.tasks[taskIndex].type);
            switch (currentTaskData.tasks[taskIndex].type)
            {
                case TYPE.DIALOGUE:
                    Debug.Log("dialogue");
                    currentTaskData.isContinue = false;
                    string path = currentTaskData.tasks[taskIndex].nextFile;
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
                case TYPE.TASKEND:
                    //Temp Task 끝날 때
                    Debug.Log("tempEnd");
                    DataController.instance_DataController.taskData = null;
                    jsonTask.Pop();
                    jsonTask.Peek().taskIndex++;
                    jsonTask.Peek().isContinue = true;
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
}