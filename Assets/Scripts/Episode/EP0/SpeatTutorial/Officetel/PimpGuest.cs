using System.Collections;
using UnityEngine;
using Utility.Character;
using Utility.Core;
using Utility.Dialogue;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class PimpGuest : MonoBehaviour
    {
#pragma warning disable 0649
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
        
        [SerializeField] private int floor;
#pragma warning restore 0649

        private PimpMiniGameManager pimpMiniGameManager;
        private CharacterController characterController;
        private int rotVal = 180;
        private bool speedUp;

        private const float SameFloorRange = 0.5f; // y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
        private readonly int speedHash = Animator.StringToHash("Speed");

        public void Init(PimpMiniGameManager miniGameManager)
        {
            pimpMiniGameManager = miniGameManager;
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

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, rotVal, 0), rotationSpeed * Time.fixedDeltaTime);

            if (!speedUp && CheckSameFloorWithCharacter() && CheckCamera() && (speatAbility.IsUsingAbilityTimer || speatAbility.IsPassing))
            {
                speedUp = true;
                StartCoroutine(SpeedUpFunc());
            }
        }


        public void Think()
        {
            if (!pimpMiniGameManager.IsPlay)
            {
                return;
            }
            var animator = GetComponent<Animator>();
            nextDirection = Random.Range(-1, 2);
            switch(nextDirection)
            {
                // 오른쪽으로 움직임
                case 1:
                    animator.SetFloat(speedHash, 1.0f);
                    rotVal = 90;
                    break;
                // 왼쪽으로 움직임
                case -1:
                    animator.SetFloat(speedHash, 1.0f);
                    rotVal = -90;
                    break;
                // 정면 바라보고 정지
                case 0:
                    animator.SetFloat(speedHash, 0.0f);
                    rotVal = 180;
                    break;
            }

            Invoke(nameof(Think), changeDirectionCycle);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!pimpMiniGameManager.IsPlay)
            {
                return;
            }
            
            if (other.gameObject.CompareTag("NPC") || other.gameObject.CompareTag("Wall") && other.gameObject != gameObject)
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
            else if (other.TryGetComponent(out CharacterManager characterManager) && characterManager.Equals(DataController.Instance.GetCharacter(CharacterType.Main)))
            {
                pimpMiniGameManager.EndPlay(false);
                
                if (jsonFile)
                {
                    DialogueController.Instance.AddDialogueEndAction(() =>
                    {
                        DataController.Instance.CurrentMap.ResetMap();
                    });
                    DialogueController.Instance.StartConversation(jsonFile.text);
                }
                else
                {
                    DataController.Instance.CurrentMap.ResetMap();
                }
            }
        }

        // pimp나 guest가 카메라에 걸리는지 확인하는 함수
        private bool CheckCamera()
        {
            var cam = DataController.Instance.Cam;
            var screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
            var onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            return onScreen;
        }

        private bool CheckSameFloorWithCharacter()
        {
            return floor == speatAbility.Floor;
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