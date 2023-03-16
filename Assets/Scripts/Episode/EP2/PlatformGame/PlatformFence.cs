using System.Collections;
using UnityEngine;
using Utility.Core;

namespace Episode.EP2.PlatformGame
{
    public class PlatformFence : Platform
    {
        public Transform obstacleTransform;
        public float speed;
        public AnimationCurve jumpCurve;
        public float sec;
        
        protected override void PressButton()
        {
            StartCoroutine(FramePerParameter());
            gameManager.ActiveButton(false);
        }

        private IEnumerator FramePerParameter()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var mainCharacter = DataController.Instance.GetCharacter(Data.CustomEnum.Character.Main);
            mainCharacter.PickUpCharacter();

            mainCharacter.RotateCharacter2D(-1f);

            mainCharacter.CharacterAnimator.SetBool("2DSide", false);
            mainCharacter.CharacterAnimator.SetTrigger("Jump_fence");
            var startPos = mainCharacter.transform.position;

            var t = 0f;
            var characterController = mainCharacter.GetComponent<CharacterController>();
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime / sec;

                var curvepercent = jumpCurve.Evaluate(t);
                var destPosition = Vector3.LerpUnclamped(startPos, obstacleTransform.position, t);
                destPosition.y = Mathf.LerpUnclamped(startPos.y, obstacleTransform.position.y, curvepercent);

                characterController.transform.position = destPosition;

                yield return waitForFixedUpdate;
            }

            mainCharacter.CharacterAnimator.SetBool("2DSide", true);
            mainCharacter.PutDownCharacter();

        }
    }
}