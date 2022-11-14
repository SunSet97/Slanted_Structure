using Play;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlayGroundManager : MonoBehaviour, IGamePlayable
{
    public bool IsPlay { get; set; }

    public GameObject canPrefab;
    
    public GameObject circleParent;
    public Button clearButton;
    
    public Transform startPoint;
    public Transform destPoint;
    
    public ThrowCan[] throwCans;
    
    public CamInfo playCam;

    private CreateRing ring;

    [Serializable]
    public struct ThrowCan
    {
        public float power;
        [Range(0, 1)]
        public float plusY;
        public ResultState resultType;
    }

    public enum ResultState
    {
        Perfect, Good, NotBad, Bad
    }

    private void Start()
    {
        clearButton.onClick.AddListener(ClearCheck);
    }

    public void Play()
    {
        JoystickController.instance.InitializeJoyStick(false);
        circleParent.SetActive(false);
        ring = circleParent.transform.GetComponentInChildren<CreateRing>();
        ring.enabled = false;

        DataController.instance.camDis = playCam.camDis;
        DataController.instance.camRot = playCam.camRot;
        StartTimingGame();
        IsPlay = true;
    }

    public void EndPlay()
    {
        DataController.instance.camDis = DataController.instance.currentMap.camDis;
        DataController.instance.camRot = DataController.instance.currentMap.camRot;
        IsPlay = false;
    }

    //원 생성
    private void StartTimingGame()
    {
        DataController.instance.currentMap.ui.gameObject.SetActive(true);

        Canvas canvas = CanvasControl.instance.GetComponent<Canvas>();
        
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = DataController.instance.cam;
        canvas.planeDistance = 0.8f;

        circleParent.SetActive(true);

        ring.enabled = true;
        ring.Setup();
        ring.CreatePoints();
        circleParent.transform.GetComponentInChildren<LineRenderer>().alignment = LineAlignment.TransformZ;

        StartCoroutine(CheckDisappearRing());

        //ring.Setup(radius *range[2].localScale.x / 2, CanvasControl.instance_CanvasControl.GetComponent<Canvas>().planeDistance * 0.03f * width, speed, segments);
    }
    //게임 끝날 경우 함수
    private void EndTimingGame(ResultState result)
    {
        circleParent.SetActive(false);
        CanvasControl.instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        DataController.instance.currentMap.ui.gameObject.SetActive(false);
        StartThrowing(result);
    }

    private void StartThrowing(ResultState type)
    {
        GameObject can = Instantiate(canPrefab);
        can.GetComponent<Can>().onCollisionEnter.AddListener(() =>
        {
            Invoke(nameof(EndPlay), 1f);
        });
        can.transform.position = startPoint.position;

        int index = Array.FindIndex(throwCans, item => item.resultType == type);

        Vector3 canDir = (destPoint.position - startPoint.position).normalized;
        canDir.y = throwCans[index].plusY;

        can.GetComponent<Collider>().isTrigger = false;

        can.GetComponent<Rigidbody>().AddForce(canDir * throwCans[index].power, ForceMode.Impulse);
        can.GetComponent<Rigidbody>().AddTorque(canDir * throwCans[index].power, ForceMode.Impulse);
    }
    

    //원 판정함수
    private void ClearCheck()
    {
        if (!IsPlay)
        {
            return;
        }

        ResultState result;
        if (ring.radius * 2 < circleParent.transform.GetChild(0).localScale.x)
        {
            Debug.Log("아싸! 한번 튕기고 들어감");
            result = ResultState.Good;
        }
        else if (ring.radius * 2 < circleParent.transform.GetChild(1).localScale.x)
        {
            Debug.Log("아깝다! 한번 튕기고 안들어감");
            result = ResultState.NotBad;
        }
        else if (ring.radius * 2 < circleParent.transform.GetChild(2).localScale.x)
        {
            Debug.Log("유후! 바로 들어감");
            result = ResultState.Perfect;
        }
        else
        {
            Debug.Log("앗! 안들어감");
            result = ResultState.Bad;
        }

        StopAllCoroutines();
        EndTimingGame(result);
    }

    private IEnumerator CheckDisappearRing()
    {
        yield return new WaitUntil(() => circleParent.transform.GetChild(2).localScale.x * 0.3 > ring.radius * 2);
        
        EndTimingGame(ResultState.Bad);
    }
}
