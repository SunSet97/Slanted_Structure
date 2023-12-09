using System;
using System.Collections;
using System.Net.NetworkInformation;
using Episode.EP2.ThrowCanGame;
using UnityEngine;
using UnityEngine.UI;

namespace Episode.EP2
{
    public enum ResultState
    {
        Perfect,
        Good,
        NotBad,
        Bad
    }

    public class RingNoteGame : MonoBehaviour
    {
        public Button clearButton;

        [SerializeField] private RectTransform outerBadArea;
        [SerializeField] private RectTransform outerGoodArea;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private RectTransform innerGoodArea;
        [SerializeField] private RectTransform innerBadArea;
        [SerializeField] private RectTransform deadArea;

        [SerializeField] private RingCreator ringCreator;
        [Range(0, 1)] [SerializeField] private float noteSpeed;

        private Action<ResultState> OnEndAction;
        
        private void Start()
        {
            clearButton.onClick.AddListener(ClearCheck);
        }

        public void PlayGame(Action<ResultState> onEndAction)
        {
            OnEndAction = onEndAction;
            ringCreator.Initialize();
            StartCoroutine(UpdateGame());
        }

        private IEnumerator UpdateGame()
        {
            while (deadArea.localScale.x * deadArea.rect.width * 0.5f <= ringCreator.Radius)
            {
                ringCreator.DisplayUpdate(noteSpeed);
                yield return null;
            }

            ringCreator.End();
            OnEndAction(ResultState.Bad);
        }

        private void ClearCheck()
        {
            // if (!IsPlay)
            {
                // return;
            }

            ResultState result;
            if (ringCreator.Radius <= deadArea.localScale.x * deadArea.rect.width * 0.5f)
            {
                Debug.Log("앗! 안들어감");
                result = ResultState.Bad;
            }
            else if (ringCreator.Radius <= innerBadArea.localScale.x * innerBadArea.rect.width * 0.5f)
            {
                Debug.Log("아깝다! 한번 튕기고 안들어감");
                result = ResultState.NotBad;
            }
            else if (ringCreator.Radius <= innerGoodArea.localScale.x * innerGoodArea.rect.width * 0.5f)
            {
                Debug.Log("아싸! 한번 튕기고 들어감");
                result = ResultState.Good;
            }
            else if (ringCreator.Radius * 2 <= safeArea.localScale.x * safeArea.rect.width)
            {
                Debug.Log("유후! 바로 들어감");
                result = ResultState.Perfect;
            }
            else if (ringCreator.Radius <= outerGoodArea.localScale.x * outerGoodArea.rect.width * 0.5f)
            {
                Debug.Log("아싸! 한번 튕기고 들어감");
                result = ResultState.Good;
            }
            else if (ringCreator.Radius <= outerBadArea.localScale.x * outerBadArea.rect.width * 0.5f)
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
            ringCreator.End();
            OnEndAction?.Invoke(result);
        }
    }
}