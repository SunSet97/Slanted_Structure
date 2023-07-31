using UnityEngine;
using Utility.Core;
using Utility.Preference;

namespace Utility.Interaction
{
    public class FloatingMark : MonoBehaviour
    {
        [SerializeField] private GameObject markPrefab;
        [SerializeField] private Vector2 markOffset;
        [SerializeField] private bool isWorld;
        private GameObject floatingMark;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");

            floatingMark = Instantiate(markPrefab, PlayUIController.Instance.worldSpaceUI);
            floatingMark.SetActive(false);
        }

        private void OnDestroy()
        {
            if (floatingMark)
            {
                Destroy(floatingMark);
            }
        }

        private void Update()
        {
            if (floatingMark.activeSelf)
            {
                if (isWorld)
                {
                    floatingMark.transform.position = (Vector3)markOffset + transform.position;
                }
                else
                {
                    var screenPoint =
                        DataController.Instance.Cam.WorldToScreenPoint(transform.position + (Vector3)markOffset);
                    floatingMark.transform.position = screenPoint;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (floatingMark)
            {
                floatingMark.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (floatingMark)
            {
                floatingMark.SetActive(false);
            }
        }
    }
}