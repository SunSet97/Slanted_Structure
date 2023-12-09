using UnityEngine;
using Utility.Game;

namespace Episode.EP2.DoveFeedGame
{
    public class DoveFeedMiniGameManager : MiniGame
    {
#pragma warning disable 0649
        [Header("Ring")] [SerializeField] private RingNoteGame ringNoteGame;
#pragma warning restore 0649
        
        [Header("비둘기")]
        [SerializeField] private Animator dove;
        [SerializeField] private Animator doveUiAnimator;
        
        private static readonly int EatHash = Animator.StringToHash("Eat");
        private static readonly int PatternHash = Animator.StringToHash("Pattern");
        private static readonly int PreenHash = Animator.StringToHash("Preen");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int PlayHash = Animator.StringToHash("Play");

        public override void Play()
        {
            base.Play();
            
            ringNoteGame.PlayGame(EndRingNoteGame);
        }
        
        private void EndRingNoteGame(ResultState result)
        {
            FeedDove(result);
        }
        
        private void FeedDove(ResultState type)
        {
            // 먹이 주기
            if (type == ResultState.Perfect || type == ResultState.Good)
            {
                switch (type)
                {
                    case ResultState.Perfect:
                        doveUiAnimator.SetInteger(PatternHash, 1);
                        break;
                    case ResultState.Good:
                        doveUiAnimator.SetInteger(PatternHash, 2);
                        break;
                }
                
                dove.SetTrigger(EatHash);
                doveUiAnimator.SetTrigger(PlayHash);
                Debug.Log($"성공 {type}");
                Invoke(nameof(Success), 1f);
            }
            else
            {
                switch (type)
                {
                    case ResultState.NotBad:
                        dove.SetTrigger(PreenHash);
                        doveUiAnimator.SetInteger(PatternHash, 3);
                        break;
                    case ResultState.Bad:
                        dove.SetTrigger(AttackHash);
                        doveUiAnimator.SetInteger(PatternHash, 4);
                        break;
                }
                doveUiAnimator.SetTrigger(PlayHash);
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
