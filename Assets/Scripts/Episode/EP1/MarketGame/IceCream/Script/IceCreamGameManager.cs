using System;
using System.Collections;
using Play;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Random = UnityEngine.Random;

public class IceCreamGameManager : MonoBehaviour, IGamePlayable
{
    private enum MoveDir
    {
        Left = 0,
        Right = 1
    }

    public bool IsPlay { get; set; }
    public Action ONEndPlay { get; set; }

    [SerializeField] private GameObject iceCreamPanel;
    
    [SerializeField] private IceCreamClicker iceCreamClicker;

    [SerializeField] private Transform iceCream;

    [SerializeField] private Transform[] wayPoint;

    [SerializeField] private Transform destTransform;

    public Image sellerImage;
    public Sprite[] sellerSprites;

    [SerializeField] [Range(5, 15)] private float horizontalSpeed = 6f;

    [SerializeField] [Range(5, 15)] private float verticalSpeed = 6f;

    private MoveDir moveDir;
    private int successCount;
    private Vector3 originPos;

    public void Play()
    {
        JoystickController.Instance.StopSaveLoadJoyStick(true);
        iceCreamPanel.SetActive(true);
        IsPlay = true;

        moveDir = (MoveDir) Random.Range(0, 2);
        iceCreamClicker.enabled = false;
        iceCreamClicker.onPointerEnter = () =>
        {
            StopAllCoroutines();
            iceCreamClicker.enabled = false;
            successCount++;
            
            StartCoroutine(ChangeSellerEmotion(2));
            
            if (successCount == 3)
            {
                StartCoroutine(Reset());
            }
            else
            {
                StartCoroutine(MoveToOrigin());
            }
        };

        // failCheckButton.onClick.AddListener(() =>
        // {
        //     int randomFace = Random.Range(0, 2);
        //     int facenum = randomFace * 2 + 1;
        //     StartCoroutine(ChangeSellerEmotion(facenum));
        // });

        var iceCreamPos = iceCream.position;
        iceCreamPos.y = wayPoint[0].position.y;
        iceCream.position = iceCreamPos;
        
        StartCoroutine(MoveHorizontal());
    }

    public void EndPlay()
    {
        IsPlay = false;
        ONEndPlay?.Invoke();
        iceCreamPanel.SetActive(false);
        JoystickController.Instance.StopSaveLoadJoyStick(false);
    }

    private IEnumerator MoveHorizontal()
    {
        var sec = 0f;
        var waitForFixedUpdate = new WaitForFixedUpdate();
        
        Vector3 targetPos = Vector3.zero;
        if (moveDir == MoveDir.Left)
        {
            targetPos = wayPoint[0].position;
        }
        else if (moveDir == MoveDir.Right)
        {
            targetPos = wayPoint[1].position;
        }
        
        while (sec <= 5f)
        {
            var moveTowards = Vector3.MoveTowards(iceCream.position, targetPos, 100 * horizontalSpeed * Time.fixedDeltaTime);
            iceCream.position = Vector3.Lerp(iceCream.position, moveTowards, 1f);

            if (Mathf.Approximately(Vector3.Distance(iceCream.position, targetPos), 0))
            {
                if (moveDir == MoveDir.Left)
                {
                    moveDir = MoveDir.Right;
                    targetPos = wayPoint[1].position;
                }
                else if (moveDir == MoveDir.Right)
                {
                    moveDir = MoveDir.Left;
                    targetPos = wayPoint[0].position;
                }
            }

            sec += Time.fixedDeltaTime;
            yield return waitForFixedUpdate;
        }

        StartCoroutine(MoveVertical());
    }

    private IEnumerator MoveVertical()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        originPos = iceCream.position;
        iceCreamClicker.enabled = true;

        float sec = 0f;

        var downSec = 3f;

        while (sec <= downSec / verticalSpeed)
        {
            sec += Time.fixedDeltaTime;
            Vector3 switchPosition = originPos;
            switchPosition.y = Mathf.Lerp(originPos.y, destTransform.position.y, sec / (downSec / verticalSpeed));
            iceCream.position = switchPosition;
            yield return waitForFixedUpdate;
        }

        yield return new WaitForSeconds(.1f);

        sec = 0;
        var upSec = 3f;
        while (sec <= upSec / verticalSpeed)
        {
            sec += Time.fixedDeltaTime;
            Vector3 switchPosition = originPos;
            switchPosition.y = Mathf.Lerp(destTransform.position.y, originPos.y, sec / (upSec / verticalSpeed));
            iceCream.position = switchPosition;
            yield return waitForFixedUpdate;
        }

        iceCreamClicker.enabled = false;
        originPos = Vector3.zero;
        yield return new WaitForSeconds(.1f);
        StartCoroutine(MoveHorizontal());
    }

    private IEnumerator MoveToOrigin()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        var sec = 0f;
        while (sec <= .5f)
        {
            sec += Time.fixedDeltaTime;
            var moveTowards = Vector3.MoveTowards(iceCream.position, originPos, 300 * Time.fixedDeltaTime);
            iceCream.position = moveTowards;
            
            yield return waitForFixedUpdate;
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine(MoveHorizontal());
    }
    
    private IEnumerator Reset()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        var sec = 0f;
        while (sec <= 1f)
        {
            sec += Time.fixedDeltaTime;
            var moveTowards = Vector3.MoveTowards(iceCream.position, originPos, 300 * Time.fixedDeltaTime);
            iceCream.position = moveTowards;
            
            yield return waitForFixedUpdate;
        }

        yield return new WaitForSeconds(1f);
        EndPlay();
    }

    private IEnumerator ChangeSellerEmotion(int emotion)
    {
        yield return new WaitForSeconds(.1f);
        sellerImage.sprite = sellerSprites[emotion];
        yield return new WaitForSeconds(.7f);
        sellerImage.sprite = sellerSprites[0];
    }
}