using UnityEngine;
using Utility.Core;
using Utility.UI;
using Utility.Utils;

namespace Utility.Interaction
{
    public class FloatingMark : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject markPrefab;
        [SerializeField] private Vector3 markOffset;
        [SerializeField] private bool isWorld;
#pragma warning restore 0649

        private GameObject floatingMark;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void OnDestroy()
        {
            if (floatingMark)
            {
                ActiveFloatingMark(false);
            }
        }

        private void Update()
        {
            if (!floatingMark) return;

            if (isWorld)
            {
                floatingMark.transform.position = markOffset + transform.position;
                var transformEulerAngles = floatingMark.transform.eulerAngles;
                transformEulerAngles.y = DataController.Instance.Cam.transform.rotation.y;
                floatingMark.transform.eulerAngles = transformEulerAngles;
            }
            else
            {
                var screenPoint =
                    DataController.Instance.Cam.WorldToScreenPoint(transform.position + markOffset);
                floatingMark.transform.position = screenPoint;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (markPrefab)
            {
                ActiveFloatingMark(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (markPrefab)
            {
                ActiveFloatingMark(false);
            }
        }

        private void ActiveFloatingMark(bool isActive)
        {
            if (isActive)
            {
                floatingMark = ObjectPoolHelper.Get(markPrefab);
                if (isWorld)
                {
                    floatingMark.transform.parent = PlayUIController.Instance.worldSpaceUI;
                }
                else
                {
                    floatingMark.transform.parent = PlayUIController.Instance.mapUi;
                }

                floatingMark.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                floatingMark.SetActive(true);
            }
            else
            {
                floatingMark.SetActive(false);
                floatingMark.transform.parent = null;
                ObjectPoolHelper.Release(markPrefab, floatingMark);
                floatingMark = null;
            }
        }
    }
}