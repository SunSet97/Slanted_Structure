using System.Collections;
using UnityEngine;

namespace Thrid_Party.Asset_Bought.Dove.Scripts
{
    public class Dove : MonoBehaviour
    {
        public float speedTime = 0.8f;

        [SerializeField] private int randomNum;
        [SerializeField] private float speed;
        [SerializeField] private float direction;
        [SerializeField] private float stateChange = 4.0f;

        private Animator doveAnimator;

        private readonly int speedHash = Animator.StringToHash("Speed");
        private readonly int directionHash = Animator.StringToHash("Direction");
        private readonly int stateHash = Animator.StringToHash("State");

        private bool attackBool = false;

        private void Start()
        {
            doveAnimator = GetComponent<Animator>();
            doveAnimator.SetInteger(stateHash, 0);
            StartCoroutine(RandomState());
        }

        private void Update()
        {
            MoveDove();
        }

        public void MoveDove()
        {
            if (doveAnimator.GetInteger(stateHash) != 0)
            {
                return;
            }

            speed = (Mathf.Sin(Time.time * speedTime) * .6f + 1f) / 2f;
            direction = Mathf.Cos(Time.time * speedTime);
            doveAnimator.SetFloat(speedHash, speed);
            doveAnimator.SetFloat(directionHash, direction);
        }

        public void Attack()
        {
            doveAnimator.SetTrigger("Attack");
        }

        IEnumerator RandomState()
        {
            var randomTime = UnityEngine.Random.Range(1, stateChange);
            var waitForSeconds = new WaitForSeconds(randomTime);
            while (true)
            {
                randomNum = UnityEngine.Random.Range(0, 3);
                doveAnimator.SetInteger(stateHash, randomNum);

                yield return waitForSeconds;
            }
        }
    }
}
