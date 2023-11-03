using UnityEngine;
using Utility.Core;
using Utility.UI.Preference;

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
            if (isWorld)
            {
                floatingMark = Instantiate(markPrefab, PlayUIController.Instance.worldSpaceUI);
            }
            else
            {
                floatingMark = Instantiate(markPrefab, PlayUIController.Instance.mapUi);
            }

            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
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
                    floatingMark.transform.position = markOffset + transform.position;
                }
                else
                {
                    var screenPoint =
                        DataController.Instance.Cam.WorldToScreenPoint(transform.position + markOffset);
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