﻿using System.Collections;
using CommonScript;
using UnityEngine;
using Utility.Core;

namespace Episode.EP2.PlatformerGame
{
    public class SlideWall : JumpInTotal
    {
        public Transform obstacleTransform;
        public float speed;
        private const float sec = 1.417f;

        protected override void ButtonPressed()
        {
            StartCoroutine(FramePerParameter());
            gameManager.ActiveButton(false);
            isActivated = true;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
        }

        private IEnumerator FramePerParameter()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            CharacterManager platformerCharacter = DataController.Instance.GetCharacter(Data.CustomEnum.Character.Main);
            platformerCharacter.PickUpCharacter();

            platformerCharacter.RotateCharacter2D(-1f);

            platformerCharacter.CharacterAnimator.SetTrigger("Slide");
            Vector3 startposition = platformerCharacter.transform.position;

            float t = 0f;

            while (t <= sec)
            {
                var direction = (obstacleTransform.position - startposition);
                var characterController = platformerCharacter.GetComponent<CharacterController>();
                characterController.Move(direction * speed * Time.fixedDeltaTime);

                t += Time.fixedDeltaTime;

                yield return waitForFixedUpdate;
            }

            platformerCharacter.PutDownCharacter();
        }
    }
}