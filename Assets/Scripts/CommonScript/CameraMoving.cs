using UnityEngine;
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
            cam.orthographicSize = DataController.instance.orthgraphic_Size;
        }

        if (viewType.Equals(CameraViewType.FixedView))
        {
            cam.transform.position = DataController.instance.currentMap.transform.position +
                                     DataController.instance.camInfo.camDis;
        }

        if (mainCharacter != null)
        {
            if (viewType.Equals(CameraViewType.FollowCharacter))
            {
                cam.transform.position = mainCharacter.position + DataController.instance.camInfo.camDis +
                                         DialogueController.instance.dialogueCameraPos;
            }

            Follow_Player(cam.transform.position);
        }
    }


    void Follow_Player(Vector3 position)
    {
        //카메라 위치 제한 설정
        //position.x = Mathf.Clamp(position.x,min_x,max_x);
        //position.y= Mathf.Clamp(position.y, min_y, max_y);

        if (DataController.instance.currentMap.cameraBound) // 카메라 경계 설정 시
        {
            bound = DataController.instance.currentMap.cameraBound;
            minBound = bound.bounds.min;
            maxBound = bound.bounds.max;
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * Screen.width / Screen.height;
            float clampedX = Mathf.Clamp(transform.position.x, minBound.x + halfWidth, maxBound.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, minBound.y + halfHeight, maxBound.y - halfHeight); ;
            //float clampedZ;
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        }

        //입력 된 카메라 각도 설정
        cam.transform.rotation = Quaternion.Euler(DataController.instance.camInfo.camRot);

        ////카메라의 Z값 고정을 위한 If문
        ////if (position.z != Z)
        //if (charPos.z - position.z != Z)
        //{
        //    position_storage = charPos.z - Z;
        //    position.z = position_storage;
        //    Camera.main.transform.position = position;
        //}
    }

}
