using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteractor : MonoBehaviour
{
    public GameObject player;
    //public GameObject cameraManager;
    CanvasControl canvasCtrl;
    public float interactableDistance = 2;

    public Transform[] NPCArray;

    private void Awake()
    {
        FindPlayer();
        
    }

    private void Start()
    {
        canvasCtrl = GameObject.Find("Canvas").GetComponent<CanvasControl>();
        FindNPC();
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
                        DataController.instance_DataController.LoadData(closestNPC.gameObject.name, DataController.instance_DataController.charData.story + "_" 
                            + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_" 
                            + DataController.instance_DataController.charData.dialogue_index + ".json");

                        canvasCtrl.StartConversation();

                        // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                        canvasCtrl.isPossibleCnvs = false;
                    }
                        
                }
            } 

        }

    }

    private bool isInteractable(Transform NPC)
    {
        // 상호작용 거리 내에 있는지 확인
        float dist = Vector3.Distance(player.transform.position, NPC.position);
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
            if (NPC != null)
            {
                float dist = Vector3.Distance(player.transform.position, NPC.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestNPC = NPC;
                }
            }
        }

        return closestNPC;
    }

    private void FindPlayer()
    {
        player = GameObject.Find(DataController.instance_DataController.charData.curPlayer).gameObject;

        player.tag = "Player";
        player.GetComponent<CharacterManager>().isSelected = true;
        player.GetComponent<CharacterManager>().enabled = true;
    }

    private void FindNPC()
    {
        int npcCnt = transform.childCount;
        NPCArray = new Transform[npcCnt];
        for (int i = 0; i < npcCnt; i++)
        {
            if (transform.GetChild(i).CompareTag("NPC"))
            {
                NPCArray[i] = transform.GetChild(i);
                
            }
        }
    }
}
