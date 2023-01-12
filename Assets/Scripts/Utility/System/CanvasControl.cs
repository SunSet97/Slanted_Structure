using UnityEngine;
using UnityEngine.UI;

namespace Utility.System
{
    public class CanvasControl : MonoBehaviour
    {
        public Transform mapUI;
    
        public bool isInConverstation;

        private static CanvasControl _instance;

        public static CanvasControl instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;   
            }
            else
            {
                Destroy(this);
            }
        }

        void Start()
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
        }
    }
}
