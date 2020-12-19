using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class doveAnim {

    public InteractionObj_stroke doveInteractionObj_Stroke;
    public List<Animator> animatorList = new List<Animator>();

}

public class PlazaManager : MonoBehaviour
{
    [Header("인터랙션하는 벤치들 부모 넣어주기~")] // 부모> 1 2 3 4 5 ...
    public GameObject interactionBenchParent;
    List<Interact_ObjectWithRau> interactionBenchList = new List<Interact_ObjectWithRau>();

    [Header("인터랙션하는 비둘기들 부모 넣어주기~")] // 부모> (그룹1>1 2 3 4 ...) (그룹2>1 2 3 4 ...) ...
    public GameObject interactionDoveParent;
    List<doveAnim> doveAnimators = new List<doveAnim>();

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

        // 비둘기 인터랙션 컴포넌트 및 애니메이터 리스트 넣기
        for (int i = 0; i < interactionDoveParent.transform.childCount; i++) {
            GameObject childGroup = interactionDoveParent.transform.GetChild(i).gameObject;
            doveAnimators.Add(new doveAnim());
            doveAnimators[i].doveInteractionObj_Stroke = childGroup.GetComponent<InteractionObj_stroke>();
            doveAnimators[i].doveInteractionObj_Stroke.touchTargetObject = doveAnimators[i].doveInteractionObj_Stroke.exclamationMark; // 터치할 대상을 먹이 말풍선으로 바꾸기!
            for (int j = 0; j < childGroup.transform.childCount; j++) {
                doveAnimators[i].animatorList.Add(childGroup.transform.GetChild(j).GetComponent<Animator>());
            }
        }


        /*if (police)
        {
            policeCtrl = police.GetComponent<CharacterController>();
            Invoke("PoliceManWalkingArounFoundtaion", 0);
        }
        */

    }

    // Update is called once per frame
    void Update()
    {
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

        //policeCtrl.Move(Vector3.right * policeDir * 2 * Time.deltaTime);

    }

    // 비둘기 머리위 씨앗 말풍선 클릭할때 호출됨.
    public void ClickDoves() {
        print("클릭");
        GameObject clickedSeed = EventSystem.current.currentSelectedGameObject; // 클릭된거가 seedballoon1 2 3 ...임.
        GameObject tempParent = clickedSeed.transform.parent.gameObject;
        int tempIndex = 0; // 일단, 초기값은 0으로 설정

        // 클릭된 seedballoon의 인덱스 확인
        for (int i = 0; i < tempParent.transform.childCount; i++) {
            if (tempParent.transform.GetChild(i).gameObject == clickedSeed) {
                tempIndex = i;
            }
        }

        // 위에서 얻은 인덱스로 아웃라인 끄고. 씨앗 말풍선 안보이게하고. 애니메이션 가동!
        doveAnimators[tempIndex].doveInteractionObj_Stroke.outline.enabled = false;
        doveAnimators[tempIndex].doveInteractionObj_Stroke.exclamationMark.SetActive(false);
        List <Animator> tempAnimatorList = doveAnimators[tempIndex].animatorList;
        for (int i = 0; i < tempAnimatorList.Count; i++) {
            StartCoroutine(PlayEatAnimation(3, tempAnimatorList[i]));
        }

    }

    void PoliceManWalkingArounFoundtaion( )
    {
        policeDir = (int)Random.Range(-1,2);

        if (isTalking)
        {
            policeDir = 0;
        }
        else
        {
            if (policeDir == 1) police.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            if (policeDir == -1) police.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
            if (policeDir == 0) police.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        Invoke("PoliceManWalkingArounFoundtaion", 3);

    }



    IEnumerator PlayEatAnimation(float playTime, Animator anim)
    {
        // ★ 라우가 먹이 주는 애니메이션 여기다 넣기!!

        anim.SetBool("idle", false);
        anim.SetBool("eat", true);

        yield return new WaitForSeconds(playTime);

        anim.SetBool("eat", false);
        anim.SetBool("idle", true);

    }

}
