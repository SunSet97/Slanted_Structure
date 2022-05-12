using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteractor : MonoBehaviour
{
    //캐릭터 오브젝트 받는 변수
    public Transform character;
    
    private CanvasControl canvasCtrl;
    public float interactableDistance = 2;

    public Transform[] NPCArray;

    public List<InteractionObj_stroke> NPCInteractComponentList = new List<InteractionObj_stroke>();

    // 인스턴스화


    private void Start()
    {
        canvasCtrl = CanvasControl.instance;
        FindNPC();
        character = DataController.instance.GetCharacter(MapData.Character.Main).transform;
    }

    void Update()
    {
        //if(character) FindInteractableNPC(interactableDistance);

        // 추가
        //if (Input.GetMouseButtonDown(0) && !canvasCtrl.isPossibleCnvs) {
        //    canvasCtrl.UpdateWord();
        //}

    }

    public Transform[] GetNPCArray()
    {
        return NPCArray;
    }

    //private void FindInteractableNPC(float interactableDist)
    public void FindInteractableNPC(RaycastHit hit, float distance)
    {
        hit.collider.gameObject.GetComponent<Interact_ObjectWithRau>().isTouched = false;
        // 가장 가까운 NPC를 찾는다
        Transform closestNPC = getClosestNPC();
        if (isInteractable(closestNPC, distance))
        {
            // ++ interact_objectwithrau에서 79줄 hit을 여기 함수의 파라미터로 넘기기
            // ++ getcloset 파라미터로 distance로 
            // ++ 이 스크립트 객체화 시키기 transform.parent....
            // ++ 이중으로 레이캐스트 ㄴㄴ
            if (!canvasCtrl.isPossibleCnvs)
            {
                canvasCtrl.UpdateWord();
            }
            if (hit.collider.gameObject.name == closestNPC.gameObject.name && canvasCtrl.isPossibleCnvs == true)
            {
                // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                canvasCtrl.isPossibleCnvs = false;
                canvasCtrl.dialogueCnt = 0;
                DataController.instance.LoadData(closestNPC.gameObject.name, DataController.instance.charData.story + "_"
                    + DataController.instance.charData.storyBranch + "_" + DataController.instance.charData.storyBranch_scnd + "_"
                    + DataController.instance.charData.dialogue_index + ".json");
                
                //수정
                //canvasCtrl.StartConversation();
            }

            /* 수정 전
            if (Input.GetMouseButtonDown(0))
            {
                if (!canvasCtrl.isPossibleCnvs)
                {
                    canvasCtrl.UpdateWord();

                }

                Ray ray = DataController.instance_DataController.cam.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject != null)
                    {
                        if (hit.collider.gameObject.name == closestNPC.gameObject.name && canvasCtrl.isPossibleCnvs == true)
                        {
                            // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                            canvasCtrl.isPossibleCnvs = false;
                            canvasCtrl.dialogueCnt = 0;
                            DataController.instance_DataController.LoadData(closestNPC.gameObject.name, DataController.instance_DataController.charData.story + "_"
                                + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_"
                                + DataController.instance_DataController.charData.dialogue_index + ".json");

                            canvasCtrl.StartConversation();
                        }
                    }
                }

            }
            */





            /* 안될때 있음..ㅠ
            for (int i = 0; i < NPCInteractComponentList.Count; i++)
            {
                if (NPCInteractComponentList[i].isTouched) // 터치
                {
                    if(canvasCtrl.isPossibleCnvs == true)
                    {
                        // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                        canvasCtrl.isPossibleCnvs = false;

                        DataController.instance_DataController.LoadData(closestNPC.gameObject.name, DataController.instance_DataController.charData.story + "_"
                            + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_"
                            + DataController.instance_DataController.charData.dialogue_index + ".json");

                        canvasCtrl.StartConversation();
                    }
                   
                }   
            }

            if (!canvasCtrl.isPossibleCnvs && Input.GetMouseButtonDown(0))
            {
                canvasCtrl.UpdateWord();
            }
            */

            /* 원래 터치 받는 코드!!

            // 상호작용 가능 할 때에만 npc와의 대화가 가능함 
            if (Input.GetMouseButtonDown(0))
            {
                int cameraIndex = 0;
                

                Camera curCamera = null;

                if (Camera.main.CompareTag("MainCamera"))
                {
                    curCamera = Camera.main.GetComponent<Camera>();
                }
                 

                // 카메라 ~ 터치 지점으로의 ray 
                Ray ray = curCamera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {

                    if (hit.collider.gameObject.name == closestNPC.gameObject.name && canvasCtrl.isPossibleCnvs == true)
                    {
                        // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                        canvasCtrl.isPossibleCnvs = false;

                        DataController.instance_DataController.LoadData(closestNPC.gameObject.name, DataController.instance_DataController.charData.story + "_" 
                            + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_" 
                            + DataController.instance_DataController.charData.dialogue_index + ".json");

                        canvasCtrl.StartConversation();
                    }
                    

                }
            } 
            */

        }

    }

    private bool isInteractable(Transform NPC, float distance)
    {
        // 상호작용 거리 내에 있는지 확인
        float dist = Vector3.Distance(character.transform.position, NPC.position);
        interactableDistance = distance;
        return dist < interactableDistance;
    }

    private Transform getClosestNPC()
    {
        if (NPCArray.Length <= 1)
            return null;

        Transform closestNPC = NPCArray[1];
        float minDist = float.MaxValue;

        foreach(Transform NPC in NPCArray)
        {
            //if (NPC.name == "NPCManager")
            //    continue;
            if (NPC != null && NPC != character)
            {
                float dist = Vector3.Distance(character.transform.position, NPC.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestNPC = NPC;
                }
            }
        }

        return closestNPC;
    }

    private void FindNPC()
    {


        //int npcCnt = transform.childCount;
        //NPCArray = new Transform[npcCnt];
        //for (int i = 0; i < npcCnt; i++)
        //{
        //    if (transform.GetChild(i).CompareTag("NPC"))
        //    {
        //        NPCArray[i] = transform.GetChild(i);

        //    }
        //}
        int npcCnt = transform.childCount;
        NPCArray = new Transform[npcCnt];
        int index = 0; 
        for (index = 0; index < npcCnt; index++)
        {
            NPCArray[index] = transform.GetChild(index);   
        }

        AddInteractionComponent();

        // 나중에 주석 풀어야 함 
        //if (DataController.instance_DataController.rau.isSelected == false && DataController.instance_DataController.rau.isExisted == true)
        //{
        //    index++;
        //    NPCArray[index] = DataController.instance_DataController.rau.gameObject.transform;
        //}
        //if (DataController.instance_DataController.speat.isSelected == false && DataController.instance_DataController.speat.isExisted == true)
        //{
        //    index++;
        //    NPCArray[index] = DataController.instance_DataController.rau.gameObject.transform;
        //}
        //if (DataController.instance_DataController.oun.isSelected == false && DataController.instance_DataController.oun.isExisted == true)
        //{
        //    index++;
        //    NPCArray[index] = DataController.instance_DataController.rau.gameObject.transform;
        //}
    }


    void AddInteractionComponent() {

        for (int i = 0; i < NPCArray.Length; i ++)
        {
            if (NPCArray[i].gameObject.GetComponent<InteractionObj_stroke>() == null)
            {
                NPCArray[i].gameObject.AddComponent<InteractionObj_stroke>();
            }
        }

        for (int i = 0; i < NPCArray.Length; i++)
        {
            NPCInteractComponentList.Add(NPCArray[i].gameObject.GetComponent<InteractionObj_stroke>());
        }

    }


}
