using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractor : MonoBehaviour
{
    public GameObject player;
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
        
        if(isInteractable(closestNPC))
        {
            // 상호작용 가능 표시
            closestNPC.Rotate(new Vector3(0, 0, 200 * Time.deltaTime));
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
