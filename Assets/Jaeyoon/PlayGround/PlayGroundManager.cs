using Play;
using UnityEngine;
using System;

public class PlayGroundManager : MonoBehaviour, IGamePlayable
{

    public bool IsPlay { get; set; } = false;


    //public RectTransform[] range;
    public GameObject Circle;
    //public float radius;
    //public int segments;
    //public float speed;
    //public float width;

    public Transform throwStart;
    public Transform throwDest;
    public GameObject canPrefab;
    public CanState can;
    public ThrowCan[] throwCans;
    public Vector3 camPlace;
    public Vector3 camDirection;
    private Vector3 BeforeCamDis;
    private Vector3 BeforeCamRot;

    [System.Serializable]
    public struct ThrowCan
    {
        public float scalar;
        [Range(0, 1)]
        public float plusY;
        public CanState cans;
    }

    public enum CanState
    {
        per, good, notgood, bad
    }

    private void Start()
    {
        JoystickController.instance.InitializeJoyStick(false);
        Circle.SetActive(false);
        Circle.transform.GetComponentInChildren<CreateRing>().enabled = false;
    }
    public void Play()
    {
        BeforeCamDis = DataController.instance.camDis;
        BeforeCamRot = DataController.instance.camRot;

        DataController.instance.camDis = camPlace;
        DataController.instance.camRot = camDirection;
        StartTimingGame();
        IsPlay = true;
    }

    public void EndPlay()
    {
        DataController.instance.camDis = BeforeCamDis;
        DataController.instance.camRot = BeforeCamRot;
        IsPlay = false;
    }

    //원 생성
    public void StartTimingGame()
    {
        DataController.instance.currentMap.ui.gameObject.SetActive(true);
        //게임 실행
        CanvasControl.instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        //CanvasControl.instance_CanvasControl.GetComponent<Canvas>().worldCamera = DataController.instance.cam;
        CanvasControl.instance.GetComponent<Canvas>().worldCamera = Camera.main;
        CanvasControl.instance.GetComponent<Canvas>().planeDistance = 0.8f;

        //원 이미지 보이게
        Circle.SetActive(true);

        //움직이는 원 생성
        Circle.transform.GetComponentInChildren<CreateRing>().enabled = true;
        Circle.transform.GetComponentInChildren<LineRenderer>().alignment = LineAlignment.TransformZ;
        Circle.transform.GetComponentInChildren<CreateRing>().Setup();
        Circle.transform.GetComponentInChildren<CreateRing>().CreatePoints();


        //ring.Setup(radius *range[2].localScale.x / 2, CanvasControl.instance_CanvasControl.GetComponent<Canvas>().planeDistance * 0.03f * width, speed, segments);

        //DataController.instance.currentMap.ui.SetActive(true);
    }
    //게임 끝날 경우 함수
    public void EndTimingGame()
    {
        Circle.SetActive(false);
        CanvasControl.instance.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        DataController.instance.currentMap.ui.gameObject.SetActive(false);
        StartThrowing(can);
    }

    public void StartThrowing(CanState type)
    {
        //canState에 따라 던지기

        //게임오브젝트 생성
        GameObject myCan = Instantiate(canPrefab);
        //시작점에 위치
        myCan.transform.position = throwStart.position;
        //도착점 방향으로 던지기 //스칼라와 y값 설정
        Vector3 canDir = (throwDest.position - throwStart.position).normalized;
        canDir.y = throwCans[0].plusY;

        int index = Array.FindIndex<ThrowCan>(throwCans, (item) => { return item.cans == type; });

        //throwCans[0] 
        //can = throwCans[0].cans;
        //can = throwCans[1].cans;
        //can = throwCans[2].cans;
        //can = throwCans[3].cans;
        myCan.GetComponent<CapsuleCollider>().isTrigger = false;

        //AddForce, AddTorque
        myCan.GetComponent<Rigidbody>().AddForce(canDir.normalized * throwCans[index].scalar, ForceMode.Impulse);
        myCan.GetComponent<Rigidbody>().AddTorque(canDir.normalized * throwCans[index].scalar, ForceMode.Impulse);
        //던진후 움직임이 멈춘후 endthrowing 실행 timing 몇초후/ 바닥에 닿은 후
        Invoke("EndThrowing", 1f);
    }
   
    public void EndThrowing()
    {
        EndPlay();
    }
    //원 판정함수
    public void ClearCheck()
    {
        CreateRing circleParent = Circle.transform.GetComponentInChildren<CreateRing>();
        if (circleParent.radius * 2 < Circle.transform.GetChild(2).localScale.x)
        {
            Debug.Log("유후! 바로 들어감");
            can = CanState.per;
        }
        else if (circleParent.radius * 2 < Circle.transform.GetChild(0).localScale.x)
        {
            Debug.Log("아싸! 한번 튕기고 들어감");
            can = CanState.good;
        }
        else if (circleParent.radius * 2 < Circle.transform.GetChild(1).localScale.x)
        {
            Debug.Log("아깝다! 한번 튕기고 안들어감");
            can = CanState.notgood;
        }
        else
        {
            Debug.Log("앗! 안들어감");
            can = CanState.bad;
        }
        EndTimingGame();
        //실행 멈추기
        //카메라 돌려놓기
        //if(ring.transform.localScale.x = range[0].)
    }

    //원 사라짐
    public void Update()
    {

        if (Circle.activeSelf && Circle.transform.GetChild(2).localScale.x * 0.3 > Circle.transform.GetComponentInChildren<CreateRing>().radius * 2)
        {
            Debug.Log(Circle.transform.GetChild(2).localScale.x);
            Debug.Log(Circle.transform.GetComponentInChildren<CreateRing>().radius);
            can = CanState.bad; EndTimingGame();
        }
    }
}
