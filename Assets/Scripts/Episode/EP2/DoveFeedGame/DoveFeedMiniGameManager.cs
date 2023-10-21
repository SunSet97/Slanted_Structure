using System.Collections;
using Episode.EP2.ThrowCanGame;
using UnityEngine;
using UnityEngine.UI;
using Utility.Game;

namespace Episode.EP2.DoveFeedGame
{
    public class DoveFeedMiniGameManager : MiniGame
    {
        private enum ResultState
        {
            Perfect = 1,
            Good = 2,
            NotBad = 3,
            Bad = 4
        }
        
        [Header("Ring")]
        [SerializeField] private Button clearButton;
        [SerializeField] private RingCreator ringCreator;
        [SerializeField] private RectTransform[] targets;
        [Range(0, 1)] [SerializeField] private float noteSpeed;

        // [Header("비둘기")]
        // [SerializeField] private Animator dove;
        
        private void Start()
        {
            clearButton.onClick.AddListener(ClearCheck);
        }

        public override void Play()
        {
            base.Play();
            
            ringCreator.Initialize();
            StartCoroutine(UpdateGame());
        }
        
        private IEnumerator UpdateGame()
        {
            while (targets[2].localScale.x * 0.3 * targets[2].rect.width <= ringCreator.Radius * 2)
            {
                ringCreator.DisplayUpdate(noteSpeed);
                yield return null;
            }

            EndTimingGame(ResultState.Bad);
        }

        private void FeedDove(ResultState type)
        {
            // 먹이 주기
            if (type == ResultState.Perfect || type == ResultState.Good)
            {
                if (type == ResultState.Perfect)
                {
                    // dove.SetInteger("Attack", (int)type);
                }
                else if (type == ResultState.Good)
                {
                    // dove.SetInteger("Attack", (int)type);
                }

                Debug.Log($"성공 {type}");
                Invoke(nameof(Success), 1f);
            }
            else
            {
                if (type == ResultState.NotBad)
                {
                    // dove.SetInteger("Attack", (int)type);
                }
                else if (type == ResultState.Bad)
                {
                    // dove.SetInteger("Attack", (int)type);
                }

                Debug.Log($"실패 {type}");
                Invoke(nameof(Fail), 1f);
            }
        }

        private void ClearCheck()
        {
            if (!IsPlay)
            {
                return;
            }
            
            ResultState result;
            if (ringCreator.Radius * 2 <= targets[2].localScale.x * targets[2].rect.width)
            {
                Debug.Log("유후! 바로 들어감");
                result = ResultState.Perfect;
            }
            else if (ringCreator.Radius * 2 <= targets[1].localScale.x * targets[1].rect.width)
            {
                Debug.Log("아싸! 한번 튕기고 들어감");
                result = ResultState.Good;
            }
            else if (ringCreator.Radius * 2 <= targets[0].localScale.x * targets[0].rect.width)
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
        
        private void EndTimingGame(ResultState result)
        {
            ringCreator.End();
            FeedDove(result);
        }
        
        private void Success()
        {
            EndPlay(true);
        }
        
        private void Fail()
        {
            EndPlay(true);
        }
    }
}
