﻿using System.Collections;
using CommonScript;
using Data;
using UnityEngine;
using Utility.Core;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class PimpGuest : MonoBehaviour
    {
        [SerializeField] private TextAsset jsonFile;

        [Header("스핏")] [Space(10)] [SerializeField]
        private SpeatAbility speatAbility;

        [SerializeField] private float changeDirectionCycle;
        [SerializeField] private float speedUpTime;

        [Header("적")] [Space(10)] [SerializeField]
        private float moveSpeed;

        [SerializeField] private float rotationSpeed;
        [SerializeField] private float increaseSpeed;
        [SerializeField] private int nextDirection;

        private PimpGameManager pimpGameManager;
        private CharacterController characterController;
        private int rotVal = 180;
        private bool speedUp;

        private const float SameFloorRange = 0.5f; // y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
        private readonly int speedHash = Animator.StringToHash("Speed");

        public void Init(PimpGameManager gameManager)
        {
            pimpGameManager = gameManager;
            characterController = GetComponent<CharacterController>();
            var animator = GetComponent<Animator>();
            animator.SetFloat(speedHash, 0f);
        }

        public void Move(bool isPlay)
        {
            if (!characterController.isGrounded)
            {
                characterController.Move(transform.up * -1);
            }

            if (!isPlay)
            {
                return;
            }

            characterController.Move(Vector3.right * nextDirection * moveSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, rotVal, 0),
                rotationSpeed * Time.fixedDeltaTime);

            if (!speedUp && CheckSameFloorWithCharacter() && CheckCamera() &&
                (speatAbility.IsUsingAbilityTimer || speatAbility.IsPassing))
            {
                speedUp = true;
                StartCoroutine(SpeedUpFunc());
            }
        }


        public void Think()
        {
            if (!pimpGameManager.IsPlay)
            {
                Invoke(nameof(Think), changeDirectionCycle);
                return;
            }

            var animator = GetComponent<Animator>();
            nextDirection = Random.Range(-1, 2);
            if (nextDirection == 1) // 오른쪽으로 움직임
            {
                animator.SetFloat(speedHash, 1.0f);
                rotVal = 90;
            }
            else if (nextDirection == -1) // 왼쪽으로 움직임
            {
                animator.SetFloat(speedHash, 1.0f);
                rotVal = -90;
            }
            else if (nextDirection == 0) // 정면 바라보고 정지
            {
                animator.SetFloat(speedHash, 0.0f);
                rotVal = 180;
            }

            Invoke(nameof(Think), changeDirectionCycle);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!pimpGameManager.IsPlay)
            {
                return;
            }

            if (hit.collider.TryGetComponent(out CharacterManager characterManager) &&
                characterManager.who.Equals(CustomEnum.Character.Main))
            {
               characterManager.gameObject.layer = LayerMask.NameToLayer("Player");

                if (jsonFile)
                {
                    characterManager.IsMove = false;
                    
                    pimpGameManager.EndPlay();

                    DialogueController.Instance.SetDialougueEndAction(() =>
                    {
                        characterManager.IsMove = true;
                        DataController.Instance.ChangeMap(DataController.Instance.mapCode);
                    });
                    DialogueController.Instance.StartConversation(jsonFile.text);
                }
                else
                {
                    DataController.Instance.ChangeMap(DataController.Instance.mapCode);
                }
            }
            else if (hit.gameObject.CompareTag("NPC"))
            {
                nextDirection *= -1;

                if (nextDirection == 1)
                {
                    rotVal = 90;
                }
                else if (nextDirection == -1)
                {
                    rotVal = -90;
                }

            }
        }

        // pimp나 guest가 카메라에 걸리는지 확인하는 함수
        private bool CheckCamera()
        {
            var cam = DataController.Instance.Cam;
            Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 &&
                            screenPoint.y < 1;

            return onScreen;
        }

        private bool CheckSameFloorWithCharacter()
        {
            var character = DataController.Instance.GetCharacter(CustomEnum.Character.Main);
            return Mathf.Abs(gameObject.transform.position.y - character.transform.position.y) <= SameFloorRange;
        }

        private IEnumerator SpeedUpFunc()
        {
            var originSpeed = moveSpeed;
            moveSpeed += increaseSpeed;
            yield return new WaitForSeconds(speedUpTime);

            moveSpeed = originSpeed;
            speedUp = false;
        }
    }
}