using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Core;

namespace Episode.EP2.CoinTossGame
{
    public class CoinTossGameManager : Game
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform arrow;
        
        [Header("-Toss Button")] 
        [SerializeField] private Button gameStartButton;
        [SerializeField] private EventTrigger tossButtonTrigger;

        [Header("-Toss Direction Ctrl")] [SerializeField]
        private float rotateSpeed = 1.5f;

        [Header("-Toss Force Ctrl")] [SerializeField]
        private float powerSpeed = 3f;

        [SerializeField] private float maxPower = .5f;
        [SerializeField] private float minPower = .3f;

        private int tossStep;
        private int tryNum;

        private const float MaxScale = 1.5f;
        private const float MinScale = .5f;

        private void Start()
        {
            var onPointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            var onPointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            
            onPointerDown.callback.AddListener(OnPointerDown);
            onPointerUp.callback.AddListener(OnPointerUp);

            tossButtonTrigger.triggers.Add(onPointerDown);
            tossButtonTrigger.triggers.Add(onPointerUp);
            
            gameStartButton.onClick.AddListener(StartButton);
        }

        public override void Play()
        {
            base.Play();
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            gameStartButton.gameObject.SetActive(true);
            tossButtonTrigger.gameObject.SetActive(false);
            arrow.gameObject.SetActive(true);
            Reset();
        }

        // 5번 넘게 실패할 경우 EndPlay
        public override void EndPlay()
        {
            base.EndPlay();
            arrow.gameObject.SetActive(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
        }

        private void Reset()
        {
            arrow.localScale = Vector3.one * 0.1f;
            tossStep = 0;
        }

        private void StartButton()
        {
            gameStartButton.gameObject.SetActive(false);
            tossButtonTrigger.gameObject.SetActive(true);
            arrow.gameObject.SetActive(true);
        }

        private void OnPointerDown(BaseEventData _)
        {
            if (tossStep == 0)
            {
                StartCoroutine(Angle());
            }
            else if(tossStep == 1)
            {
                StartCoroutine(Power());
            }
        }

        private void OnPointerUp(BaseEventData _)
        {
            StopAllCoroutines();

            if (tossStep == 0)
            {
                tossStep++;
            }
            else if(tossStep == 1)
            {
                tryNum++; // 시도 횟수 증가
                tossButtonTrigger.gameObject.SetActive(false);

                var scaleRatio = Mathf.InverseLerp(MinScale, MaxScale, arrow.localScale.x);
                var tossPow = Mathf.Lerp(minPower, maxPower, scaleRatio);
                // 던지는 위치 변경
                var tossedCoin = Instantiate(coinPrefab, arrow.position, Quaternion.identity).GetComponent<Coin>();
                tossedCoin.Init(arrow.right.normalized, tossPow, 1f, () =>
                {
                    if (tryNum >= 5)
                    {
                        Debug.Log("끝남");
                        // EndPlay();
                    }
                    // else
                    {
                        Reset();
                        tossButtonTrigger.gameObject.SetActive(true);
                    }
                });
            }
        }

        // 각도조절, 이름 바꿔라 제발
        private IEnumerator Angle()
        {
            var t = 0f;
            while (true)
            {
                t += Time.deltaTime * rotateSpeed;
                arrow.eulerAngles = Vector3.forward * (Mathf.Sin(t) * 45f + 45f);

                yield return null;
            }
        }

        // 힘조절, 이름 바꿔라 제발
        private IEnumerator Power()
        {
            var t = 0f;
            while (true)
            {
                t += Time.deltaTime * powerSpeed;
                var localScale = arrow.localScale;
                localScale.x = (Mathf.Sin(t) * (MaxScale - MinScale) + (MinScale + MaxScale)) / 2f;
                arrow.localScale = localScale;

                yield return null;
            }
        }
    }
}