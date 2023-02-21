using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Interaction.Click;
using static Data.CustomEnum;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class SpeatAbility : MonoBehaviour
    {
        [Header("-UI")] [SerializeField] private Button abilityButton;
        public Image abilityCooldownImage;
        public Text abilityText;

        [Header("-Variable")] [SerializeField] private float setDuration = 4f;
        [SerializeField] private float setCooldown = 3f;
        [SerializeField] private int maxWallCount = 2;

        [SerializeField] private float hideSpeed = 0.5f;

        private Vector2 passVector2;

        private Transform lastForward, lastBack, lastUpFloor, lastDownFloor;
        private RaycastHit wallPassTemp;

        [NonSerialized] public bool IsUsingAbilityTimer;
        [NonSerialized] public bool IsPassing;

        private float abilityDuration;
        private float cooldown;
        private int passedWallCount;

        private bool isHiding;
        private OutlineClickObj hidingDoor;
        private float originPosZ;
        private Coroutine abilityTimerCoroutine;
        
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Start()
        {
            ObjectClicker.Instance.IsCustomUse = true;
            abilityButton.onClick.AddListener(UseAbility);
            Debug.Log("ㅎㅇ");

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);

            var mainCharacterCenterPos = mainCharacter.CharacterController.center + mainCharacter.transform.position;

            var fwdDir = mainCharacter.transform.forward;
            var upDir = mainCharacter.transform.up;

            var fwdPos = mainCharacterCenterPos + fwdDir * 0.3f;
            var bwdPos = mainCharacterCenterPos + -fwdDir * 0.3f;
            var upPos = mainCharacterCenterPos + upDir * 0.5f;
            var downPos = mainCharacterCenterPos + -upDir * 0.2f;

            var wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
            var floorLayerMask = 1 << LayerMask.NameToLayer("Floor");

            // Forward
            if (Physics.Raycast(fwdPos, fwdDir, out var wallFwdFace, float.MaxValue, wallLayerMask))
            {
                lastForward = wallFwdFace.transform;
            }
            // Back
            if (Physics.Raycast(bwdPos, -fwdDir, out var wallBwdFace, float.MaxValue, wallLayerMask))
            {
                lastBack = wallBwdFace.transform;
            }
            // Up
            if (Physics.Raycast(upPos, upDir, out var upFloor, float.MaxValue, floorLayerMask))
            {
                lastUpFloor = upFloor.transform;
            }
            // Down
            if (Physics.Raycast(downPos, -upDir, out var downFloor, float.MaxValue, floorLayerMask))
            {
                lastDownFloor = downFloor.transform;
            }
        }

        private void Update()
        {
            PassDoor();
        }
        
        private void UseAbility()
        {
            if (IsPassing || isHiding || IsUsingAbilityTimer || cooldown > 0f)
            {
                return;
            }

            IsUsingAbilityTimer = true;
            passedWallCount = 0;
            abilityText.text = "0";
            abilityDuration = setDuration;
            abilityButton.image.fillAmount = 0f;
            abilityTimerCoroutine = StartCoroutine(AbilityTimer());
        }

        private IEnumerator AbilityTimer()
        {
            while (abilityDuration > 0)
            {
                Dash();

                abilityDuration -= Time.deltaTime;
                abilityButton.image.fillAmount = abilityDuration / setDuration;

                yield return null;
            }
            AbilityCooldown();
        }

        private void AbilityCooldown()
        {
            abilityText.text = "";
            IsUsingAbilityTimer = false;
            abilityButton.image.fillAmount = 0f;
            passVector2 = Vector2.zero;
            
            if (abilityTimerCoroutine != null)
            {
                StopCoroutine(abilityTimerCoroutine);
            }

            StartCoroutine(Cooldown());
        }

        private IEnumerator Cooldown()
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
            
            cooldown = setCooldown;
            abilityCooldownImage.fillAmount = 1f;

            while (cooldown > 0f)
            {
                cooldown -= Time.deltaTime;
                abilityCooldownImage.fillAmount = cooldown / setCooldown;
                yield return null;
            }
        }

        private IEnumerator CheckPass()
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            
            if (passVector2 == Vector2.left)
            {
                mainCharacter.gameObject.layer = LayerMask.NameToLayer("SpeatWallPass");
                mainCharacter.moveHorDir = Vector3.left * 6f;
            }
            else if (passVector2 == Vector2.right)
            {
                mainCharacter.gameObject.layer = LayerMask.NameToLayer("SpeatWallPass");
                mainCharacter.moveHorDir = Vector3.right * 6f;
            }
            else if (passVector2 == Vector2.down)
            {
                mainCharacter.gameObject.layer = LayerMask.NameToLayer("SpeatFloorPass");
                mainCharacter.moveVerDir = Vector3.down * 6f;
            }
            else if (passVector2 == Vector2.up)
            {
                mainCharacter.gameObject.layer = LayerMask.NameToLayer("SpeatFloorPass");
                mainCharacter.moveVerDir = Vector3.up * 15f;
            }
            
            var mainCharacterCenterPos = mainCharacter.CharacterController.center + mainCharacter.transform.position;
            
            var fwdDir = mainCharacter.transform.forward;
            var upDir = mainCharacter.transform.up;

            var wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
            var floorLayerMask = 1 << LayerMask.NameToLayer("Floor");

            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                if (passVector2 == Vector2.up)
                {
                    Debug.Log("Check Up");
                    var upPos = mainCharacterCenterPos + upDir * .5f;
                    var downPos = mainCharacterCenterPos + -upDir * .2f;
                    if (Physics.Raycast(downPos, -upDir, out var downFloor, float.MaxValue, floorLayerMask))
                    {
                        if (lastUpFloor == downFloor.transform)
                        {
                            lastDownFloor = downFloor.transform;
                            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
                            mainCharacter.moveVerDir = Vector3.zero;
                            passVector2 = Vector2.zero;
                            passedWallCount++;
                            
                            if (Physics.Raycast(upPos, upDir, out var upFloor, float.MaxValue, floorLayerMask))
                            {
                                if (lastUpFloor != upFloor.transform)
                                {
                                    lastUpFloor = upFloor.transform;
                                }
                                else
                                {
                                    Debug.LogWarning("오류 코드 고쳐야됨");
                                }
                            }
                            else
                            {
                                lastUpFloor = null;
                            }
                            break;
                        }
                    }
                }
                else if (passVector2 == Vector2.down)
                {
                    Debug.Log("Check Down");
                    var upPos = mainCharacterCenterPos + upDir * .5f;
                    var downPos = mainCharacterCenterPos + -upDir * .2f;
                    if (Physics.Raycast(upPos, upDir, out var upFloor, float.MaxValue, floorLayerMask))
                    {
                        if (lastDownFloor == upFloor.transform)
                        {
                            lastUpFloor = upFloor.transform;
                            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
                            mainCharacter.moveVerDir = Vector3.zero;
                            passVector2 = Vector2.zero;
                            passedWallCount++;
                            
                            if (Physics.Raycast(downPos, -upDir, out var downFloor, float.MaxValue, floorLayerMask))
                            {
                                if (lastDownFloor != downFloor.transform)
                                {
                                    lastDownFloor = downFloor.transform;
                                }
                                else
                                {
                                    Debug.LogWarning("오류 코드 고쳐야됨");
                                }
                            }
                            else
                            {
                                lastDownFloor = null;
                            }
                            break;
                        }
                    }
                }
                else if (passVector2 == Vector2.right || passVector2 == Vector2.left)
                {
                    var fwdPos = mainCharacterCenterPos + fwdDir * .3f;
                    var bwdPos = mainCharacterCenterPos + -fwdDir * .3f;
                    
                    if (Physics.Raycast(bwdPos, -fwdDir, out var backWall, float.MaxValue, wallLayerMask))
                    {
                        if (lastForward == backWall.transform)
                        {
                            lastBack = backWall.transform;
                            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
                            mainCharacter.moveVerDir = Vector3.zero;
                            passVector2 = Vector2.zero;
                            passedWallCount++;

                            if (Physics.Raycast(fwdPos, fwdDir, out var forwardWall, float.MaxValue, wallLayerMask))
                            {
                                if (lastForward != forwardWall.transform)
                                {
                                    lastForward = forwardWall.transform;
                                }
                                else
                                {
                                    Debug.LogWarning("오류 코드 고쳐야됨");
                                }
                            }
                            else
                            {
                                lastForward = null;
                            }
                            break;
                        }
                        if (lastBack == backWall.transform)
                        {
                            lastForward = backWall.transform;
                            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
                            mainCharacter.moveVerDir = Vector3.zero;
                            passVector2 = Vector2.zero;
                            passedWallCount++;

                            if (Physics.Raycast(bwdPos, -fwdDir, out var forwardWall, float.MaxValue, wallLayerMask))
                            {
                                if (lastBack != forwardWall.transform)
                                {
                                    lastBack = forwardWall.transform;
                                }
                                else
                                {
                                    Debug.LogWarning("오류 코드 고쳐야됨");
                                }
                            }
                            else
                            {
                                lastBack = null;
                            }
                            break;
                        }
                    }
                }

                yield return waitForFixedUpdate;
            }
            
            if (passedWallCount < maxWallCount)
            {
                abilityText.text = passedWallCount.ToString();
            }
            else
            {
                AbilityCooldown();
            }
        }
        
        private void Dash()
        {
            if (passVector2 != Vector2.zero || !IsUsingAbilityTimer)
            {
                return;
            }

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var joyStick = JoystickController.instance.joystick;

            var mainCharacterCenterPos = mainCharacter.CharacterController.center + mainCharacter.transform.position;
            
            var fwdDir = mainCharacter.transform.forward;
            var upDir = mainCharacter.transform.up;
            
            var fwdPos = mainCharacterCenterPos + fwdDir * .3f;
            var upPos = mainCharacterCenterPos + upDir * 0.5f;
            var downPos = mainCharacterCenterPos + -upDir * 0.2f;
            
            var wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
            var floorLayerMask = 1 << LayerMask.NameToLayer("Floor");
            
            var isForwardRaycast = Physics.Raycast(fwdPos, fwdDir, out var wallFwdFace, float.MaxValue, wallLayerMask);
            
            if (isForwardRaycast && joyStick.Horizontal < -.7f && Vector3.Distance(mainCharacterCenterPos, wallFwdFace.point) < .5f)
            {
                passVector2 = Vector2.left;
                StartCoroutine(CheckPass());
            }
            else if (isForwardRaycast && joyStick.Horizontal > .7f && Vector3.Distance(mainCharacterCenterPos, wallFwdFace.point) < .5f)
            {
                passVector2 = Vector2.right;
                StartCoroutine(CheckPass());
            }
            else if (joyStick.Vertical < -.7f && Physics.Raycast(downPos, -upDir, float.MaxValue, floorLayerMask))
            {
                passVector2 = Vector2.down;
                StartCoroutine(CheckPass());
            }
            else if (joyStick.Vertical > .7f && Physics.Raycast(upPos, upDir, float.MaxValue, floorLayerMask))
            {
                passVector2 = Vector2.up;
                StartCoroutine(CheckPass());
            }
        }

        private void PassDoor()
        {
            if (cooldown > 0f || IsPassing || !ObjectClicker.Instance.TouchDisplay(out RaycastHit[] hits))
            {
                return;
            }

            foreach (var hit in hits)
            {
                var door = hit.collider.GetComponent<OutlineClickObj>();
                if (!door || !door.IsClickEnable || isHiding && door != hidingDoor)
                {
                    continue;
                }

                var target = hit.collider.transform.position;
                if (!isHiding)
                {
                    JoystickController.instance.StopSaveLoadJoyStick(true);
                    StartCoroutine(Hide(target, door));
                    if (abilityTimerCoroutine != null)
                    {
                        StopCoroutine(abilityTimerCoroutine);
                        abilityText.text = "";
                        IsUsingAbilityTimer = false;
                        abilityButton.image.fillAmount = 0f;
                        abilityCooldownImage.fillAmount = 1f;
                        passVector2 = Vector2.zero;
                    }
                }
                else
                {
                    JoystickController.instance.StopSaveLoadJoyStick(false);
                    StartCoroutine(Hide(target, hidingDoor));
                    AbilityCooldown();
                }
            }
        }

        private IEnumerator Hide(Vector3 targetPos, OutlineClickObj door)
        {
            isHiding = !isHiding;

            if (isHiding)
            {
                var canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = .6f;
                canvasGroup.interactable = false;
            }

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            door.IsClickEnable = false;
            IsPassing = true;

            Quaternion rotation;
            if (!isHiding)
            {
                rotation = Quaternion.Euler(0, 180, 0);
                targetPos.z = originPosZ;
                Debug.Log("뒤에서 앞으로");
                hidingDoor = null;
            }
            else
            {
                originPosZ = mainCharacter.transform.position.z;
                rotation = Quaternion.Euler(0, 0, 0);
                targetPos.z += 2f;
                Debug.Log("앞에서 뒤로");
                hidingDoor = door;
            }

            mainCharacter.PickUpCharacter();
            mainCharacter.transform.rotation = rotation;

            mainCharacter.Animator.SetFloat(Speed, 1f);

            var t = 0f;
            var startPos = mainCharacter.transform.position;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime * hideSpeed;
                mainCharacter.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return waitForFixedUpdate;
            }

            mainCharacter.Animator.SetFloat(Speed, 0f);
            mainCharacter.PutDownCharacter();

            door.IsClickEnable = true;
            IsPassing = false;

            if (!isHiding)
            {
                var canvasGroup = GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
            }
        }
    }
}