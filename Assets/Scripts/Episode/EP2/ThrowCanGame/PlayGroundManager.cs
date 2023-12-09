using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Game;

namespace Episode.EP2.ThrowCanGame
{
    public class PlayGroundManager : MiniGame
    {
        [Serializable]
        public struct ThrowProps
        {
            public float power;
            [Range(0, 1)] public float plusY;
            public ResultState resultType;
        }
        
#pragma warning disable 0649
        [Header("Can")]
        [SerializeField] private GameObject canPrefab;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform destPoint;
        [FormerlySerializedAs("throwCans")] [SerializeField] private ThrowProps[] throwProps;

        [Header("Ring")] [SerializeField] private RingNoteGame ringNoteGame;
#pragma warning restore 0649

        public override void Play()
        {
            base.Play();
            
            ringNoteGame.PlayGame(EndTimingGame);
        }

        private void EndTimingGame(ResultState result)
        {
            ThrowCan(result);
        }
        
        private void ThrowCan(ResultState result)
        {
            var can = Instantiate(canPrefab);
            can.GetComponent<Can>().OnCollisionEnter += () =>
            {
                if (result == ResultState.Perfect || result == ResultState.Good)
                {
                    Debug.Log($"성공 {result}");
                    Invoke(nameof(Success), 1f);
                }
                else
                {
                    Debug.Log($"실패 {result}");
                    Invoke(nameof(Fail), 1f);
                }
            };
            can.transform.position = startPoint.position;

            var index = Array.FindIndex(throwProps, item => item.resultType == result);

            var canDir = (destPoint.position - startPoint.position).normalized;
            canDir.y = throwProps[index].plusY;

            can.GetComponent<Collider>().isTrigger = false;

            var throwRigid = can.GetComponent<Rigidbody>();
            throwRigid.AddForce(canDir * throwProps[index].power, ForceMode.Impulse);
            throwRigid.AddTorque(canDir * throwProps[index].power, ForceMode.Impulse);
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