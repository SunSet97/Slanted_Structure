using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Core;
using Utility.Game;

namespace Episode.EP2.CoinTossGame
{
    public class CoinTossMiniGameManager : MiniGame
    {
        private enum ChargeStepType
        {
            None, Direction, Power, Hold
        }
        
#pragma warning disable 0649
        [SerializeField] private GameObject coinPrefab;

        [SerializeField] private Transform throwPos;

        [SerializeField] private GameObject directionPanel;
        [SerializeField] private Transform directionArrow;
        [SerializeField] private GameObject powerPanel;
        [SerializeField] private Transform powerArrow;
        [SerializeField] private GameObject remainCoinPanel;
        [SerializeField] private Text remainCoinText;
        [SerializeField] private Button gameStartButton;

        [Header("-Toss")] [SerializeField] private EventTrigger tossButtonTrigger;

        [SerializeField] private float angleRange = 70f;
        [SerializeField] private float powerRange = 230f;

        [SerializeField] private float rotateSpeed = 1.5f;
        [SerializeField] private float powerSpeed = 3f;

        [SerializeField] private float maxPower = .5f;
        [SerializeField] private float minPower = .3f;

        [SerializeField] private float throwAngleRange = 45f;
#pragma warning restore 0649
        
        private int tryNum;
        private int TryNum
        {
            get => tryNum;
            set
            {
                tryNum = value;
                remainCoinText.text = $"X{MaximumTryCount - tryNum}";
            }
        }
        private const int MaximumTryCount = 5;
        private ChargeStepType chargeStep;

        private ChargeStepType ChargeStep
        {
            get => chargeStep;
            set
            {
                switch (value)
                {
                    case ChargeStepType.Direction:
                        directionPanel.SetActive(true);
                        powerPanel.SetActive(true);
                        tossButtonTrigger.gameObject.SetActive(true);
                        // directionPanel.SetEnable(true);
                        // powerPanel.SetEnable(false);
                        break;
                    case ChargeStepType.Power:
                        directionPanel.SetActive(true);
                        powerPanel.SetActive(true);
                        tossButtonTrigger.gameObject.SetActive(true);
                        // directionPanel.고정
                        // powerPanel.SetEnable(true);
                        break;
                    case ChargeStepType.None:
                        directionPanel.SetActive(false);
                        powerPanel.SetActive(false);
                        tossButtonTrigger.gameObject.SetActive(false);
                        break;
                    case ChargeStepType.Hold:
                        tossButtonTrigger.gameObject.SetActive(false);
                        break;
                }

                chargeStep = value;
            }
        }

        private Vector3 initialThrowEulerAngle;
        private Vector3 initialDirectionArrowEulerAngles;
        private Vector3 initialPowerArrowPos;

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
            TryNum = 0;
            initialThrowEulerAngle = throwPos.eulerAngles;
            initialDirectionArrowEulerAngles = directionArrow.eulerAngles;
            initialPowerArrowPos = powerArrow.localPosition;
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            gameStartButton.gameObject.SetActive(true);
            remainCoinPanel.SetActive(false);
            ChargeStep = ChargeStepType.None;
            ResetUI();
        }

        // 5번 넘게 실패할 경우 EndPlay
        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            ChargeStep = ChargeStepType.None;
            remainCoinPanel.SetActive(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
        }

        private void ResetUI(Action onEndAction = null)
        {
            StartCoroutine(ResetCoroutine(onEndAction));
        }

        private IEnumerator ResetCoroutine(Action onEndAction)
        {
            while (Vector3.Distance(directionArrow.eulerAngles, initialDirectionArrowEulerAngles) > Time.deltaTime || Vector3.Distance(powerArrow.transform.localPosition, initialPowerArrowPos) > Time.deltaTime)
            {
                directionArrow.eulerAngles = Vector3.Lerp(directionArrow.eulerAngles, initialDirectionArrowEulerAngles, .05f);
                powerArrow.transform.localPosition =
                    Vector3.Lerp(powerArrow.transform.localPosition, initialPowerArrowPos, .05f);
                yield return null;
                // Debug.Log($"리셋 중  {Vector3.Distance(directionArrow.eulerAngles, initialDirectionArrowEulerAngles)}, {Time.deltaTime}  -> {Vector3.Distance(directionArrow.eulerAngles, initialDirectionArrowEulerAngles) < Time.deltaTime} || " +
                          // $"{Vector3.Distance(powerArrow.transform.localPosition, initialPowerArrowPos)}, {Time.fixedDeltaTime}  -> {Vector3.Distance(powerArrow.transform.localPosition, initialPowerArrowPos) < Time.deltaTime}");
            }
            
            // Debug.Log("끝 - 리셋");

            onEndAction?.Invoke();
        }

        private void StartButton()
        {
            gameStartButton.gameObject.SetActive(false);
            remainCoinPanel.SetActive(true);
            ChargeStep = ChargeStepType.Direction;
        }

        private void OnPointerDown(BaseEventData _)
        {
            if (ChargeStep == ChargeStepType.Direction)
            {
                StartCoroutine(ChargeDirection());
            }
            else if (ChargeStep == ChargeStepType.Power)
            {
                StartCoroutine(ChargePower());
            }
        }

        private void OnPointerUp(BaseEventData _)
        {
            StopAllCoroutines();

            if (ChargeStep == ChargeStepType.Direction)
            {
                ChargeStep = ChargeStepType.Power;
            }
            else if (ChargeStep == ChargeStepType.Power)
            {
                TryNum++;
                ChargeStep = ChargeStepType.Hold;
                
                var angle = directionArrow.localEulerAngles.z;
                
                // 180 ~ 0, 360 ~ 180 -> 180 ~ 0, 0 ~ -180
                if (directionArrow.localEulerAngles.z > 180)
                {
                    angle -= 360;
                }

                angle = -angle / angleRange * throwAngleRange;

                var throwEulerAngle = initialThrowEulerAngle;
                throwEulerAngle.x += angle;
                throwPos.eulerAngles = throwEulerAngle;

                // - powerRange ~ powerRange -> -1 ~ 1 -> 0 ~ 1 -> minPower ~ maxPower
                var power = Mathf.Abs(powerArrow.transform.localPosition.y / powerRange);
                var tossPow = Mathf.Lerp(minPower, maxPower, power);
                
                // Debug.Log($"ratio -> angle: {unasdf}, pow: {tossPow}");
                Debug.Log($"angle: {angle}, pow: {tossPow}");
                
                // 던지는 위치 변경
                var tossedCoin = Instantiate(coinPrefab, throwPos.position, Quaternion.identity)
                    .GetComponent<Coin>();
                tossedCoin.Init(throwPos.forward, tossPow, 5f, isClear =>
                {
                    Debug.Log($"성공여부: {isClear}");
                    if (isClear)
                    {
                        // 성공
                        EndPlay(true);
                    }
                    else
                    {
                        // 실패
                        if (TryNum >= MaximumTryCount)
                        {
                            Debug.Log("끝남");
                            EndPlay(false);
                        }
                        else
                        {
                            ResetUI(() =>
                            {
                                ChargeStep = ChargeStepType.Direction;
                            });
                        }
                    }
                });
            }
        }

        private IEnumerator ChargeDirection()
        {
            var t = 90f * Mathf.Deg2Rad;
            while (true)
            {
                t += Time.deltaTime * rotateSpeed;
                var localEulerAngles = directionArrow.localEulerAngles;
                localEulerAngles.z = Mathf.Sin(t) * angleRange;
                directionArrow.localEulerAngles = localEulerAngles;
                
                yield return null;
            }
        }

        private IEnumerator ChargePower()
        {
            var t = 90f * Mathf.Deg2Rad;
            while (true)
            {
                t += Time.deltaTime * powerSpeed;
                var localPosition = powerArrow.transform.localPosition;
                localPosition.y = Mathf.Sin(t) * powerRange;
                powerArrow.localPosition = localPosition;

                yield return null;
            }
        }
    }
}