using UnityEngine;
using Utility.System;
using static Data.CustomEnum;

public class CameraMoving : MonoBehaviour
{
    //캐릭터 오브젝트 받는 변수
    public Transform mainCharacter;

    private CameraViewType viewType;
    private Camera cam;

    // 카메라 무빙 경계
    private BoxCollider bound;
    private Vector3 minBound;
    private Vector3 maxBound;
    private float halfWidth;
    private float halfHeight;

    public void Initialize()
    {
        viewType = DataController.instance.currentMap.cameraViewType;
        cam = Camera.main;
        var mainChar = DataController.instance.GetCharacter(Character.Main);
        if (mainChar != null)
        {
            mainCharacter = mainChar.transform;
        }
        else
        {
            mainCharacter = null;
        }
    }

    void Update()
    {
        if (!mainCharacter)
        {
            return;
        }

        if (cam.orthographic)
        {
            cam.orthographicSize = DataController.instance.camOrthgraphicSize;
        }

        if (viewType.Equals(CameraViewType.FixedView))
        {
            cam.transform.position = DataController.instance.currentMap.transform.position +
                                     DataController.instance.camInfo.camDis +
                                     DialogueController.instance.dialogueData.CamInfo.camDis;
        }

        if (mainCharacter != null)
        {
            if (viewType.Equals(CameraViewType.FollowCharacter))
            {
                cam.transform.position = mainCharacter.position + DataController.instance.camInfo.camDis +
                                         DialogueController.instance.dialogueData.CamInfo.camDis;
            }

            Follow_Player(cam.transform.position);
        }
    }


    void Follow_Player(Vector3 position)
    {
        cam.transform.rotation = Quaternion.Euler(DataController.instance.camInfo.camRot);
    }
}
