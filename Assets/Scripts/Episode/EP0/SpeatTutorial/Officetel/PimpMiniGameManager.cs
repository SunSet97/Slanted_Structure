using Data.GamePlay;
using UnityEngine;
using Utility.Core;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class PimpMiniGameManager : MiniGame
    {
        public PimpGuest[] pimpGuests;

        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Start()
        {
            foreach (var t in pimpGuests)
            {
                t.Init(this);
            }
        }

        private void FixedUpdate()
        {
            foreach (var t in pimpGuests)
            {
                t.Move(IsPlay);
            }
        }


        public override void Play()
        {
            base.Play();
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            foreach (var t in pimpGuests)
            {
                t.Think();
            }
        }

        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            foreach (var pimpGuest in pimpGuests)
            {
                var animator = pimpGuest.GetComponent<Animator>();
                animator.SetFloat(Speed, 0f);
            }
        }
    }
}
