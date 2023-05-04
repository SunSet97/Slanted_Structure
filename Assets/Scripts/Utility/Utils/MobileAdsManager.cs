using GoogleMobileAds.Api;
using UnityEngine;

namespace Utility.Utils
{
    public static class MobileAdsManager
    {
        private const string ADUnitId = "ca-app-pub-3940256099942544/1033173712";

        private static InterstitialAd interstitialAd;

        public static int ADCount = 0;
        public const int CountPerAds = 4;

        public static void ShowAd()
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                interstitialAd.Show();

                // Audio 출력 중지, On Change Map Interaction은 이후에 실행
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }
        }

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        public static void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            DestroyAd();

            Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest.Builder()
                .AddKeyword("unity-admob-sample")
                .Build();

            // send the request to load the ad.
            InterstitialAd.Load(ADUnitId, adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    interstitialAd = ad;
                    RegisterEventHandlers(interstitialAd);
                    RegisterReloadHandler(interstitialAd);
                });
        }

        //Ad 사용이 끝나면 사용해야됨.
        private static void DestroyAd()
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }
        }

        private static void RegisterEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += adValue => { Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}."); };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () => { Debug.Log("Interstitial ad recorded an impression."); };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () => { Debug.Log("Interstitial ad was clicked."); };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () => { Debug.Log("Interstitial ad full screen content opened."); };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () => { Debug.Log("Interstitial ad full screen content closed."); };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        private static void RegisterReloadHandler(InterstitialAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
            };
        }
    }
}