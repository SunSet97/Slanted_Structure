using System.Collections;
using UnityEngine;
using Utility.Character;
using Utility.Core;

namespace Episode.EP2.PlatformGame
{
    public class PlatformWall : Platform
    {
        public Transform firstPos;
        public Transform secondPos;
        public Transform thirdPos;
        public float speed;
        public AnimationCurve jumpCurve;
        public float sec1;
        public float sec2;

        protected override void PressButton()
        {
            StartCoroutine(FrameForParameter());
            miniGameManager.ActiveButton(false);
        }

        private IEnumerator FrameForParameter()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
            var characterController = mainCharacter.GetComponent<CharacterController>();
            mainCharacter.UseGravity = false;
            mainCharacter.PickUpCharacter();

            mainCharacter.RotateCharacter2D(-1f);

            var moveY = (firstPos.transform.position - mainCharacter.transform.position).normalized;
            characterController.Move(moveY);


            mainCharacter.CharacterAnimator.SetTrigger("Climb");
            var t = 0f;
            var direction = (secondPos.position - firstPos.position).normalized;
            while (t <= 1f)
            {
                yield return waitForFixedUpdate;
                characterController.Move(direction * Time.fixedDeltaTime * speed);

                t += Time.fixedDeltaTime / sec1;
            }

            var startPosition = mainCharacter.transform.position;
            mainCharacter.CharacterAnimator.SetTrigger("Jump_fence");
            t = 0f;
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime / sec2;

                var curvepercent = jumpCurve.Evaluate(t);
                var destPosition = Vector3.LerpUnclamped(startPosition, thirdPos.position, t) -
                                   mainCharacter.transform.position;
                destPosition.y = Mathf.LerpUnclamped(startPosition.y, thirdPos.position.y, curvepercent) -
                                 mainCharacter.transform.position.y;

                characterController.Move(destPosition * Time.fixedDeltaTime);

                yield return waitForFixedUpdate;
            }

            mainCharacter.UseGravity = true;
            mainCharacter.CharacterAnimator.SetBool("2DSide", false);
            mainCharacter.PutDownCharacter();
        }
    }
}