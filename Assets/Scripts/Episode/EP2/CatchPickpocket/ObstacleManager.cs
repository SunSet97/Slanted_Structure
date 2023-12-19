using System;
using UnityEngine;

namespace Episode.EP2.CatchPickpocket
{
    public class ObstacleManager : MonoBehaviour
    {
        [Serializable]
        private class Obstacle
        {
            public Animator animator;
            public float realMoveSpeed;
            public float animationMoveSpeed;
        }

        [SerializeField] private Obstacle[] obstacles;

        public int index;

        public void Initialize(float speed)
        {
            foreach (var obstacle in obstacles)
            {
                obstacle.animator.enabled = true;
                obstacle.animator.gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
                obstacle.animator.speed = speed * obstacle.animationMoveSpeed;
            }
        }

        public void Move(float deltaTime)
        {
            foreach (var obstacle in obstacles)
            {
                obstacle.animator.transform.Translate(obstacle.realMoveSpeed * deltaTime *
                                                      obstacle.animator.transform.InverseTransformDirection(
                                                          obstacle.animator.transform.forward));
            }
        }
    }
}