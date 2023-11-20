using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Save;
using Utility.Utils;

namespace TitleScene
{
    public class TitleSceneManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private string mapCode;
        [SerializeField] private int step;

        [SerializeField] private Button newStartButton;

        [SerializeField] private Button diaryButton;
        [SerializeField] private Button diaryExitButton;

        [SerializeField] private GameObject diaryPanel;
#pragma warning restore 0649
        
        private void Start()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("MobileAds Initialized");
                // This callback is called once the MobileAds SDK is initialized.
                MobileAdsManager.LoadInterstitialAd();
            });
            
            newStartButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.AddOnLoadListener(() =>
                {
                    if (Application.isEditor)
                    {
                        DataController.Instance.GameStart(mapCode, step);
                    }
                    else
                    {
                        DataController.Instance.GameStart();
                    }
                    SceneLoader.Instance.RemoveAllListener();
                });
                
                SceneLoader.Instance.LoadScene("Ingame_set");
            });
            diaryButton.onClick.AddListener(() => { diaryPanel.SetActive(true); });
            diaryExitButton.onClick.AddListener(() => { diaryPanel.SetActive(false); });

            for (var i = 0; i < 10; i++)
            {
                SaveManager.LoadCover(i);
            }
        }
    }
}