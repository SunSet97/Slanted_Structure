using System;
using System.Collections;
using UnityEngine;
using Utility.Preference;
using Random = UnityEngine.Random;

namespace Utility.Core
{
    public enum CameraViewType
    {
        /// <summary>
        /// TrackObject
        /// </summary>
        FollowCharacter,
        FixedView
    }

    public enum FreezeType
    {
        X,
        Y,
        Z
    }

    public class CameraMoving : MonoBehaviour
    {
        [NonSerialized] public Transform TrackTransform;
        [NonSerialized] public CameraViewType ViewType;
        private Camera cam;

        // 카메라 무빙 경계
        private BoxCollider bound;
        private Vector3 minBound;
        private Vector3 maxBound;
        private float halfWidth;
        private float halfHeight;
        private Coroutine shakeCameraCoroutine;

        private bool isFreezeX;
        private bool isFreezeY;
        private bool isFreezeZ;

        private float freezeX;
        private float freezeY;
        private float freezeZ;

        public void Initialize(CameraViewType cameraViewType, Transform focusObj = null)
        {
            isFreezeX = false;
            isFreezeY = false;
            isFreezeZ = false;
            ViewType = cameraViewType;
            cam = Camera.main;
            TrackTransform = focusObj;
        }

        public void Freeze(FreezeType freezeType)
        {
            switch (freezeType)
            {
                case FreezeType.X:
                    isFreezeX = true;
                    freezeX = cam.transform.position.x;
                    break;
                case FreezeType.Y:
                    isFreezeY = true;
                    freezeY = cam.transform.position.y;
                    break;
                case FreezeType.Z:
                    isFreezeZ = true;
                    freezeZ = cam.transform.position.z;
                    break;
            }
        }

        public void UnFreeze()
        {
            isFreezeX = false;
            isFreezeY = false;
            isFreezeZ = false;
        }

        private void Update()
        {
            if (cam.orthographic)
            {
                cam.orthographicSize = DataController.Instance.camOrthographicSize;
            }

            var camTransform = cam.transform;
            camTransform.rotation = Quaternion.Euler(DataController.Instance.camOffsetInfo.camRot);
            PlayUIController.Instance.worldSpaceUI.rotation = camTransform.rotation;

            if (ViewType.Equals(CameraViewType.FixedView))
            {
                camTransform.position = DataController.Instance.CurrentMap.transform.position +
                                        DataController.Instance.camOffsetInfo.camDis;
            }
            else if (ViewType.Equals(CameraViewType.FollowCharacter))
            {
                camTransform.position = TrackTransform.position +
                                        DataController.Instance.camOffsetInfo.camDis;
            }

            if (isFreezeX)
            {
                var camPos = camTransform.position;
                camPos.x = freezeX;
                camTransform.position = camPos;
            }
            else if (isFreezeY)
            {
                var camPos = camTransform.position;
                camPos.y = freezeY;
                camTransform.position = camPos;
            }
            else if (isFreezeZ)
            {
                var camPos = camTransform.position;
                camPos.z = freezeZ;
                camTransform.position = camPos;
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