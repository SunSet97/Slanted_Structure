using Play;
using UnityEngine;
using System;
using System.Collections;
using Data;
using UnityEngine.UI;
using Utility.Core;
using Utility.Preference;

public class PlayGroundManager : MonoBehaviour, IGamePlayable
{
    public bool IsPlay { get; set; }
    public Action ONEndPlay { get; set; }

    public GameObject canPrefab;

    public GameObject rangeParent;
    public Button clearButton;

    public Transform startPoint;
    public Transform destPoint;

    public ThrowCan[] throwCans;

    public CamInfo playCam;

    [SerializeField] private CreateRing ring;

    [Serializable]
    public struct ThrowCan
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

    private void Start()
    {
        clearButton.onClick.AddListener(ClearCheck);
    }

    public void Play()
    {
        JoystickController.Instance.InitializeJoyStick(false);

        DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
        rangeParent.SetActive(false);
        ring.enabled = false;

        // DataController.instance.camDis = playCam.camDis;
        // DataController.instance.camRot = playCam.camRot;
        // StartTimingGame();
        StartCoroutine(StartTestThrow());
        IsPlay = true;
    }

    IEnumerator StartTestThrow()
    {
        while (true)
        {
            StartThrowing(ResultState.Bad);
            yield return new WaitForSeconds(2);
        }
    }

    public void EndPlay()
    {
        ONEndPlay?.Invoke();
        
        DataController.Instance.camInfo.camDis = DataController.Instance.CurrentMap.camDis;
        DataController.Instance.camInfo.camRot = DataController.Instance.CurrentMap.camRot;
        IsPlay = false;
    }

    private void StartTimingGame()
    {
        DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);

        Canvas canvas = PlayUIController.Instance.Canvas;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = DataController.Instance.Cam;
        canvas.planeDistance = 0.8f;

        rangeParent.SetActive(true);

        ring.Setup();
        ring.CreatePoints();

        StartCoroutine(PlayTimingGame());
    }

    private void EndTimingGame(ResultState result)
    {
        rangeParent.SetActive(false);
        ring.Remove();
        PlayUIController.Instance.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
        StartThrowing(result);
    }

    private void StartThrowing(ResultState type)
    {
        GameObject can = Instantiate(canPrefab);
        can.GetComponent<Can>().onCollisionEnter.AddListener(() => { Invoke(nameof(EndPlay), 1f); });
        can.transform.position = startPoint.position;

        int index = Array.FindIndex(throwCans, item => item.resultType == type);
        
        Vector3 canDir = (destPoint.position - startPoint.position).normalized;
        canDir.y = throwCans[index].plusY;

        can.GetComponent<Collider>().isTrigger = false;

        Rigidbody throwRigid = can.GetComponent<Rigidbody>();
        throwRigid.AddForce(canDir * throwCans[index].power, ForceMode.Impulse);
        throwRigid.AddTorque(canDir * throwCans[index].power, ForceMode.Impulse);
    }

    private void ClearCheck()
    {
        if (!IsPlay)
        {
            return;
        }

        ResultState result;
        if (ring.radius * 2 <= rangeParent.transform.GetChild(2).localScale.x *
            rangeParent.transform.GetChild(2).GetComponent<RectTransform>().rect.width)
        {
            Debug.Log("유후! 바로 들어감");
            result = ResultState.Perfect;
        }
        else if (ring.radius * 2 <= rangeParent.transform.GetChild(1).localScale.x *
            rangeParent.transform.GetChild(1).GetComponent<RectTransform>().rect.width)
        {
            Debug.Log("아싸! 한번 튕기고 들어감");
            result = ResultState.Good;
        }
        else if (ring.radius * 2 <= rangeParent.transform.GetChild(0).localScale.x *
            rangeParent.transform.GetChild(0).GetComponent<RectTransform>().rect.width)
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

    private IEnumerator PlayTimingGame()
    {
        while (rangeParent.transform.GetChild(2).localScale.x * 0.3 *
            rangeParent.transform.GetChild(2).GetComponent<RectTransform>().rect.width <= ring.radius * 2)
        {
            ring.DisplayUpdate();
            yield return null;
        }

        EndTimingGame(ResultState.Bad);
    }
}