using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Ending
{
    public enum EndingType
    {
        None = 0,
        Normal = 1,
        Bad = 2,
        Special = 3,
        Happy = 4
    }
    
    [Serializable]
    public struct Ending
    {
        public Sprite endingSprite;
        public EndingType endingType;
    }
    
    public class EndingHelper : MonoBehaviour
    {
        private static EndingHelper _instance;

        public static EndingHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<EndingHelper>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private Ending[] endings;
        
        private static EndingHelper Create()
        {
            var sceneLoaderPrefab = Resources.Load<EndingHelper>("EndingHelper");
            return Instantiate(sceneLoaderPrefab);
        }

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                // SceneLoader.SceneLoader.Instance.LoadScene("TitleScene");
                // 홈버튼이었는지 맵 초기화였는지 기억이 안남.
                
            });
        }


        public void StartEnd(EndingType endingType)
        {
            var ending = Array.Find(endings, item => item.endingType == endingType);
            image.sprite = ending.endingSprite;
        }
    }
}