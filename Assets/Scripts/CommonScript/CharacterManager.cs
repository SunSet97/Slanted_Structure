using Move;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Data.CustomEnum;

public class CharacterManager : MonoBehaviour, IMovable
{
    #region 기본 설정
    [Header("#Default inspector setting(auto)")]
    public Joystick joyStick; // 조이스틱
    public CharacterController ctrl; // 캐릭터컨트롤러

    public Animator anim; // 애니메이션
    private SkinnedMeshRenderer skinnedMesh; // 캐릭터 머테리얼
    private Texture[] faceExpression;//표정 메터리얼

    public Character who;
    private Camera cam; // 카메라
    [Tooltip("캐릭터 Pos을 넣으시오.")]
    [SerializeField] private Transform waitTransform;

    void Start()
    {
        ctrl = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        joyStick = DataController.instance.joyStick;
        cam = DataController.instance.cam;
        // 표정 머테리얼 초기화
        if (who.Equals(Character.Speat) || who.Equals(Character.Oun) || who.Equals(Character.Rau))
        {
            faceExpression = Resources.LoadAll<Texture>("Face");
            skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int) emotion]);
        }
    }

    void Update()
    {
        // 에니메이션 설정
        AnimationSetting();
    }
    #endregion

    #region 캐릭터 컨트롤
    
    public bool IsMove { get; set; }    //움직일 수 있는지 여부 (움직임을 억지로 막을 때 사용)

    // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
    public void PickUpCharacter()
    {
        IsMove = false;
        anim.applyRootMotion = false;
    }

    // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
    public void PutDownCharacter()
    {
        IsMove = true;
        anim.applyRootMotion = true;
    }
    public void InitializeCharacter()
    {
        gameObject.layer = 0;
        moveHorDir = Vector3.zero;
        moveVerDir = Vector3.zero;
        anim.SetFloat(SpeedHash, 0f);
    }
    public void SetCharacter(Transform mapSettingTransform)
    {
        gameObject.SetActive(true);
        transform.position = mapSettingTransform.position;
        transform.LookAt(transform.position + mapSettingTransform.right);
        characterOriginRot = transform.eulerAngles;

        camRotation = Quaternion.Euler(0, -DataController.instance.camRot.y, 0);
        Vector3 transformedDir = camRotation * transform.forward;

        if(transformedDir.x < 0)
            characterOriginRot.y += 180f;
    }

    //대기 방으로 이동하는 함수
    public void WaitInRoom()
    {
        gameObject.SetActive(false);
        transform.position = waitTransform.position;
        transform.rotation = waitTransform.rotation;
    }
    // 일정 시간 후 캐릭터를 조이스틱으로 움직이게 함
    public void UseJoystickCharacter()
    {
        Invoke("PutDownCharacter", Time.fixedDeltaTime);
    }
    #endregion

    #region 캐릭터 애니메이션 설정
    // 감정상태
    public enum Emotion { Idle, Laugh, Sad, Cry, Angry, Surprise, Panic, Suspicion, Fear, Curious };
    public Emotion emotion = Emotion.Idle;      // 캐릭터 감정

    // 에니메이션 설정
    void AnimationSetting()
    {
        EmotionAnimationSetting();
        ActionAnimationSetting();
    }
    // 현재 Emotion상태값 넣기
    void EmotionAnimationSetting()
    {
        if (SceneManager.GetActiveScene().name == "Cinematic")
        {
            anim.SetInteger("Emotion", (int)emotion); // 애니메이션실행
        }
        if (who.Equals(Character.Speat) || who.Equals(Character.Oun) || who.Equals(Character.Rau))
            skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int)emotion]); // 현재 감정으로 메터리얼 변경
    }

    // public void EmotionAnimationSetting(Emotion inEmotion)
    // {
    //     if (SceneManager.GetActiveScene().name == "Cinematic")
    //     {
    //         anim.SetInteger("Emotion", (int)inEmotion); // 애니메이션실행
    //     }
    //     skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int)inEmotion]); // 현재 감정으로 메터리얼 변경
    // }

    public void SetCinematic()
    {
        faceExpression = Resources.LoadAll<Texture>("Face");
        skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    // 액션 관련
    void ActionAnimationSetting()
    {
        if (SceneManager.GetActiveScene().name == "Ingame")
        {
            // 대시
            if (DataController.instance.inputDash == true)
            {
                anim.SetTrigger("Dash");
                DataController.instance.inputDash = false;
            }

            // 앉기
            /*  인터렉션 으로 의자 주변에 있을 때, 의자 자체에 트리거가 활성화 되고, /희원이랑 이야기해야될 부분
            *  터치하면 bool값이 변경되는 것으로 세팅할 필요가 있음!*/
            if (DataController.instance.inputSeat == true)
                anim.SetBool("Seat", true);
            else
                anim.SetBool("Seat", false);

            // 사망
            /*죽음 상태를 DataController에 bool값 추가하여
            플레이어의 상태값을 인지하고 Dead가 true이면 사망 상태의 애니메이션이 활성화되고
            게임 오버 or 리스폰 할 수 있도록 해야할 듯.*/
            // anim.SetBool("Dead", true);
        }
    }
    #endregion

    #region 캐릭터 이동 설정
    [Header("#Character move setting")]
    public Vector3 moveHorDir = default, moveVerDir = default;    // 수평, 수직 이동 방향 벡터
    public float joyRot;
    public Quaternion camRotation; // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)

    private Vector3 characterOriginRot; // 캐릭터의 기존 방향

    public bool isJump;                     // 캐릭터의 점프 여부
    public float jumpForce = 5f;            // 점프력
    public float gravityScale = 0.6f;       // 중력 배수
    public float airResistance = 1.2f;      // 공기 저항

    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Move2DSide(float x)
    {
        anim.SetBool("2DSide", true);

        Vector2 characterRot = default;
        if (x < 0)
        {
            characterRot.y = characterOriginRot.y + 180;
            transform.eulerAngles = characterRot;
        }
        else if (x > 0)
        {
            characterRot.y = characterOriginRot.y;
            transform.eulerAngles = characterRot;
        }
    }
    private void QuarterView()
    {
        anim.SetBool("2DSide", false);
        if (Mathf.Abs(joyRot) > 0) { transform.Rotate(Vector3.up, joyRot); } // 임시 회전
        anim.SetFloat("Direction", joyRot); //X방향
    }

    public void MoveCharacter(JoystickInputMethod joystickInputMethod)
    {
        // 캐릭터를 이 함수로 조종할 수 있을때 (조이스틱 외 미포함)
        if (IsMove)
        {
            // 메인 카메라 기준으로 캐릭터가 바라보는 방향 계산
            camRotation = Quaternion.Euler(0, -cam.transform.rotation.eulerAngles.y, 0);
            Vector3 transformedDir = camRotation * transform.forward;
            Vector2 characterDir = new Vector2(transformedDir.x, transformedDir.z);
            // 조이스틱이 가리키는 방향
            Vector2 joystickDir = new Vector2(DataController.instance.inputDirection.x, DataController.instance.inputDirection.y);

            joyRot = Vector2.SignedAngle(joystickDir, characterDir);
            //사이드뷰 일 때
            if (joystickInputMethod.Equals(JoystickInputMethod.OneDirection))
            {
                Move2DSide(joystickDir.x);
            }
            //쿼터뷰일 때    
            else if(joystickInputMethod.Equals(JoystickInputMethod.AllDirection))
            {
                QuarterView();
            }
            if(joystickInputMethod.Equals(JoystickInputMethod.Other))
            {
                if (Mathf.Abs(joyRot) > 0) { transform.Rotate(Vector3.up, joyRot); } // 임시 회전
            }
            anim.SetFloat(SpeedHash, DataController.instance.inputDegree);
            //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
            if (DataController.instance.inputJump && ctrl.isGrounded)
            {
                moveVerDir.y = 0;
                anim.SetBool("Jump", true);  //점프 가능 상태로 변경
            }


            //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
            if (!ctrl.isGrounded)
            {
                moveVerDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            }
            //땅에 붙어있을 경우
            else
            {
                moveVerDir.y = 0;
                //캐릭터 점프 가능
                if (DataController.instance.inputJump && anim.GetBool("Jump"))
                {
                    moveVerDir.y += jumpForce; //점프력 만큼 힘을 가함
                    anim.SetBool("Jump", false); //점프 불가능 상태로 변경하여 연속적인 점프 제한
                }
            }
            ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 최종 이동 시킴
        }
        else
        {
            //anim.SetFloat(SpeedHash, 0); //Speed
        }
    }
    #endregion
}