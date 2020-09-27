using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using UnityEngine;

public enum mapType
{
    RauTutorial,
    SpeatTutorial
}

public class CollisionEventController : MonoBehaviour
{
    private CanvasControl canvasCtrl;
    private mapType type;
    private CharacterController ctrl;

    public void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;
        ctrl = DataController.instance_DataController.currentChar.GetComponent<CharacterController>();
    }

    public void setTypeRauTutorial()
    {
        type = mapType.RauTutorial;
    }
    
    //Tag: ScriptCollider
    public void script(GameObject go)
    {
        canvasCtrl.isGoNextStep = true;
        var value = go.transform.parent.name + "/" + type.ToString() + "/";
        DataController.instance_DataController.LoadData(value,go.name + ".json");
        canvasCtrl.progressIndex++;
        canvasCtrl.isPossibleCnvs = false;
        canvasCtrl.StartConversation();
    }

    //Tag: ChangeAngle
    public void changeAngle(CameraTransform ct)
    {
        DataController.instance_DataController.camDis_x = ct.dis_X;
        DataController.instance_DataController.camDis_z = ct.dis_Z;
        DataController.instance_DataController.rot = ct.rotation;
    }

    public void colliderNext()
    {
        canvasCtrl.isGoNextStep = true;
        canvasCtrl.progressIndex++;
        canvasCtrl.GoNextStep();
    }

    // Tag: CommandCollider
    public void tutorialCmd()
    {
        canvasCtrl.TutorialCmdCtrl();
    }

    public void Interaction(GameObject go)
    {
        
        canvasCtrl.progressIndex++;

        InteractionList list = go.GetComponent<InteractionList>();
        int listLen = list.activeList.Length;

        if (go.name == "ActiveInteraction")
        {
            canvasCtrl.isGoNextStep = false;
            for (int i = 0; i < listLen; i++)
            {
                list.activeList[i].GetComponent<InteractObjectControl>().isInteractable = true;
            }
        }
        else if (go.gameObject.name == "DisableInteraction")// 인터랙션 비활성화 
        {
            canvasCtrl.isGoNextStep = true;
            for (int i = 0; i < listLen; i++)
            {
                list.activeList[i].GetComponent<InteractObjectControl>().isInteractable = false;
                canvasCtrl.GoNextStep();
            }
        }
        else if (go.gameObject.name == "MiniGame")
        {
            canvasCtrl.isGoNextStep = true;
            list.activeList[0].SetActive(true);
            list.activeList[0].GetComponent<InteractObjectControl>().playMiniGame = true;

        }
    }

    public void setFalseCurrentCollider(GameObject currentCollider)
    {
        currentCollider.SetActive(false);
    }

    // 라우 respawn 장소 저장
    public void SaveRespawnPos(GameObject rp)
    {
        DataController.instance_DataController.charData.respawnLocation = rp.transform.position;
    }

    public void Die()
    {
        StartCoroutine(DieAction());
    }

    // 라우가 물에 빠지면 일어나는 코루틴 
    IEnumerator DieAction()
    {
        Material mat = this.GetComponentInChildren<SkinnedMeshRenderer>().material;
        Color color = mat.color;
      
        // 플레이어가 죽었음을 보여줌 (반투명)
        mat.color = new Color(color.r, color.g, color.b, 0.5f);
        yield return new WaitForSeconds(2);

        // 캐릭터컨트롤러를 끄고 플레이어를 리스폰 위치로 이동시킴
        ctrl.enabled = false;
        transform.position = DataController.instance_DataController.charData.respawnLocation;

        // 투명도를 원상태로 복귀
        mat.color = new Color(color.r, color.g, color.b, 1f);


        // 플레이어가 부활한 후 다시 캐릭터컨트롤러 활성화
        ctrl.enabled = true;
     
        StopCoroutine(DieAction());
    }
}
