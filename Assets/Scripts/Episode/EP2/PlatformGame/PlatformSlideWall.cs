using System.Collections;
using UnityEngine;
using Utility.Character;
using Utility.Core;

namespace Episode.EP2.PlatformGame
{
    public class PlatformSlideWall : Platform
    {
        public Transform obstacleTransform;
        public float speed;
        private const float sec = 1.417f;

        protected override void PressButton()
        {
            StartCoroutine(FramePerParameter());
            miniGameManager.ActiveButton(false);
        }

        private IEnumerator FramePerParameter()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
            mainCharacter.PickUpCharacter();

            mainCharacter.RotateCharacter2D(-1f);

            mainCharacter.CharacterAnimator.SetTrigger("Slide");
            var startPos = mainCharacter.transform.position;

            var t = 0f;
            var characterController = mainCharacter.GetComponent<CharacterController>();
            while (t <= sec)
            {
                var direction = (obstacleTransform.position - startPos);
                characterController.Move(direction * speed * Time.fixedDeltaTime);

                t += Time.fixedDeltaTime;

                yield return waitForFixedUpdate;
            }

            mainCharacter.PutDownCharacter();
        }
    }
}