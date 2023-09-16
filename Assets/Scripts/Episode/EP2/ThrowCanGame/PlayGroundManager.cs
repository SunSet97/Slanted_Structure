using System;
using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

        public enum ResultState
        {
            Perfect,
            Good,
            NotBad,
            Bad
        }
        
        [Header("Can")]
        [SerializeField] private GameObject canPrefab;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform destPoint;
        [FormerlySerializedAs("throwCans")] [SerializeField] private ThrowProps[] throwProps;

        [Header("Ring")]
        [SerializeField] private Button clearButton;
        [SerializeField] private RectTransform[] targets;
        [SerializeField] private RingCreator ringCreator;
        [Range(0, 1)] [SerializeField] private float noteSpeed;
        
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
        
        private void ThrowCan(ResultState type)
        {
            var can = Instantiate(canPrefab);
            can.GetComponent<Can>().OnCollisionEnter += () =>
            {
                if (type == ResultState.Perfect || type == ResultState.Good)
                {
                    Debug.Log($"성공 {type}");
                    Invoke(nameof(Success), 1f);
                }
                else
                {
                    Debug.Log($"실패 {type}");
                    Invoke(nameof(Fail), 1f);
                }
            };
            can.transform.position = startPoint.position;

            var index = Array.FindIndex(throwProps, item => item.resultType == type);

            var canDir = (destPoint.position - startPoint.position).normalized;
            canDir.y = throwProps[index].plusY;

            can.GetComponent<Collider>().isTrigger = false;

            var throwRigid = can.GetComponent<Rigidbody>();
            throwRigid.AddForce(canDir * throwProps[index].power, ForceMode.Impulse);
            throwRigid.AddTorque(canDir * throwProps[index].power, ForceMode.Impulse);
        }

        private void EndTimingGame(ResultState result)
        {
            ringCreator.End();
            ThrowCan(result);
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