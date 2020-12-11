using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Script // 대사
{
    public string speaker; // 말하는 사람
    [TextArea]
    public string dialogue; // 대사

}

// 이 스크립트는 obj_interaction 오브젝트에 

public class SpeechBubbleDialogue : MonoBehaviour
{

    public enum SpeechBubbleDialogueType
    {

        loop, // 메이플같이 obj_interaction 오브젝트주위에 캐릭터가 있는지 없는지 상관 없이 설정한 시간 간격으로 계속 반복되는 말풍선.
        //보류 loop_near, // 가까이 있을 때만 반복실행되는 말풍선.
        once, // obj_interaction 오브젝트 주위에 캐릭터가 있으면 뜨는 말풍선. 띄워진 말풍선은 몇 초 뒤에 사라짐.
        story //  obj_interaction 오브젝트 주위에 캐릭터가 있는 상태에서 플레이어가 obj_interaction 오브젝트를 터치하면 말풍선 띄워지고 대화 진행 화살표를 클릭하면 다음 대화 뜸 or 종료

    }

    public enum RandomSecond
    {
        useSettedSecond, // 시간설정할 수 있음.
        useRandomSecond // 랜덤으로 
    }

    [Header("말풍선 대화 타입 선택")]
    public SpeechBubbleDialogueType speechBubbleDialogueType;

    [Header("말풍선 보이는, 안보이는 시간 타입 선택")]
    public RandomSecond randomSecond;
    public float visibleSecond = 5; // 말풍선 보이는 시간(초) 세팅. 따로 설정안한 경우 5초가 디폴트
    public float invisibleSecond = 5; // 말풍선 안보이는 시간(초) 세팅. 만약 말풍선 타입이 once면, invisibleSecond 값은 무시되고 무조건 0임. 따로 설정안한 경우 5초가 디폴트
    public float randomRange = 5; // 랜덤 범위. 따로 설정안한 경우 5초가 디폴트

    public Camera cam;
    public bool isStartSpeechBubble = false;
    public CharacterManager character;
    InteractionObj_stroke interactionObj_Stroke;

    // 공통
    [Header("말풍선")]
    public GameObject speechBubbleObj; // 말풍선
    public Text speakerNameText; // 말하는 사람 이름 텍스트
    public Text dialogueText; // 대사 텍스트
    Coroutine speechBubbleCoroutine; // 코루틴
    int cnt = 0; // SpeechBubbleDialogueArr 인덱스

    [Header("말풍선 대사 입력")]
    public Script[] SpeechBubbleDialogueArr; // inspector창에서 대사 입력하면됨.

    [Header("말풍선 위치 조절")]
    public float x;
    public float y;

    // loop
    public bool isLoop = true;

    // once
    bool triggerNext = true;

    // continuous, once
    bool isLastDialogue = false;


    // 주위에 캐릭터 있는지 감지
    public bool isCharacterInRange = false;
    public int radius = 3;



    // Start is called before the first frame update
    void Start()
    {
        // 캐릭터 맵으로 이동시키기
        DataController.instance_DataController.isMapChanged = true;

        // 카메라
        cam = DataController.instance_DataController.cam;

        // 오브젝트 주위 선
        interactionObj_Stroke = gameObject.GetComponent<InteractionObj_stroke>();

        // 말풍선 안보이게
        speechBubbleObj.SetActive(false);

        // 태그 설정
        if (speechBubbleDialogueType != SpeechBubbleDialogueType.loop)
        {
            gameObject.tag = "obj_interaction";
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (character == null)
        {
            character = DataController.instance_DataController.currentChar;
        }

        CheckAroundCharacter(); // obj_interaction 주위 범위내에 character있는지 확인
        SetSpeechBubblePosition(); // 말풍선 위치시키기

        if (speechBubbleDialogueType == SpeechBubbleDialogueType.loop && isLoop)
        {
            if (interactionObj_Stroke) interactionObj_Stroke.exclamationMark.gameObject.SetActive(false); // 느낌표 끄기
            speechBubbleCoroutine = StartCoroutine(StartSpeechBubble());
        }
        /*보류else if (speechBubbleDialogueType == SpeechBubbleDialogueType.loop_near && isCharacterInRange && isLoop)
        {
            if (interactionObj_Stroke) interactionObj_Stroke.exclamationMark.gameObject.SetActive(false); // 느낌표 끄기
            speechBubbleCoroutine = StartCoroutine(StartSpeechBubble());
        }*/
        else if (speechBubbleDialogueType == SpeechBubbleDialogueType.once) // 주위에 캐릭터 있을때 한번 실행되는!
        {
            if (!isCharacterInRange)
            {
                cnt = 0;
                triggerNext = true;
                isLastDialogue = false;
                isStartSpeechBubble = false;
            }
            else if (isCharacterInRange && !isStartSpeechBubble && !isLastDialogue && triggerNext)
            {
                if (interactionObj_Stroke) interactionObj_Stroke.exclamationMark.gameObject.SetActive(false); // 느낌표 끄기
                isStartSpeechBubble = true;
                speechBubbleCoroutine = StartCoroutine(StartSpeechBubble());
            }

        }
        else if (speechBubbleDialogueType == SpeechBubbleDialogueType.story)
        {
            if (!isCharacterInRange)
            {
                cnt = 0;
                isLastDialogue = false;
                isStartSpeechBubble = false;
            }
            else if (isCharacterInRange && !isStartSpeechBubble && Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    // 오브젝트 터치 감지
                    if (hit.collider.gameObject == gameObject)
                    {
                        isStartSpeechBubble = true;
                        if (interactionObj_Stroke)
                        {
                            interactionObj_Stroke.exclamationMark.gameObject.SetActive(false); // 느낌표 끄기
                            interactionObj_Stroke.outline.enabled = false; // 아웃라인 끄기
                        }

                        speechBubbleObj.SetActive(true);
                        UpdateDialogue();
                    }

                }
            }

        }

    }

    void UpdateDialogue()
    {
        speakerNameText.text = SpeechBubbleDialogueArr[cnt].speaker;
        dialogueText.text = SpeechBubbleDialogueArr[cnt].dialogue;
        cnt++;
        if (cnt >= SpeechBubbleDialogueArr.Length)
        { // 마지막 대사면 cnt = 0
            isLastDialogue = true;
            cnt = 0;
        }
    }

    void SetSpeechBubblePosition()
    {
        Vector3 myScreenPos = cam.WorldToScreenPoint(transform.position);
        speechBubbleObj.transform.position = myScreenPos + new Vector3(x, y, 0);
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
                    if (hit.collider.gameObject.name == character.gameObject.name)
                    {
                        isCharacterInRange = true;
                    }
                    else
                    {
                        isCharacterInRange = false;
                    }

                }
                else
                {
                    isCharacterInRange = false;
                }


            }
        }

    }


    // next버튼 눌렀을 때 호출
    public void ClickNextBtn()
    {
        if (speechBubbleDialogueType == SpeechBubbleDialogueType.story)
        {
            if (isLastDialogue) speechBubbleObj.SetActive(false);
            UpdateDialogue();
        }

    }

    IEnumerator StartSpeechBubble()
    {

        if (speechBubbleDialogueType == SpeechBubbleDialogueType.loop)
        {
            isLoop = false;
            speechBubbleObj.SetActive(true);
            UpdateDialogue();

            if (randomSecond == RandomSecond.useSettedSecond)
            {
                yield return new WaitForSeconds(visibleSecond); // visibleSecond동안 말풍선 띄우기   
                speechBubbleObj.SetActive(false);
                yield return new WaitForSeconds(invisibleSecond); // invisibleSecond동안 말풍선 없애기
            }
            else if (randomSecond == RandomSecond.useRandomSecond)
            {
                visibleSecond = Random.Range(0, randomRange);
                invisibleSecond = Random.Range(0, randomRange);
                yield return new WaitForSeconds(visibleSecond); // visibleSecond동안 말풍선 띄우기   
                speechBubbleObj.SetActive(false);
                yield return new WaitForSeconds(invisibleSecond); // invisibleSecond동안 말풍선 없애기
            }

            isLoop = true;
            StopCoroutine(speechBubbleCoroutine);
        }
        else if (speechBubbleDialogueType == SpeechBubbleDialogueType.once)
        {
            triggerNext = false;
            speechBubbleObj.SetActive(true);
            UpdateDialogue();
            if (randomSecond == RandomSecond.useSettedSecond)
            {
                yield return new WaitForSeconds(visibleSecond); // 5초간 말풍선 띄우기
            }
            else if (randomSecond == RandomSecond.useRandomSecond)
            {
                visibleSecond = Random.Range(0, randomRange);
                yield return new WaitForSeconds(visibleSecond); // 5초간 말풍선 띄우기
            }
            speechBubbleObj.SetActive(false);
            triggerNext = true;
            yield return new WaitForSeconds(0); // 0초간 말풍선 없애기
            StopCoroutine(speechBubbleCoroutine);
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}