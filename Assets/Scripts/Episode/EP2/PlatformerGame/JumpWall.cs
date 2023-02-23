using System.Collections;
using CommonScript;
using UnityEngine;
using Utility.Core;

namespace Episode.EP2.PlatformerGame
{
    public class JumpWall : JumpInTotal
    {
        public Transform firstPos;
        public Transform secondPos;
        public Transform thirdPos;
        public float speed;
        public AnimationCurve jumpCurve;
        public float sec1;
        public float sec2;
        protected override void ButtonPressed()
        {
            StartCoroutine(FrameForParameter());
            gameManager.ActiveButton(false);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
        }

        private IEnumerator FrameForParameter()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            CharacterManager platformerCharacter = DataController.Instance.GetCharacter(Data.CustomEnum.Character.Main);
            var characterController = platformerCharacter.GetComponent<CharacterController>();
            platformerCharacter.UseGravity = false;
            platformerCharacter.PickUpCharacter();

            platformerCharacter.RotateCharacter2D(-1f);

            var moveY = (firstPos.transform.position - platformerCharacter.transform.position).normalized;
            characterController.Move(moveY);


            platformerCharacter.CharacterAnimator.SetTrigger("Climb");
            float t = 0f;
            var direction = (secondPos.position - firstPos.position).normalized;
            while (t <= 1f)
            {
                yield return waitForFixedUpdate;
                characterController.Move(direction * Time.fixedDeltaTime * speed);

                t += Time.fixedDeltaTime / sec1;
            }

            var startPosition = platformerCharacter.transform.position;
            platformerCharacter.CharacterAnimator.SetTrigger("Jump_fence");
            t = 0f;
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime / sec2;

                float curvepercent = jumpCurve.Evaluate(t);
                var destPosition = Vector3.LerpUnclamped(startPosition, thirdPos.position, t) - platformerCharacter.transform.position;
                destPosition.y = Mathf.LerpUnclamped(startPosition.y, thirdPos.position.y, curvepercent) - platformerCharacter.transform.position.y;

                characterController.Move(destPosition * Time.fixedDeltaTime);

                yield return waitForFixedUpdate;
            }

            platformerCharacter.UseGravity = true;
            platformerCharacter.CharacterAnimator.SetBool("2DSide", false);
            platformerCharacter.PutDownCharacter();

        }
    }
}