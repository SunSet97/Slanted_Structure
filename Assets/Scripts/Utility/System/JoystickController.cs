using System;
using Data;
using UnityEngine;
using UnityEngine.UI;

public class JoystickController : MonoBehaviour
{
    [Header("조이스틱")] public Joystick dynamicJoystick;
    public Joystick fixedJoyStick;

    [SerializeField] private Button jumpButton;

    [NonSerialized] public Joystick joystick;
    [NonSerialized] public Vector2 inputDirection; // 조이스틱 입력 방향
    [NonSerialized] public float inputDegree; // 조이스틱 입력 크기
    [NonSerialized] public bool inputJump; // 조이스틱 입력 점프 판정
    [NonSerialized] public bool inputDash; // 조이스틱 입력 대쉬 판정
    [NonSerialized] public bool inputSeat;
    private bool isAlreadySave;
    private bool wasJoystickUse;


    private static JoystickController _instance;

    public static JoystickController instance => _instance;

    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(_instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        jumpButton.onClick.AddListener(() => { inputJump = true; });
    }

    public void Init(JoystickType joystickType)
    {
        Debug.Log("조이스틱 초기화");
        isAlreadySave = false;
        if (joystick)
        {
            joystick.gameObject.SetActive(false);
        }

        jumpButton.gameObject.SetActive(false);

        if (joystickType == JoystickType.Dynamic)
        {
            joystick = dynamicJoystick;
        }
        else if (joystickType == JoystickType.Fixed)
        {
            joystick = fixedJoyStick;
        }
    }

    /// <summary>
    /// 조이스틱 상태 초기화하는 함수
    /// </summary>
    /// <param name="isOn">JoyStick On/Off</param>
    public void InitializeJoyStick(bool isOn)
    {
        if (!joystick)
        {
            return;
        }

        Debug.Log(joystick.GetType());
        Debug.Log(isOn);
        joystick.gameObject.SetActive(isOn);
        if (joystick.GetType() == typeof(DynamicJoystick))
        {
            joystick.transform.GetChild(0).gameObject.SetActive(false);
            joystick.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position = default;
        }
        else if (joystick.GetType() == typeof(FixedJoystick))
        {
            jumpButton.gameObject.SetActive(isOn);
            joystick.transform.GetChild(0).gameObject.SetActive(false);
        }

        joystick.OnPointerUp();
        inputDegree = 0;
        inputDirection = Vector2.zero;
        joystick.input = Vector2.zero;
        inputJump = false;
    }

    /// <summary>
    /// 조이스틱 멈추고 이전 상태에 따라 키거나 끄는 함수
    /// ex) 대화 이전에 조이스틱을 사용하지 않으면 계속 사용하지 않는다.
    /// </summary>
    /// <param name="isStop">Save여부   true - save, false - load</param>
    public void StopSaveLoadJoyStick(bool isSave)
    {
        Debug.Log((isSave ? "Save" : "Load") + ", " + (isAlreadySave ? "저장된 상태" : "저장되지 않은 상태"));
        //처음 실행하는 경우
        if (isSave)
        {
            if (isAlreadySave) return;

            isAlreadySave = true;
            wasJoystickUse = joystick.gameObject.activeSelf;
            InitializeJoyStick(false);
        }
        // Load하는 경우
        else
        {
            Debug.Log("저장된 상태 - " + wasJoystickUse);
            isAlreadySave = false;
            InitializeJoyStick(wasJoystickUse);
        }
    }

    public void SetJoystickArea(CustomEnum.JoystickAreaType joystickAreaType)
    {
        var rect = joystick.GetComponent<RectTransform>();
        if (joystickAreaType == CustomEnum.JoystickAreaType.DEFAULT)
        {
            rect.anchorMax = new Vector2(.5f, .5f);
        }
        else if (joystickAreaType == CustomEnum.JoystickAreaType.FULL)
        {
            rect.anchorMax = Vector2.one;
        }
        else if (joystickAreaType == CustomEnum.JoystickAreaType.NONE)
        {
            rect.anchorMax = Vector2.zero;
        }

        Debug.Log("조이스틱 영역 조절" + rect.anchorMax);
    }

    public void JoystickInputUpdate(CustomEnum.JoystickInputMethod method)
    {
        //사이드뷰 일 때
        if (method.Equals(CustomEnum.JoystickInputMethod.OneDirection))
        {
            inputDegree = Mathf.Abs(joystick.Horizontal); // 조정된 입력 방향으로 크기 계산
            inputDirection.Set(joystick.Horizontal, 0); // 조정된 입력 방향 설정
            inputJump = joystick.Vertical > 0.5f; // 수직 입력이 일정 수치 이상 올라가면 점프 판정
        }
        else
        {
            inputDirection.Set(joystick.Horizontal, joystick.Vertical); // 조정된 입력 방향 설정
            inputDegree = Vector2.Distance(Vector2.zero, inputDirection); // 조정된 입력 방향으로 크기 계산
            inputJump = false;
        }
    }
}