using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Script // 대사
{
    public string talker; // 말하는 사람
    [TextArea]
    public string dialogue; // 대사

}


public class NPCOBJsetting : MonoBehaviour
{
    //public GameObject character; // 현재 캐릭터
    //public Text talkerText; // 말하는 캐릭터 텍스트
    [Header("카메라")]
    public Camera cam;

    [Header("대화 타입 선택")]
    public bool isDefaultDialogue = false; // 스토리 없이 그냥 대화만 하는 npc면 체크
    //public bool isStoryDialogue = false; // (아직..)스토리 있는 대화하는 npc면 체크 => npcInteractor로 ㄱㄴ
    public bool isLoopingDialogue = false; // 몇초에 한번씩 나타나는 말풍선 대화하는 npc면 체크
    int npcID;

    [Header("★defaultDialogue일때 사용★")]
    public int distance = 2;
    public GameObject defaultDialogue;
    public Text talkerNameText;
    public Text defaultDialogueText;
    public Script[] defaultDialogueArr;
    int defaultDialogueCnt = 0;
    bool isTalking = false;
    //public Text characterNameText;
    //public Text defaultDialogueText;

    //[Header("storyDialogue일때 사용")]


    [Header("☆loopingDialogue일때 사용☆")]
    public GameObject loopingDialogue; // 말풍선
    public TextMesh loopingDialogueText; // 말풍선 대사 텍스트
    public Script[] loopingDialogueArr;
    int loopingDialogueCnt = 0;
    int onTime = 3;
    int offTime = 3;
    Coroutine loopingDialogueCoroutine;
    int loopTime = 10; // 10초에 한번씩 말풍선 대화창 뜸. 

    // Update is called once per frame
    void Update()
    {
        // 몇초에 한번씩 말풍선 대화
        if (isLoopingDialogue)
        {
            loopingDialogueCoroutine = StartCoroutine(LoopingDialogue());
        }

        if (isDefaultDialogue)
        {
            if (isTalking)
            {
                if (Input.GetMouseButtonDown(0) && defaultDialogueCnt >= defaultDialogueArr.Length)
                {
                    defaultDialogueCnt = 0;
                    isTalking = false; // Default 대화 끝!
                    OffDefault();
                }
                else if (Input.GetMouseButtonDown(0) && defaultDialogueCnt < defaultDialogueArr.Length)
                {
                    UpdateDefaultDialogue();
                    defaultDialogueCnt++;
                }
            }
        }

    }


    void FixedUpdate()
    {

        if (isDefaultDialogue)
        {
            // 대화 npc 터치 감지 (범위 내에 캐릭터가 있으면 반응.)
            if (Input.GetMouseButtonDown(0) && IsInRange(DataController.instance_DataController.currentChar.transform.position))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.gameObject == gameObject && !isTalking)
                    {
                        OnDefault();
                        UpdateDefaultDialogue();
                    }
                }
            }

        }

        /*
        Debug.DrawRay(transform.position, rayDirection * distance, new Color(0,1,0));
        RaycastHit rayHit = Physics.Raycast(transform.position, rayDirection, distance, ' 레이어 마스크 인덱스 ');
        */

    }

    bool IsInRange(Vector3 characterPos) // character랑 거리계산하고 범위 내에 있으면 true.
    {
        if (Mathf.Round(Vector3.Distance(characterPos, transform.position)) <= distance) return true;
        else return false;
        
    }
   

  
    void OnBallon()
    {
        loopingDialogue.SetActive(true);
        // npc 머리위에 말풍선 위치시키기.
        loopingDialogue.transform.position = transform.position + new Vector3(2, 2, 0);

    }

    void OffBallon()
    {
        loopingDialogue.SetActive(false);
    }

    void OnDefault()
    {
        defaultDialogue.SetActive(true);
    }

    void OffDefault()
    {
        defaultDialogue.SetActive(false);
    }

    void UpdateDefaultDialogue()
    {
        print("★");
        isTalking = true;
        talkerNameText.text = defaultDialogueArr[defaultDialogueCnt].talker;
        defaultDialogueText.text = defaultDialogueArr[defaultDialogueCnt].dialogue;

    }

    void UpdateLoopingDialogue()
    {

        loopingDialogueText.text = loopingDialogueArr[loopingDialogueCnt].dialogue;
        loopingDialogueCnt++;
        if (loopingDialogueCnt >= loopingDialogueArr.Length)
        {

            loopingDialogueCnt = 0;
        }

    }
    
    IEnumerator LoopingDialogue()
    {
        isLoopingDialogue = false; // 이 코루틴이 한번 수행되도록
        UpdateLoopingDialogue(); // 말풍선 대사 세팅
        OnBallon(); // 말풍선 보이게
        yield return new WaitForSeconds(onTime); // 말풍선 보이는 시간
        OffBallon();
        yield return new WaitForSeconds(offTime); // 말풍선 안보이는 시간
        isLoopingDialogue = true;
        StopCoroutine(loopingDialogueCoroutine);
    }

}
