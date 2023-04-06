using System.Collections;
using Data.GamePlay;
using Episode.EP2.ThrowCanGame;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Preference;

namespace Episode.EP2.DoveFeedGame
{
    public class DoveFeedMiniGameManager : MiniGame
    {
        private enum ResultState
        {
            Perfect,
            Good,
            NotBad,
            Bad
        }
        
        [SerializeField] private Button clearButton;

        [SerializeField] private RingCreator ringCreator;

        private void Start()
        {
            clearButton.onClick.AddListener(ClearCheck);
        }

        public override void Play()
        {
            base.Play();
            StartTimingGame();
        }

        private void StartTimingGame()
        {
            ringCreator.rangeParent.SetActive(true);

            ringCreator.Setup();

            StartCoroutine(PlayTimingGame());
        }
        
        private IEnumerator PlayTimingGame()
        {
            var child = ringCreator.rangeParent.transform.GetChild(2).GetComponent<RectTransform>();
            while (child.localScale.x * 0.3 * child.rect.width <= ringCreator.Radius * 2)
            {
                ringCreator.DisplayUpdate();
                yield return null;
            }

            EndTimingGame(ResultState.Bad);
        }

        private void EndTimingGame(ResultState result)
        {
            ringCreator.rangeParent.SetActive(false);
            ringCreator.Reset();
            PlayUIController.Instance.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            FeedDove(result);
        }

        private void FeedDove(ResultState type)
        {
            // 먹이
        }

        private void ClearCheck()
        {
            if (!IsPlay)
            {
                return;
            }
            
            ResultState result;
            if (ringCreator.Radius * 2 <= ringCreator.rangeParent.transform.GetChild(2).localScale.x *
                ringCreator.rangeParent.transform.GetChild(2).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("유후! 바로 들어감");
                result = ResultState.Perfect;
            }
            else if (ringCreator.Radius * 2 <= ringCreator.rangeParent.transform.GetChild(1).localScale.x *
                     ringCreator.rangeParent.transform.GetChild(1).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("아싸! 한번 튕기고 들어감");
                result = ResultState.Good;
            }
            else if (ringCreator.Radius * 2 <= ringCreator.rangeParent.transform.GetChild(0).localScale.x *
                     ringCreator.rangeParent.transform.GetChild(0).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("아깝다! 한번 튕기고 안들어감");
                result = ResultState.NotBad;
            }
            else
            {
                Debug.Log("앗! 안들어감");
                result = ResultState.Bad;
            }

            StopAllCoroutines();
            EndTimingGame(result);
        }
    }
}
