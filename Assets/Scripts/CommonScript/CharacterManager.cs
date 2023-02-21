using System;
using Move;
using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace CommonScript
{
    public class CharacterManager : MonoBehaviour, IMovable
    {
        [NonSerialized] public CharacterController CharacterController;
        [NonSerialized] public Animator Animator;
    
        public Character who;
        [SerializeField] private Transform waitTransform;
    
        private SkinnedMeshRenderer skinnedMesh; // 캐릭터 머테리얼
        private Texture[] faceExpression; //표정 메터리얼

        public void Init()
        {
            Animator = GetComponent<Animator>();
            CharacterController = GetComponent<CharacterController>();
        
            Animator.SetFloat(SpeedHash, 0);
            // 표정 머테리얼 초기화
            if (who.Equals(Character.Speat) || who.Equals(Character.Oun) || who.Equals(Character.Rau))
            {
                faceExpression = Resources.LoadAll<Texture>("Face");
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
                if (faceExpression.Length < (int)Emotion)
                {
                    return;
                }
                skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int) Emotion]);
            }
            //스핏 표정 머테리얼 초기화
            if (who.Equals(Character.Speat_Adolescene) || who.Equals(Character.Speat_Adult) || who.Equals(Character.Speat_Child))
            {
                faceExpression = Resources.LoadAll<Texture>($"Speat_Face/{who}");
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
                if (faceExpression.Length < (int)Emotion)
                {
                    return;
                }
                skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int)Emotion]);
            }
        }

        #region 캐릭터 컨트롤

        public bool IsMove { get; set; } //움직일 수 있는지 여부 (움직임을 억지로 막을 때 사용)

        // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
        public void PickUpCharacter()
        {
            IsMove = false;
            Animator.applyRootMotion = false;
        }

        // 캐릭터를 스크립트로 직접 이동할 수 있게 함 (캐릭터를 손으로 집는다고 생각)
        public void PutDownCharacter()
        {
            IsMove = true;
            Animator.applyRootMotion = true;
        }

        public void InitializeCharacter()
        {
            Emotion = Expression.IDLE;
            gameObject.layer = LayerMask.NameToLayer("Default");
            moveHorDir = Vector3.zero;
            moveVerDir = Vector3.zero;
            Animator.SetFloat(SpeedHash, 0f);
        
            Animator.SetBool(JumpHash, false);
            Animator.SetBool(TwoSideHash, false);
            Animator.SetBool(EatHash, false);
            Animator.SetBool(SeatHash, false);
        }

        public void SetCharacter(Transform mapSettingTransform)
        {
            var posSetIndex = DataController.Instance.CurrentMap.positionSets.FindIndex(item => item.who == who);
            if (posSetIndex == -1)
            {
                return;
            }

            if (DataController.Instance.CurrentMap.positionSets[posSetIndex].isFollow)
            {
                var mainPosSet = DataController.Instance.CurrentMap.positionSets.Find(item => item.isMain);
                var targetPos = (mainPosSet.startPosition.position - mainPosSet.startPosition.right) * followDistance;

                targetPos.y = mainPosSet.startPosition.position.y;

                transform.position = targetPos;
                transform.LookAt(mainPosSet.startPosition);
            
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
            else if(DataController.Instance.CurrentMap.positionSets[posSetIndex].isMain)
            {
                transform.position = mapSettingTransform.position;
                transform.LookAt(transform.position + mapSettingTransform.right);
            
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
        
            Emotion = Expression.IDLE;
        
            gameObject.SetActive(true);
        
            characterOriginRot = transform.eulerAngles;

            camRotation = Quaternion.Euler(0, -DataController.Instance.camInfo.camRot.y, 0);
            Vector3 transformedDir = camRotation * transform.forward;

            if (transformedDir.x < 0)
                characterOriginRot.y += 180f;
        
            Debug.Log($"{who}  세팅: " + transform.position);
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
        private Expression emotion;

        public Expression Emotion
        {
            get => emotion;
            set
            {
                emotion = value;
                EmotionAnimationSetting();
            }
        }

        // 현재 Emotion상태값 넣기
        private void EmotionAnimationSetting()
        {
            if (faceExpression.Length < (int)Emotion || Emotion == Expression.NONE)
            {
                return;
            }
            skinnedMesh.materials[1].SetTexture("_MainTex", faceExpression[(int) Emotion]);
        }

        #endregion

        #region 캐릭터 이동 설정

        [Header("#Character move setting")] public Vector3 moveHorDir, moveVerDir; // 수평, 수직 이동 방향 벡터
        public float joyRot;
        public Quaternion camRotation; // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)

        private Vector3 characterOriginRot; // 캐릭터의 기존 방향

        public float jumpForce = 5f; // 점프력
        public float gravityScale = 0.6f; // 중력 배수
        public float airResistance = 1.2f; // 공기 저항
    
        [SerializeField] private float followDistance = 1f;
        [SerializeField] private float followSpeed = 1f;
        private float lastFollowSpeed;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int TwoSideHash = Animator.StringToHash("2DSide");
        private static readonly int DirectionHash = Animator.StringToHash("Direction");
        private static readonly int SeatHash = Animator.StringToHash("Seat");
        private static readonly int EatHash = Animator.StringToHash("Eat");

        private void Move2DSide(float x)
        {
            Animator.SetBool(TwoSideHash, true);

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
            Animator.SetBool(TwoSideHash, false);
            if (Mathf.Abs(joyRot) > 0)
            {
                transform.Rotate(Vector3.up, joyRot);
            } // 임시 회전

            Animator.SetFloat(DirectionHash, joyRot); //X방향
        }

        public void MoveCharacter(JoystickInputMethod joystickInputMethod)
        {
            // 캐릭터를 이 함수로 조종할 수 있을때 (조이스틱 외 미포함)
            if (IsMove)
            {
                // 메인 카메라 기준으로 캐릭터가 바라보는 방향 계산
                camRotation = Quaternion.Euler(0, -DataController.Instance.Cam.transform.rotation.eulerAngles.y, 0);
                Vector3 transformedDir = camRotation * transform.forward;
                Vector2 characterDir = new Vector2(transformedDir.x, transformedDir.z);
                // 조이스틱이 가리키는 방향
                Vector2 joystickDir = new Vector2(JoystickController.instance.inputDirection.x,
                    JoystickController.instance.inputDirection.y);

                joyRot = Vector2.SignedAngle(joystickDir, characterDir);
                //사이드뷰 일 때
                if (joystickInputMethod.Equals(JoystickInputMethod.OneDirection))
                {
                    Move2DSide(joystickDir.x);
                }
                //쿼터뷰일 때    
                else if (joystickInputMethod.Equals(JoystickInputMethod.AllDirection))
                {
                    QuarterView();
                }

                if (joystickInputMethod.Equals(JoystickInputMethod.Other))
                {
                    if (Mathf.Abs(joyRot) > 0)
                    {
                        transform.Rotate(Vector3.up, joyRot);
                    } // 임시 회전
                }

                Animator.SetFloat(SpeedHash, JoystickController.instance.inputDegree);
                //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
                if (JoystickController.instance.inputJump && CharacterController.isGrounded)
                {
                    moveVerDir.y = 0;
                    Animator.SetBool(JumpHash, true); //점프 가능 상태로 변경
                }


                //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
                if (!CharacterController.isGrounded)
                {
                    moveVerDir.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
                }
                //땅에 붙어있을 경우
                else
                {
                    moveVerDir.y = 0;
                    //캐릭터 점프 가능
                    if (JoystickController.instance.inputJump && Animator.GetBool(JumpHash))
                    {
                        moveVerDir.y += jumpForce; //점프력 만큼 힘을 가함
                        Animator.SetBool(JumpHash, false); //점프 불가능 상태로 변경하여 연속적인 점프 제한
                    }
                }

                CharacterController.Move((moveHorDir + moveVerDir) * Time.fixedDeltaTime); //캐릭터를 최종 이동 시킴
            }
            else
            {
                Animator.SetFloat(SpeedHash, 0);
            }
        }

        public void FollowMainCharacter(JoystickInputMethod joystickInputMethod)
        {
            if (joystickInputMethod.Equals(JoystickInputMethod.OneDirection))
            {
                Animator.SetBool(TwoSideHash, true);
            }
            else if (joystickInputMethod.Equals(JoystickInputMethod.AllDirection))
            {
                Animator.SetBool(TwoSideHash, false);
            }
        
        
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);

            var targetPos = (mainCharacter.transform.position - mainCharacter.transform.forward) * followDistance;

            targetPos.y = mainCharacter.transform.position.y;

            var distance = Vector2.Distance(new Vector2(targetPos.x, targetPos.z),
                new Vector2(transform.position.x, transform.position.z));
        
            var lookPos = mainCharacter.transform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        
            var distanceRatio = distance / followDistance;
        
            if (distanceRatio <= 1)
            {
                var curFollowSpeed = Mathf.Lerp(lastFollowSpeed, 0f, 0.95f);
                Animator.SetFloat(SpeedHash, curFollowSpeed);
                lastFollowSpeed = curFollowSpeed;
            }
            else
            {
                Animator.SetFloat(SpeedHash, (distanceRatio - 1) * followSpeed);
            }

            if (JoystickController.instance.inputJump && CharacterController.isGrounded)
            {
                moveVerDir.y = 0;
                Animator.SetBool(JumpHash, true);
            }

            if (!CharacterController.isGrounded)
            {
                moveVerDir.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
            }
            else
            {
                moveVerDir.y = 0;
            
                if (JoystickController.instance.inputJump && Animator.GetBool(JumpHash))
                {
                    moveVerDir.y += jumpForce;
                    Animator.SetBool(JumpHash, false);
                }
            }
            CharacterController.Move((moveHorDir + moveVerDir) * Time.fixedDeltaTime);
        }

        #endregion

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && DataController.Instance.GetCharacter(Character.Main) == this)
            {
                var scale = 1f;
                var a = (transform.position - transform.forward) * scale;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(a, 1f);
            }
        }
    }
}