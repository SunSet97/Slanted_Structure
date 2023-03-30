using System.Collections;
using UnityEngine;
using Utility.Core;

namespace CommonScript
{
    public enum CameraViewType
    {
        FollowCharacter,
        FixedView,
        FocusObject
    }
    
    public class CameraMoving : MonoBehaviour
    {
        private Transform focusObject;
        private CameraViewType viewType;
        private Camera cam;

        // 카메라 무빙 경계
        private BoxCollider bound;
        private Vector3 minBound;
        private Vector3 maxBound;
        private float halfWidth;
        private float halfHeight;
        private Coroutine shakeCameraCoroutine;

        public void Initialize(CameraViewType cameraViewType, Transform focusObj = null)
        {
            viewType = cameraViewType;
            cam = Camera.main;
            focusObject = focusObj;
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
                                         // DialogueController.Instance.dialogueData.CamInfo.camDis +
                                         DataController.Instance.CamOffsetInfo.camDis;
            }
            else if (viewType.Equals(CameraViewType.FollowCharacter))
            {
                cam.transform.position = focusObject.position +
                                         DataController.Instance.camInfo.camDis +
                                         // DialogueController.Instance.dialogueData.CamInfo.camDis +
                                         DataController.Instance.CamOffsetInfo.camDis;
            }
            else if (viewType.Equals(CameraViewType.FocusObject))
            {
                cam.transform.position = focusObject.position +
                                         DataController.Instance.CamOffsetInfo.camDis;
            }
        }

        public void Shake(float shakeTime, float shakeAmount)
        {
            if (shakeCameraCoroutine != null)
            {
                StopCoroutine(shakeCameraCoroutine);
            }

            shakeCameraCoroutine = StartCoroutine(ShakeCamera(shakeTime, shakeAmount));
        }

        private IEnumerator ShakeCamera(float shakeTime, float shakeAmount)
        {
            var camTransform = transform.parent;
            var originVector3 = camTransform.localPosition;

            var t = 0f;
            while (t <= shakeTime)
            {
                var randomPoint = originVector3 + (Vector3)Random.insideUnitCircle * shakeAmount;
                camTransform.localPosition = randomPoint;
                yield return null;

                t += Time.deltaTime;
            }

            camTransform.localPosition = originVector3;
            shakeCameraCoroutine = null;
        }
    }
}