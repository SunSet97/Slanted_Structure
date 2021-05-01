using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlazaManager : MonoBehaviour
{

    [Header("인터랙션하는 벤치들 부모 넣어주기~")] // 부모> 1 2 3 4 5 ...
    public GameObject interactionBenchParent;
    List<Interact_ObjectWithRau> interactionBenchList = new List<Interact_ObjectWithRau>();

    [Header("비둘기 그룹 각각 넣기")]
    public Interact_ObjectWithRau[] doves;
    [SerializeField]

    [Header("말풍선 대화하는 학생들 부모 넣어주기~")] // 부모> (그룹1>1 2 3 4 ...) (그룹2>1 2 3 4 ...) ...
    public GameObject interactionStudentParent;
    List<SpeechBubbleDialogue> interactionStudentList = new List<SpeechBubbleDialogue>();

    [Header("말풍선 대화하는 커플들 부모 넣어주기~")] //// 부모> (그룹1>1 2 3 4 ...) (그룹2>1 2 3 4 ...) ...
    public GameObject interactionCoupleParent;
    List<SpeechBubbleDialogue> interactionCoupleList = new List<SpeechBubbleDialogue>();

    Camera cam;

    public GameObject oldMan;
    public GameObject police;
    CharacterController policeCtrl;
    public int policeDir = 0;
    bool isWalkingAroundFountain = true;
    bool isTalking = false;


    // Start is called before the first frame update
    void Start()
    {
        cam = DataController.instance_DataController.cam;

        // 벤치들의 인터랙션 컴포넌트들 interactionBenchList에 넣기
        for (int i = 0; i < interactionBenchParent.transform.childCount; i++)
        {
            interactionBenchList.Add(interactionBenchParent.transform.GetChild(i).GetComponent<Interact_ObjectWithRau>());
        }

        // 말풍선 대화하는 학생들 SpeechBubbleDialogue 컴포넌트 interactionStudentList에 넣기
        for (int i = 0; i < interactionStudentParent.transform.childCount; i++)
        {
            GameObject tempParent = interactionStudentParent.transform.GetChild(i).gameObject;

            for (int j = 0; j < tempParent.transform.childCount; j++)
            {
                interactionStudentList.Add(tempParent.transform.GetChild(i).GetComponent<SpeechBubbleDialogue>());
                interactionStudentList[i].speechBubbleDialogueType = 0; // 말풍선 타입 loop로 설정
                interactionStudentList[i].isLoop = true;
            }

        }

        // 말풍선 대화하는 커플들 SpeechBubbleDialogue 컴포넌트 interactionStudentList에 넣기
        for (int i = 0; i < interactionCoupleParent.transform.childCount; i++)
        {
            GameObject tempParent = interactionCoupleParent.transform.GetChild(i).gameObject;

            for (int j = 0; j < tempParent.transform.childCount; j++)
            {
                interactionCoupleList.Add(tempParent.transform.GetChild(i).GetComponent<SpeechBubbleDialogue>());
                interactionCoupleList[i].speechBubbleDialogueType = 0; // 말풍선 타입 loop로 설정
                interactionCoupleList[i].isLoop = true;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        CheckDoveClick(); // 비둘기 클릭하는 것 감지 및 애니메이션 처리

        // 벤치 인터랙션 관련
        for (int i = 0; i < interactionBenchList.Count; i++)
        {
            Interact_ObjectWithRau tempInteractionObj = interactionBenchList[i];
            if (tempInteractionObj.isTouched) {
                // ★ 여기에 "앉는" 애니메이션 넣기 
                Debug.Log("앉는 애니메이션~~~");
            }
        }


        // 대화를 할 수 없는 상황이 되면(즉, 대화중일때)
        if (!CanvasControl.instance_CanvasControl.isPossibleCnvs)
        {
            Interact_ObjectWithRau interaction_oldMan = oldMan.GetComponent<Interact_ObjectWithRau>();
            Interact_ObjectWithRau interaction_police = police.GetComponent<Interact_ObjectWithRau>();
            interaction_oldMan.mark.SetActive(false); // 노인 느낌표 없애기
            interaction_police.mark.SetActive(false); // 경찰 느낌표 없애기
            interaction_oldMan.outline.enabled = false; // 노인 아웃라인 끄기
            interaction_police.outline.enabled = false; // 경찰 아웃라인 끄기

        }

    }

    void CheckDoveClick() {
        int idx = -1;
        foreach (Interact_ObjectWithRau doveGroup in doves) {
            idx++;
            if (doveGroup.isTouched) {
                doveGroup.isInteracting = true;
                doveGroup.mark.SetActive(false);// 씨앗 말풍선 끄기
                doveGroup.outline.enabled = false; // 아웃라인 끄기
                for (int i = 0; i < doveGroup.transform.childCount; i++) {
                    print("doveGroup.transform.childCount: "+ doveGroup.transform.childCount);
                    Animator doveChildAnim = doveGroup.transform.GetChild(i).GetComponent<Animator>();
                    StartCoroutine(PlayEatAnimation(3, doveGroup, doveChildAnim));
                }
                break;
            }
        }
    }

    IEnumerator PlayEatAnimation(float playTime, Interact_ObjectWithRau doveGroup, Animator doveChildAnim)
    {
        doveChildAnim.SetBool("idle", false);
        doveChildAnim.SetBool("eat", true);
        yield return new WaitForSeconds(playTime);
        doveChildAnim.SetBool("eat", false);
        doveChildAnim.SetBool("idle", true);
        doveGroup.isInteracting = false;
    }

}
