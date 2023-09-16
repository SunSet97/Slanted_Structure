using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Utils;

namespace Utility.Core.Ending
{
    [Serializable]
    public struct Ending
    {
        public Sprite endingSprite;
        public int endingIndex;
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
                }

                return _instance;
            }
        }

        [SerializeField] private Button button;
        [FormerlySerializedAs("image")] [SerializeField] private Image endingImage;
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