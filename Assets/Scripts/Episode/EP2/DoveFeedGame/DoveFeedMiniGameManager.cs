using UnityEngine;
using Utility.Game;

namespace Episode.EP2.DoveFeedGame
{
    public class DoveFeedMiniGameManager : MiniGame
    {
#pragma warning disable 0649
        [Header("Ring")] [SerializeField] private RingNoteGame ringNoteGame;
#pragma warning restore 0649
        
        // [Header("비둘기")]
        // [SerializeField] private Animator dove;
        
        public override void Play()
        {
            base.Play();
            
            ringNoteGame.PlayGame(EndTimingGame);
        }
        
        private void EndTimingGame(ResultState result)
        {
            FeedDove(result);
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
