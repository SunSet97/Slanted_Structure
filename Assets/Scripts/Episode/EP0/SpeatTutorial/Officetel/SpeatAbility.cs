using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.Character;
using Utility.Core;
using Utility.Interaction.Click;

namespace Episode.EP0.SpeatTutorial.Officetel
{
    public class SpeatAbility : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("-UI")] [SerializeField] private Button abilityButton;
        public Image abilityCooldownImage;
        public Text abilityText;

        [Header("-Variable")] [SerializeField] private float setDuration = 4f;
        [SerializeField] private float setCooldown = 3f;
        [SerializeField] private int maxWallCount = 2;

        [SerializeField] private float hideSpeed = 0.5f;

        [SerializeField] private float passTime = .7f;

        [NonSerialized] public bool IsUsingAbilityTimer;
        [NonSerialized] public bool IsPassing;
        [NonSerialized] public int Floor = 5;
#pragma warning restore 0649

        private float abilityDuration;
        private float cooldown;
        private int passedWallCount;
        private Vector2 passVector2;
        private Coroutine abilityTimerCoroutine;

        private bool isHiding;
        private OutlineClickObj hidingDoor;
        private float originPosZ;
        

        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Start()
        {
            ObjectClicker.Instance.IsCustomUse = true;
            abilityButton.onClick.AddListener(UseAbility);
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
                if (!isHiding)
                {
                    Dash();
                }

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
            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
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

        private IEnumerator CheckPass(Vector3 targetPos)
        {
            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);

            mainCharacter.PickUpCharacter();

            Debug.Log("목표: " + targetPos);

            var waitForFixedUpdate = new WaitForFixedUpdate();
            var t = 0f;
            var delta = (targetPos - mainCharacter.transform.position) * Time.fixedDeltaTime / passTime;
            while (t <= 1f)
            {
                mainCharacter.transform.position += delta;

                t += Time.fixedDeltaTime / passTime;
                yield return waitForFixedUpdate;
            }

            mainCharacter.PutDownCharacter();
            passedWallCount++;
            passVector2 = Vector2.zero;

            if (passedWallCount < maxWallCount)
            {
                abilityText.text = (maxWallCount - passedWallCount).ToString();
            }
            else
            {
                AbilityCooldown();
            }
        }

        private void Dash()
        {
            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);

            if (passVector2 != Vector2.zero || !IsUsingAbilityTimer)
            {
                return;
            }

            var joyStick = JoystickController.Instance.Joystick;

            var fwdDir = mainCharacter.transform.forward;
            var upDir = mainCharacter.transform.up;

            var wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
            var floorLayerMask = 1 << LayerMask.NameToLayer("Floor");

            var isForwardRaycast = Physics.Raycast(mainCharacter.transform.position, fwdDir,
                out var forwardWall, float.MaxValue,
                wallLayerMask);


            if (isForwardRaycast && joyStick.Horizontal < -.7f &&
                Vector3.Distance(mainCharacter.transform.position, forwardWall.point) < .5f)
            {
                passVector2 = Vector2.left;
                StartCoroutine(CheckPass(forwardWall.point + fwdDir));
            }
            else if (isForwardRaycast && joyStick.Horizontal > .7f &&
                     Vector3.Distance(mainCharacter.transform.position, forwardWall.point) < .5f)
            {
                passVector2 = Vector2.right;
                StartCoroutine(CheckPass(forwardWall.point + fwdDir));
            }
            else if (joyStick.Vertical > .7f && Physics.Raycast(mainCharacter.transform.position, upDir,
                         out var upFloor, float.MaxValue, floorLayerMask))
            {
                Floor += 1;
                passVector2 = Vector2.up;
                StartCoroutine(CheckPass(upFloor.point + Vector3.up));
            }
            else if (joyStick.Vertical < -.7f && Physics.Raycast(mainCharacter.transform.position, -upDir,
                         out var downFloor, float.MaxValue, floorLayerMask))
            {
                Floor -= 1;
                passVector2 = Vector2.down;
                StartCoroutine(CheckPass(downFloor.point + Vector3.down));
            }
        }

        private void PassDoor()
        {
            if (IsPassing || !ObjectClicker.Instance.TouchDisplay(out RaycastHit[] hits))
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
                    JoystickController.Instance.StopSaveLoadJoyStick(true);
                    StartCoroutine(Hide(target, door));
                }
                else
                {
                    JoystickController.Instance.StopSaveLoadJoyStick(false);
                    StartCoroutine(Hide(target, hidingDoor));
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

            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
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

            mainCharacter.CharacterAnimator.SetFloat(Speed, 1f);

            var t = 0f;
            var startPos = mainCharacter.transform.position;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime * hideSpeed;
                mainCharacter.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return waitForFixedUpdate;
            }

            mainCharacter.CharacterAnimator.SetFloat(Speed, 0f);
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