using Data;
using UnityEngine;
using Utility.Core;

namespace CommonScript
{
    public enum CameraViewType
    {
        FollowCharacter,
        FixedView
    }
    
    public class CameraMoving : MonoBehaviour
    {
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
            viewType = DataController.Instance.CurrentMap.cameraViewType;
            cam = Camera.main;
            var mainChar = DataController.Instance.GetCharacter(CustomEnum.Character.Main);
            if (mainChar)
            {
                mainCharacter = mainChar.transform;
            }
            else
            {
                mainCharacter = null;
            }
        }

        private void Update()
        {
            if (cam.orthographic)
            {
                cam.orthographicSize = DataController.Instance.camOrthgraphicSize;
            }

            cam.transform.rotation = Quaternion.Euler(DataController.Instance.camInfo.camRot + DataController.Instance.CamOffsetInfo.camRot);

            if (viewType.Equals(CameraViewType.FixedView))
            {
                cam.transform.position = DataController.Instance.CurrentMap.transform.position +
                                         DataController.Instance.camInfo.camDis +
                                         DialogueController.Instance.dialogueData.CamInfo.camDis +
                                         DataController.Instance.CamOffsetInfo.camDis;
            }

            if (mainCharacter && viewType.Equals(CameraViewType.FollowCharacter))
            {
                cam.transform.position = mainCharacter.position + DataController.Instance.camInfo.camDis +
                                         DialogueController.Instance.dialogueData.CamInfo.camDis +
                                         DataController.Instance.CamOffsetInfo.camDis;
            }

        }
    }
}