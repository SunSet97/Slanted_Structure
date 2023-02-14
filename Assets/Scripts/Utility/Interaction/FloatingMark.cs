using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Property;

namespace Utility.Interaction
{
    public class FloatingMark : MonoBehaviour
    {
        [SerializeField] private GameObject markPrefab;
        [SerializeField] private Vector2 markOffset;

        private GameObject _floatingMark;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");

            _floatingMark = Instantiate(markPrefab, DataController.instance.currentMap.ui);
            _floatingMark.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_floatingMark)
            {
                Destroy(_floatingMark);
            }
        }

        private void Update()
        {
            if (_floatingMark.activeSelf)
            {
                _floatingMark.transform.position =
                    DataController.instance.cam.WorldToScreenPoint((Vector3)markOffset + transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_floatingMark)
            {
                _floatingMark.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_floatingMark)
            {
                _floatingMark.SetActive(false);
            }
        }
    }
}
