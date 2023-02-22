using System;
using Play;
using UnityEngine;
using Utility.Core;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class PimpGameManager : MonoBehaviour, IGamePlayable
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

        public bool IsPlay { get; set; }
        public Action ONEndPlay { get; set; }

        public void Play()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            IsPlay = true;
            foreach (var t in pimpGuests)
            {
                t.Think();
            }
        }

        public void EndPlay()
        {
            IsPlay = false;
            ONEndPlay?.Invoke();
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            foreach (var pimpGuest in pimpGuests)
            {
                var animator = pimpGuest.GetComponent<Animator>();
                animator.SetFloat(Speed, 0.0f);
            }
        }
    }
}
