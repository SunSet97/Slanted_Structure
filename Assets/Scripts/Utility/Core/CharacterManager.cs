using System;
using Move;
using UnityEngine;
using static Data.CustomEnum;

namespace Utility.Core
{
    public class CharacterManager : MonoBehaviour, IMovable
    {
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

        public Character who;

        [SerializeField] private Transform waitTransform;

        [NonSerialized] public CharacterController CharacterController;

        [NonSerialized] public Animator CharacterAnimator;

        [NonSerialized] public bool UseGravity;

        private SkinnedMeshRenderer skinnedMesh; // 캐릭터 머테리얼
        private Texture[] faceExpression; //표정 메터리얼 

        public void Init()
        {
            CharacterAnimator = GetComponent<Animator>();
            CharacterController = GetComponent<CharacterController>();

            CharacterAnimator.SetFloat(SpeedHash, 0);

            if (who.Equals(Character.Speat) || who.Equals(Character.Oun) || who.Equals(Character.Rau))
            {
                faceExpression = Resources.LoadAll<Texture>("Face");
            }
            else if (who.Equals(Character.Speat_Adolescene) || who.Equals(Character.Speat_Adult) ||
                     who.Equals(Character.Speat_Child))
            {
                faceExpression = Resources.LoadAll<Texture>($"Speat_Face/{who}");
            }

            skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (faceExpression.Length <= (int)Emotion)
            {
                return;
            }

            skinnedMesh.materials[1].SetTexture(MainTex, faceExpression[(int)Emotion]);
        }

        public bool IsMove { get; set; }

        public void PickUpCharacter()
        {
            IsMove = false;
            CharacterAnimator.applyRootMotion = false;
        }

        public void PutDownCharacter()
        {
            IsMove = true;
            CharacterAnimator.applyRootMotion = true;
        }

        public void UseJoystickCharacter()
        {
            Invoke(nameof(PutDownCharacter), Time.fixedDeltaTime);
        }

        public void InitializeCharacter()
        {
            Emotion = Expression.IDLE;
            gameObject.layer = LayerMask.NameToLayer("Default");
            MoveHorizontal = Vector3.zero;
            MoveVerical = Vector3.zero;
            UseGravity = false;

            CharacterAnimator.SetFloat(SpeedHash, 0f);

            CharacterAnimator.SetBool(TwoSideHash, false);
            CharacterAnimator.SetBool(EatHash, false);
            CharacterAnimator.SetBool(SeatHash, false);
        }

        public void SetCharacter(MapData.CharacterPositionSet posSet)
        {
            if (posSet.isFollow)
            {
                var mainPosSet = DataController.Instance.CurrentMap.positionSets.Find(item => item.isMain);
                var targetPos = (mainPosSet.startPosition.position - mainPosSet.startPosition.right) * followDistance;

                targetPos.y = mainPosSet.startPosition.position.y;

                transform.position = targetPos;
                transform.LookAt(mainPosSet.startPosition);

                gameObject.layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                transform.position = posSet.startPosition.position;
                transform.LookAt(transform.position + posSet.startPosition.right);

                if (posSet.isMain)
                {
                    gameObject.layer = LayerMask.NameToLayer("Player");
                }
                else
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }

            Emotion = Expression.IDLE;
            UseGravity = true;

            gameObject.SetActive(true);

            characterOriginRot = transform.eulerAngles;

            if (DataController.Instance.CurrentMap.method == JoystickInputMethod.OneDirection &&
                !DataController.Instance.CurrentMap.rightIsForward)
            {
                characterOriginRot.y += 180f;
                Debug.Log("정방향이 아님 반전시킴");
            }

            Debug.Log($"{who}  세팅: " + transform.position);
        }

        public void WaitInRoom()
        {
            gameObject.SetActive(false);
            transform.position = waitTransform.position;
            transform.rotation = waitTransform.rotation;
        }

        #region 캐릭터 이동 설정

        [Header("#Character move setting")] [NonSerialized]
        public Vector3 MoveHorizontal; // 수평, 수직 이동 방향 벡터

        [NonSerialized] public Vector3 MoveVerical; // 수평, 수직 이동 방향 벡터

        private Vector3 characterOriginRot; // 캐릭터의 기존 방향

        public float jumpForce = 5f; // 점프력
        public float gravityScale = 0.6f; // 중력 배수
        public float airResistance = 1.2f; // 공기 저항

        [SerializeField] private float followDistance = 1f;

        [SerializeField] private float followSpeed = 1f;

        private float lastFollowSpeed;
        private bool isGrounded;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int TwoSideHash = Animator.StringToHash("2DSide");
        private static readonly int DirectionHash = Animator.StringToHash("Direction");
        private static readonly int SeatHash = Animator.StringToHash("Seat");
        private static readonly int EatHash = Animator.StringToHash("Eat");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int EmotionHash = Animator.StringToHash("Emotion");

        public void RotateCharacter2D(float x)
        {
            Vector2 characterRot = transform.eulerAngles;
            if (x < 0)
            {
                characterRot.y = characterOriginRot.y + 180f;
                transform.eulerAngles = characterRot;
            }
            else if (x > 0)
            {
                characterRot.y = characterOriginRot.y;
                transform.eulerAngles = characterRot;
            }
        }

        private void Move2DSide(float x)
        {
            if (!Mathf.Approximately(x, 0f))
            {
                CharacterAnimator.SetBool(TwoSideHash, true);
            }

            RotateCharacter2D(x);
        }

        private void QuarterView(float joystickAngle)
        {
            CharacterAnimator.SetBool(TwoSideHash, false);
            if (Mathf.Abs(joystickAngle) > 0)
            {
                transform.Rotate(Vector3.up, joystickAngle);
            }

            CharacterAnimator.SetFloat(DirectionHash, joystickAngle);
        }

        public void MoveCharacter(JoystickInputMethod joystickInputMethod, bool isJoystickInputUse)
        {
            // Jump -> Jump에서 isGround = false;
            // 떨어지는 경우 -> 바닥에 붙어있다고 판정 넣고 점프 가능하게
            if (!isGrounded)
            {
                // jump하고 떨어지고 있는 경우
                
                if (CharacterController.isGrounded)
                {
                    // 점프 가능
                    isGrounded = true;
                }
            }
            
            // 캐릭터를 이 함수로 조종할 수 있을때 (조이스틱 외 미포함)
            if (!IsMove)
            {
                return;
            }

            if (isJoystickInputUse)
            {
                var characterForward2D =
                    new Vector2(Vector3.Dot(transform.forward, DataController.Instance.Cam.transform.right),
                        Vector3.Dot(transform.forward, DataController.Instance.Cam.transform.forward));

                var joystickDir = new Vector2(JoystickController.Instance.inputDirection.x,
                    JoystickController.Instance.inputDirection.y);

                var joystickDeltaAngle = Vector2.SignedAngle(joystickDir, characterForward2D);

                if (joystickInputMethod == JoystickInputMethod.OneDirection)
                {
                    Move2DSide(joystickDir.x);
                }
                else if (joystickInputMethod == JoystickInputMethod.AllDirection)
                {
                    QuarterView(joystickDeltaAngle);
                }
                else if (joystickInputMethod == JoystickInputMethod.Waypoint)
                {
                    if (Mathf.Abs(joystickDeltaAngle) > 0)
                    {
                        transform.Rotate(Vector3.up, joystickDeltaAngle);
                    }

                    DataController.Instance.CurrentMap.waypoint.JoystickUpdate();

                    // CharacterAnimator.SetFloat(DirectionHash, joystickAngle);
                }
                else if (joystickInputMethod == JoystickInputMethod.Other)
                {
                    if (Mathf.Abs(joystickDeltaAngle) > 0)
                    {
                        transform.Rotate(Vector3.up, joystickDeltaAngle);
                    }

                    // CharacterAnimator.SetFloat(DirectionHash, joystickAngle);
                }

                CharacterAnimator.SetFloat(SpeedHash, JoystickController.Instance.inputDegree);
            }

            if (UseGravity && !isGrounded)
            {
                MoveVerical.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
            }
            else if (isGrounded)
            {
                if (!CharacterController.isGrounded)
                {
                    MoveVerical.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
                }
                else if (MoveVerical.y <= 0)
                {
                    MoveVerical = Vector3.zero;
                }

                if (JoystickController.Instance.InputJump)
                {
                    Jump();
                }
            }

            CharacterController.Move((MoveHorizontal + MoveVerical) * Time.fixedDeltaTime);
        }

        public void FollowMainCharacter(JoystickInputMethod joystickInputMethod)
        {
            if (joystickInputMethod.Equals(JoystickInputMethod.OneDirection))
            {
                CharacterAnimator.SetBool(TwoSideHash, true);
            }
            else if (joystickInputMethod.Equals(JoystickInputMethod.AllDirection))
            {
                CharacterAnimator.SetBool(TwoSideHash, false);
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
                CharacterAnimator.SetFloat(SpeedHash, curFollowSpeed);
                lastFollowSpeed = curFollowSpeed;
            }
            else
            {
                CharacterAnimator.SetFloat(SpeedHash, (distanceRatio - 1) * followSpeed);
            }

            if (UseGravity && !isGrounded)
            {
                MoveVerical.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
            }
            else if (isGrounded)
            {
                if (!CharacterController.isGrounded)
                {
                    MoveVerical.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
                }
                else if (MoveVerical.y <= 0)
                {
                    MoveVerical = Vector3.zero;
                }

                if (JoystickController.Instance.InputJump)
                {
                    Jump();
                }
            }

            CharacterController.Move((MoveHorizontal + MoveVerical) * Time.fixedDeltaTime);
        }

        public void TryJump()
        {
            JoystickController.Instance.InputJump = true;
        }

        // 점프 중에 이동속도 느려짐, RootMotion 때문으로 추측
        public void Jump()
        {
            if (!isGrounded)
            {
                return;
            }

            isGrounded = false;
            JoystickController.Instance.InputJump = false;
            CharacterAnimator.SetBool(TwoSideHash, false);
            MoveVerical = new Vector3(0, jumpForce);
            CharacterAnimator.SetTrigger(JumpHash);
        }

        #endregion

        private void EmotionAnimationSetting()
        {
            CharacterAnimator.SetInteger(EmotionHash, (int)Emotion);

            if (faceExpression.Length <= (int)Emotion || Emotion == Expression.None)
            {
                return;
            }

            skinnedMesh.materials[1].SetTexture(MainTex, faceExpression[(int)Emotion]);
        }

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