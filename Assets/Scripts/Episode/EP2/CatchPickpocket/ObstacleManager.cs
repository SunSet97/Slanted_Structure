using UnityEngine;

namespace Episode.EP2.CatchPickpocket
{
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        
        public void Initialize(float speed)
        {
            animators = GetComponentsInChildren<Animator>(true);
            
            foreach (var animator in animators)
            {
                animator.enabled = true;
                animator.gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
                animator.speed = speed;
            }
        }
    }
}
