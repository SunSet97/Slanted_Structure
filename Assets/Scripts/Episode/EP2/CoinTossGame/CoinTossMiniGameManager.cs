using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Core;

namespace Episode.EP2.CoinTossGame
{
    public class CoinTossMiniGameManager : MiniGame
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform arrow;
        
        [SerializeField] private Button gameStartButton;
        
        [Header("-Toss")] 
        [SerializeField] private EventTrigger tossButtonTrigger;

        [SerializeField] private float rotateSpeed = 1.5f;
        [SerializeField] private float powerSpeed = 3f;

        [SerializeField] private float maxPower = .5f;
        [SerializeField] private float minPower = .3f;

        [SerializeField] private float maxScale = 1.5f;
        [SerializeField] private float minScale = .5f;
        
        private int tossStep;
        private int tryNum;

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
        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            arrow.gameObject.SetActive(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
        }
        
        private void Reset()
        {
            StartCoroutine(ResetCoroutine());
            tossStep = 0;
        }

        private IEnumerator ResetCoroutine()
        {
            var forward = Vector3.forward * 45f;
            while (!Mathf.Approximately(Vector3.Distance(arrow.localScale, Vector3.one), 0f))
            {
                arrow.eulerAngles = Vector3.Lerp(arrow.eulerAngles, forward, .05f);
                arrow.localScale = Vector3.Lerp(arrow.localScale, Vector3.one, .05f);
                yield return null;
            }
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
            else if (tossStep == 1)
            {
                tryNum++;
                tossButtonTrigger.gameObject.SetActive(false);

                var scaleRatio = Mathf.InverseLerp(minScale, maxScale, arrow.localScale.x);
                var tossPow = Mathf.Lerp(minPower, maxPower, scaleRatio);
                // 던지는 위치 변경
                var tossedCoin = Instantiate(coinPrefab, arrow.position, Quaternion.identity).GetComponent<Coin>();
                tossedCoin.Init(arrow.right.normalized, tossPow, 1f, () =>
                    {
                        if (tryNum >= 5)
                        {
                            Debug.Log("끝남");
                            EndPlay(false);
                        }
                        else
                        {
                            Reset();
                            tossButtonTrigger.gameObject.SetActive(true);
                        }
                    },
                    isClear =>
                    {
                        if (isClear)
                        {
                            // 성공
                        }
                        else
                        {
                            // 실패
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
            var t = 180f * Mathf.Deg2Rad;
            while (true)
            {
                t += Time.deltaTime * powerSpeed;
                var localScale = arrow.localScale;
                localScale.x = (Mathf.Sin(t) * (maxScale - minScale) + (minScale + maxScale)) / 2f;
                arrow.localScale = localScale;
        
                yield return null;
            }
        }
    }
}