using UnityEngine;
using Utility.Core;

namespace Utility.Interaction
{
    public class FloatingMark : MonoBehaviour
    {
        [SerializeField] private GameObject markPrefab;
        [SerializeField] private Vector2 markOffset;

        private GameObject floatingMark;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");

            floatingMark = Instantiate(markPrefab, DataController.Instance.CurrentMap.ui);
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
                floatingMark.transform.position =
                    DataController.Instance.Cam.WorldToScreenPoint((Vector3)markOffset + transform.position);
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
