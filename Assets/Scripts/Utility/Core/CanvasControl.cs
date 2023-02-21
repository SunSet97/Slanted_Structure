using UnityEngine;
using UnityEngine.UI;

namespace Utility.Core
{
    public class CanvasControl : MonoBehaviour
    {
        public Transform mapUI;

        private static CanvasControl _instance;

        public static CanvasControl instance => _instance;

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(_instance);   
            }
        }

        void Start()
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
        }
    }
}
