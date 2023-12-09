using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Game;
using Random = UnityEngine.Random;

namespace Episode.EP1.IceCreamGame
{
    public class IceCreamMiniGameManager : MiniGame
    {
        private enum SellerEmotion
        {
            Default = 0,
            Joyful = 1,
            Smile = 2,
            Surprise = 3,
        }

#pragma warning disable 0649
        [SerializeField] private GameObject iceCreamPanel;

        [Header("Seller")] [SerializeField] private Image sellerImage;
        [SerializeField] private Sprite[] sellerSprites;

        [Header("Ice Cream")] [SerializeField] private IceCreamClicker iceCreamClicker;
        [SerializeField] private Transform iceCream;
        [SerializeField] private Transform[] wayPoint;
        [SerializeField] private Transform destTransform;
        [SerializeField] private Button clickMissCheckButton;

        [Header("Ice Cream - Parameter Value")] [SerializeField] [Range(10, 20)]
        private float horizontalSpeed = 11f;

        [SerializeField] [Range(5, 10)] private float verticalSpeed = 10f;
        [SerializeField] private int clearCount;

        [Header("Timer")] [SerializeField] private Text timerText;
        [SerializeField] private float timerSec;
#pragma warning restore 0649

        private int successCount;
        private Vector3 originPos;
        private Coroutine moveVerticalCoroutine;

        private bool IsClickEnable
        {
            set
            {
                iceCreamClicker.enabled = value;
                clickMissCheckButton.enabled = value;
            }
        }

        public override void Play()
        {
            base.Play();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            iceCreamPanel.SetActive(true);

            IsClickEnable = false;
            iceCreamClicker.Init(() =>
            {
                IsClickEnable = false;
                successCount++;

                if (successCount == clearCount)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveToOrigin(SellerEmotion.Surprise, 1f, () => { EndPlay(true); }));
                }
                else
                {
                    StopCoroutine(moveVerticalCoroutine);
                    StartCoroutine(MoveToOrigin(SellerEmotion.Surprise, 1f, () => { StartCoroutine(StartPattern()); }));
                }
            });

            clickMissCheckButton.onClick.AddListener(() =>
            {
                StopCoroutine(moveVerticalCoroutine);
                IsClickEnable = false;

                StartCoroutine(MoveToOrigin(SellerEmotion.Smile, 1f, () => { StartCoroutine(StartPattern()); }));
            });

            var iceCreamPos = iceCream.position;
            iceCreamPos.y = wayPoint[0].position.y;
            iceCream.position = iceCreamPos;

            StartCoroutine(StartPattern());

            StartCoroutine(StartTimer());
        }

        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            iceCreamPanel.SetActive(false);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
        }

        private IEnumerator StartPattern()
        {
            var moveCount = Random.Range(4, 7);
            var horizontalMoveCounting = 0;
            while (horizontalMoveCounting < moveCount)
            {
                float tarPosX;
                do
                {
                    tarPosX = Random.Range(wayPoint[0].position.x, wayPoint[1].position.x);
                } while (Mathf.Abs(tarPosX - iceCream.position.x) < 500f);


                while (Mathf.Abs(iceCream.position.x - tarPosX) > 0.01f)
                {
                    iceCream.position = Vector3.MoveTowards(iceCream.position,
                        new Vector3(tarPosX, iceCream.position.y, iceCream.position.z),
                        100 * horizontalSpeed * Time.deltaTime);

                    yield return null;
                }

                horizontalMoveCounting++;
            }

            moveVerticalCoroutine = StartCoroutine(MoveVertical());
        }

        private IEnumerator MoveVertical()
        {
            originPos = iceCream.position;
            IsClickEnable = true;

            SetSellerEmotion(SellerEmotion.Joyful);

            var sec = 0f;
            const float downSec = 1f;
            while (sec <= downSec)
            {
                sec += Time.deltaTime * verticalSpeed;
                var t = sec / downSec;
                var switchPosition = originPos;
                switchPosition.y = Mathf.Lerp(originPos.y, destTransform.position.y, t);
                iceCream.position = switchPosition;
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(1f, 1.5f));

            StartCoroutine(MoveToOrigin(SellerEmotion.Smile, 1f, () => { StartCoroutine(StartPattern()); }));

            // sec = 0f;
            // const float upSec = 3f;
            // while (sec <= upSec)
            // {
            //     sec += Time.deltaTime * verticalSpeed;
            //     var t = sec / upSec;
            //     var switchPosition = originPos;
            //     switchPosition.y = Mathf.Lerp(destTransform.position.y, originPos.y, t);
            //     iceCream.position = switchPosition;
            //     yield return null;
            // }
            //
            // IsClickEnable = false;
            // SetSellerEmotion(SellerEmotion.Default);
            // yield return new WaitForSeconds(1f);
            // StartCoroutine(StartPattern());
        }

        private IEnumerator MoveToOrigin(SellerEmotion emotion, float waitSec, Action onEndAction)
        {
            Debug.Log("Move to Origin");
            SetSellerEmotion(emotion);
            do
            {
                iceCream.position = Vector3.MoveTowards(iceCream.position,
                    new Vector3(iceCream.position.x, originPos.y, iceCream.position.z),
                    100 * verticalSpeed * Time.deltaTime);
                yield return null;
            } while (Mathf.Abs(originPos.y - iceCream.position.y) >= .1f);

            SetSellerEmotion(SellerEmotion.Default);
            iceCream.position = originPos;

            yield return new WaitForSeconds(waitSec);

            onEndAction?.Invoke();
        }

        private IEnumerator StartTimer()
        {
            var t = timerSec;
            while (t >= 0)
            {
                timerText.text = $"{t:0}";
                t -= Time.deltaTime;
                yield return null;
            }

            Debug.Log($"End Timer");

            StopAllCoroutines();
            IsClickEnable = false;

            StartCoroutine(MoveToOrigin(SellerEmotion.Smile, 1f, () => { EndPlay(false); }));
        }


        private void SetSellerEmotion(SellerEmotion emotion)
        {
            sellerImage.sprite = sellerSprites[(int)emotion];
        }
    }
}