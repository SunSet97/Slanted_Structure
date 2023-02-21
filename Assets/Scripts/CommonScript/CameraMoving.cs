using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace CommonScript
{
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
            var mainChar = DataController.Instance.GetCharacter(Character.Main);
            if (mainChar)
            {
                mainCharacter = mainChar.transform;
            }
            else
            {
                mainCharacter = null;
                Debug.LogWarning("오류오류 메인캐릭터 없음");
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
                cam.orthographicSize = DataController.Instance.camOrthgraphicSize;
            }

            if (viewType.Equals(CameraViewType.FixedView))
            {
                cam.transform.position = DataController.Instance.CurrentMap.transform.position +
                                         DataController.Instance.camInfo.camDis +
                                         DialogueController.instance.dialogueData.CamInfo.camDis;
            }

            if (viewType.Equals(CameraViewType.FollowCharacter))
            {
                cam.transform.position = mainCharacter.position + DataController.Instance.camInfo.camDis +
                                         DialogueController.instance.dialogueData.CamInfo.camDis;
            }

            cam.transform.rotation = Quaternion.Euler(DataController.Instance.camInfo.camRot);
        }
    }
}
