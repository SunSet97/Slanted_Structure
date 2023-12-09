using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Utility.Utils
{
    [Serializable]
    public struct Ending
    {
        public int endingIndex;
        public Sprite endingSprite;
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
        
#pragma warning disable 0649
        [SerializeField] private Button button;
        [FormerlySerializedAs("image")] [SerializeField] private Image endingImage;
        [SerializeField] private Ending[] endings;
#pragma warning restore 0649
        
        private static EndingHelper Create()
        {
            var sceneLoaderPrefab = Resources.Load<EndingHelper>("EndingHelper");
            return Instantiate(sceneLoaderPrefab);
        }

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("TitleScene");
            });
        }


        public void StartEnd(int endingIndex)
        {
            gameObject.SetActive(true);
            var ending = Array.Find(endings, item => item.endingIndex == endingIndex);
            endingImage.sprite = ending.endingSprite;
        }
    }
}