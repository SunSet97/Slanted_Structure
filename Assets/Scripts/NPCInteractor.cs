using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteractor : MonoBehaviour
{
    public GameObject player;
    public GameObject cameraManager;
    public DialogueController dialogueController;
    public float interactableDistance = 2;
    

    private Transform[] NPCArray;

    void Start()
    {
        NPCArray = GetComponentsInChildren<Transform>();
        
    }

    void Update()
    {
        FindInteractableNPC(interactableDistance);
    }

    private void FindInteractableNPC(float interactableDist)
    {
        // 가장 가까운 NPC를 찾는다
        Transform closestNPC = getClosestNPC();
        

        if (isInteractable(closestNPC))
        {
            
            // 상호작용 가능 표시
            closestNPC.Rotate(new Vector3(0, 0, 200 * Time.deltaTime));


            // 상호작용 가능 할 때에만 npc와의 대화가 가능함 
            if (Input.GetMouseButtonDown(0))
            {
                int cameraIndex = 0;

                // 현재 활성화 되어있는 카메라를 찾음 
                for (int i = 0; i < 4; i ++)
                {
                    if (cameraManager.GetComponent<CameraManager>().toggle[i].isOn)
                    {
                        cameraIndex = i;
                        break;
                    }
                }
                Camera curCamera = cameraManager.transform.GetChild(cameraIndex).GetComponent<Camera>();

                // 카메라 ~ 터치 지점으로의 ray 
                Ray ray = curCamera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.gameObject.name == closestNPC.gameObject.name && dialogueController.isPossibleCnvs == true)
                    {
                        DataController.Instance.LoadData(closestNPC.gameObject.name, DataController.Instance.charData.story + "_" 
                            + DataController.Instance.charData.story_branch + "_" + DataController.Instance.charData.dialogue_index + ".json");

                        dialogueController.StartConversation();

                        // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                        dialogueController.isPossibleCnvs = false;
                    }
                        
                }
            } 

        }

    }

    private bool isInteractable(Transform NPC)
    {
        // 상호작용 거리 내에 있는지 확인
        float dist = Vector3.Distance(player.transform.position, NPC.transform.position);
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
            if (NPC.name == "NPCManager")
                continue;

            float dist = Vector3.Distance(player.transform.position, NPC.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestNPC = NPC;
            }
        }

        return closestNPC;
    }
}
