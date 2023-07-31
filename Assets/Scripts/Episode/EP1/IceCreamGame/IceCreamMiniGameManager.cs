using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Random = UnityEngine.Random;

namespace Episode.EP1.IceCreamGame
{
    public class IceCreamMiniGameManager : MiniGame
    {
        private enum MoveDir
        {
            Left = 0,
            Right = 1
        }

        [SerializeField] private GameObject iceCreamPanel;
    
        [SerializeField] private IceCreamClicker iceCreamClicker;

        [SerializeField] private Transform iceCream;

        [SerializeField] private Transform[] wayPoint;

        [SerializeField] private Transform destTransform;

        public Button clickMissCheckButton;

        public Image sellerImage;
        public Sprite[] sellerSprites;

        [SerializeField] [Range(10, 20)] private float horizontalSpeed = 11f;

        [SerializeField] [Range(5, 10)] private float verticalSpeed = 11f;

        private MoveDir moveDir;
        private int successCount;
        private Vector3 originPos;

        public override void Play()
        {
            base.Play();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            iceCreamPanel.SetActive(true);

            moveDir = (MoveDir) Random.Range(0, 2);
            iceCreamClicker.enabled = false;
            clickMissCheckButton.enabled = false;
            iceCreamClicker.onPointerEnter = () =>
            {
                StopAllCoroutines();
                iceCreamClicker.enabled = false;
                clickMissCheckButton.enabled = false;
                successCount++;
            
                StartCoroutine(ChangeSellerEmotion(1));
            
                if (successCount == 3)
                {
                    StartCoroutine(Reset());
                }
                else
                {
                    StartCoroutine(MoveToOrigin());
                }
            };
            clickMissCheckButton.onClick.AddListener(() =>
            {
                StopAllCoroutines();
                iceCreamClicker.enabled = false;
                clickMissCheckButton.enabled = false;

                StartCoroutine(ChangeSellerEmotion(3));

                StartCoroutine(MoveToOrigin());
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

        //private IEnumerator MoveHorizontal()
        //{
        //    var sec = 0f;
        //    var waitForFixedUpdate = new WaitForFixedUpdate();

        //    Vector3 targetPos = Vector3.zero;
        //    if (moveDir == MoveDir.Left)
        //    {
        //        targetPos = wayPoint[0].position;
        //    }
        //    else if (moveDir == MoveDir.Right)
        //    {
        //        targetPos = wayPoint[1].position;
        //    }

        //    while (sec <= 5f)
        //    {
        //        var moveTowards = Vector3.MoveTowards(iceCream.position, targetPos, 100 * horizontalSpeed * Time.fixedDeltaTime);
        //        iceCream.position = Vector3.Lerp(iceCream.position, moveTowards, 1f);

        //        if (Mathf.Approximately(Vector3.Distance(iceCream.position, targetPos), 0))
        //        {
        //            if (moveDir == MoveDir.Left)
        //            {
        //                moveDir = MoveDir.Right;
        //                targetPos = wayPoint[1].position;
        //            }
        //            else if (moveDir == MoveDir.Right)
        //            {
        //                moveDir = MoveDir.Left;
        //                targetPos = wayPoint[0].position;
        //            }
        //        }

        //        sec += Time.fixedDeltaTime;
        //        yield return waitForFixedUpdate;
        //    }

        //    StartCoroutine(MoveVertical());
        //}

        private IEnumerator StartPattern()
        {
            Debug.Log("시작");
            int count = Random.Range(4, 7);
            Debug.Log(count);
            int position_counting = 0;
            var waitForFixedUpdate = new WaitForFixedUpdate();

            while (position_counting != count)
            {
                float tarPosX;
                while (true)
                {
                    tarPosX = Random.Range(wayPoint[0].position.x, wayPoint[1].position.x);
                    if (Mathf.Abs(tarPosX - iceCream.position.x) > 500f)
                    {
                        break;
                    }
                }


                while (Mathf.Abs(iceCream.position.x - tarPosX) > 0.01f)
                {
                    Vector2 moveTowards = Vector2.MoveTowards(iceCream.position, new Vector2(tarPosX, iceCream.position.y), 100 * horizontalSpeed * Time.fixedDeltaTime);
                    iceCream.position = moveTowards;

                    yield return waitForFixedUpdate;
                }
            
                position_counting++;

            }

            StartCoroutine(MoveVertical());
        }

        private IEnumerator MoveVertical()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            originPos = iceCream.position;
            iceCreamClicker.enabled = true;
            clickMissCheckButton.enabled = true;

            float sec = 0f;

            var downSec = 1f;

            SetSellerEmotion(2);
            while (sec <= downSec / verticalSpeed)
            {
                sec += Time.fixedDeltaTime;
                Vector3 switchPosition = originPos;
                switchPosition.y = Mathf.Lerp(originPos.y, destTransform.position.y, sec / (downSec / verticalSpeed));
                iceCream.position = switchPosition;
                yield return waitForFixedUpdate;
            }

            yield return new WaitForSeconds(Random.Range(1f, 1.5f));

            sec = 0;
            var upSec = 3f;
            SetSellerEmotion(0);
            while (sec <= upSec / verticalSpeed)
            {
                sec += Time.fixedDeltaTime;
                Vector3 switchPosition = originPos;
                switchPosition.y = Mathf.Lerp(destTransform.position.y, originPos.y, sec / (upSec / verticalSpeed));
                iceCream.position = switchPosition;
                yield return waitForFixedUpdate;
            }

            iceCreamClicker.enabled = false;
            clickMissCheckButton.enabled = false;
            yield return new WaitForSeconds(.1f);
            StartCoroutine(StartPattern());
        }

        private IEnumerator MoveToOrigin()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            Vector3 moveTowards;
            do
            {
                moveTowards = Vector3.Lerp(iceCream.position, originPos, 0.1f * Random.Range(1, 50));
                iceCream.position = moveTowards;
                Debug.Log(Vector3.Distance(originPos, iceCream.position));
                yield return waitForFixedUpdate;
            } while (Vector3.Distance(originPos, iceCream.position) >= .1f);

            iceCream.position = originPos;

            yield return new WaitForSeconds(1f);

            StartCoroutine(StartPattern());
        }
    
        private IEnumerator Reset()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            Vector3 moveTowards;
            do
            {
                moveTowards = Vector3.Lerp(iceCream.position, originPos, 0.1f * Random.Range(1, 50));
                iceCream.position = moveTowards;
                Debug.Log(Vector3.Distance(originPos, iceCream.position));
                yield return waitForFixedUpdate;
            } while (Vector3.Distance(originPos, iceCream.position) >= .1f);

            iceCream.position = originPos;

            yield return new WaitForSeconds(1f);
            EndPlay(true);
        }

        private IEnumerator ChangeSellerEmotion(int emotion)
        {
            yield return new WaitForSeconds(.1f);
            sellerImage.sprite = sellerSprites[emotion];
            yield return new WaitForSeconds(.7f);
            sellerImage.sprite = sellerSprites[0];
        }

        private void SetSellerEmotion(int emotion)
        {
            sellerImage.sprite = sellerSprites[emotion];
        }
    }
}