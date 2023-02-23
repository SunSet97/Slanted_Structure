﻿using System;
using Move;
using UnityEngine;
using Utility.Core;
using static Data.CustomEnum;

namespace CommonScript
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
            MoveHorizontal = 0f;
            MoveVerical = 0f;
            UseGravity = false;

            CharacterAnimator.SetFloat(SpeedHash, 0f);

            CharacterAnimator.SetBool(TwoSideHash, false);
            CharacterAnimator.SetBool(EatHash, false);
            CharacterAnimator.SetBool(SeatHash, false);
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
            else if (DataController.Instance.CurrentMap.positionSets[posSetIndex].isMain)
            {
                transform.position = mapSettingTransform.position;
                Debug.Log("전  " + transform.rotation.eulerAngles);
                transform.LookAt(transform.position + mapSettingTransform.right);
                Debug.Log("후  " + transform.rotation.eulerAngles);

                gameObject.layer = LayerMask.NameToLayer("Player");
            }

            Emotion = Expression.IDLE;
            UseGravity = true;

            gameObject.SetActive(true);


            var yRotation = DataController.Instance.camInfo.camRot.y * Mathf.Deg2Rad;
            var cameraRight = new Vector3(Mathf.Sin(yRotation), 0, Mathf.Cos(yRotation));

            characterOriginRot = transform.eulerAngles;

            var dot = Vector3.Dot(transform.forward, cameraRight);
            if (dot > 0)
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
        public float MoveHorizontal; // 수평, 수직 이동 방향 벡터

        [NonSerialized] public float MoveVerical; // 수평, 수직 이동 방향 벡터

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
            // 캐릭터를 이 함수로 조종할 수 있을때 (조이스틱 외 미포함)
            if (IsMove)
            {
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
                    else if (joystickInputMethod == JoystickInputMethod.Other)
                    {
                        if (Mathf.Abs(joystickDeltaAngle) > 0)
                        {
                            transform.Rotate(Vector3.up, joystickDeltaAngle);
                        }
                    }

                    CharacterAnimator.SetFloat(SpeedHash, JoystickController.Instance.inputDegree);
                }

                if (UseGravity && !CharacterController.isGrounded)
                {
                    MoveVerical += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
                }
                else if (CharacterController.isGrounded)
                {
                    if (MoveVerical <= 0)
                    {
                        MoveVerical = 0;
                    }

                    if (JoystickController.Instance.InputJump)
                    {
                        Jump();
                    }
                }

                CharacterController.Move(new Vector2(MoveHorizontal, MoveVerical) * Time.fixedDeltaTime);
            }
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

            if (UseGravity && !CharacterController.isGrounded)
            {
                MoveVerical += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
            }
            else if (CharacterController.isGrounded)
            {
                if (MoveVerical <= 0)
                {
                    MoveVerical = 0;
                }

                if (JoystickController.Instance.InputJump)
                {
                    Jump();
                }
            }

            CharacterController.Move(new Vector2(MoveHorizontal, MoveVerical) * Time.fixedDeltaTime);
        }

        // 점프 중에 이동속도 느려짐, RootMotion 때문으로 추측
        public void Jump()
        {
            CharacterAnimator.SetBool(TwoSideHash, false);
            JoystickController.Instance.InputJump = false;
            MoveVerical = jumpForce;
            CharacterAnimator.SetTrigger(JumpHash);
        }

        #endregion

        private void EmotionAnimationSetting()
        {
            if (faceExpression.Length <= (int)Emotion || Emotion == Expression.None)
            {
                return;
            }

            CharacterAnimator.SetInteger(EmotionHash, (int)Emotion);
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