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

        [SerializeField] private GameObject iceCreamPanel;
        [SerializeField] private IceCreamClicker iceCreamClicker;
        [SerializeField] private Transform iceCream;
        [SerializeField] private Transform[] wayPoint;
        [SerializeField] private Transform destTransform;
        [SerializeField] private Button clickMissCheckButton;
        [SerializeField] private Image sellerImage;
        [SerializeField] private Sprite[] sellerSprites;

        [SerializeField] [Range(10, 20)] private float horizontalSpeed = 11f;
        [SerializeField] [Range(5, 10)] private float verticalSpeed = 11f;
        
        private int successCount;
        private Vector3 originPos;

        private bool IsEnable
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
            
            IsEnable = false;
            iceCreamClicker.Init(() =>
            {
                StopAllCoroutines();
                IsEnable = false;
                successCount++;

                StartCoroutine(successCount == 3
                    ? MoveToOrigin(SellerEmotion.Surprise, 1f, () => { EndPlay(true); })
                    : MoveToOrigin(SellerEmotion.Surprise, 1f, () => { StartCoroutine(StartPattern()); }));
            });

            clickMissCheckButton.onClick.AddListener(() =>
            {
                StopAllCoroutines();
                IsEnable = false;

                StartCoroutine(MoveToOrigin(SellerEmotion.Smile, 1f, () => { StartCoroutine(StartPattern()); }));
            });

            var iceCreamPos = iceCream.position;
            iceCreamPos.y = wayPoint[0].position.y;
            iceCream.position = iceCreamPos;

            StartCoroutine(StartPattern());
        }

        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            iceCreamPanel.SetActive(false);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
        }

        private IEnumerator StartPattern()
        {
            Debug.Log("시작");
            var moveCount = Random.Range(4, 7);
            var horizontalMoveCounting = 0;
            Debug.Log(moveCount);
            while (horizontalMoveCounting < moveCount)
            {
                float tarPosX;
                do
                {
                    tarPosX = Random.Range(wayPoint[0].position.x, wayPoint[1].position.x);
                } while (Mathf.Abs(tarPosX - iceCream.position.x) < 500f);


                while (Mathf.Abs(iceCream.position.x - tarPosX) > 0.01f)
                {
                    var position = iceCream.position;
                    Vector2 moveTowards = Vector2.MoveTowards(position,
                        new Vector2(tarPosX, position.y), 100 * horizontalSpeed * Time.deltaTime);
                    iceCream.position = moveTowards;

                    yield return null;
                }

                horizontalMoveCounting++;
            }

            StartCoroutine(MoveVertical());
        }

        private IEnumerator MoveVertical()
        {
            originPos = iceCream.position;
            IsEnable = true;

            SetSellerEmotion(SellerEmotion.Joyful);
            
            var sec = 0f;
            const float downSec = 1f;
            while (sec <= downSec / verticalSpeed)
            {
                sec += Time.deltaTime * verticalSpeed;
                Vector3 switchPosition = originPos;
                switchPosition.y = Mathf.Lerp(originPos.y, destTransform.position.y, sec / (downSec));
                iceCream.position = switchPosition;
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(1f, 1.5f));

            SetSellerEmotion(SellerEmotion.Smile);
            sec = 0f;
            const float upSec = 3f;
            while (sec <= upSec)
            {
                sec += Time.deltaTime * verticalSpeed;
                Vector3 switchPosition = originPos;
                switchPosition.y = Mathf.Lerp(destTransform.position.y, originPos.y, sec / (upSec));
                iceCream.position = switchPosition;
                yield return null;
            }

            IsEnable = false;
            yield return new WaitForSeconds(.5f);
            StartCoroutine(StartPattern());
        }

        private IEnumerator MoveToOrigin(SellerEmotion emotion, float waitSec, Action onEndAction)
        {
            SetSellerEmotion(emotion);
            var waitForFixedUpdate = new WaitForFixedUpdate();
            do
            {
                var moveTowards = Vector3.Lerp(iceCream.position, originPos, 0.02f * Random.Range(1, 10));
                iceCream.position = moveTowards;
                yield return waitForFixedUpdate;
            } while (Vector3.Distance(originPos, iceCream.position) >= .1f);

            SetSellerEmotion(SellerEmotion.Default);
            iceCream.position = originPos;

            yield return new WaitForSeconds(waitSec);

            onEndAction();
        }

        private void SetSellerEmotion(SellerEmotion emotion)
        {
            sellerImage.sprite = sellerSprites[(int)emotion];
        }
    }
}