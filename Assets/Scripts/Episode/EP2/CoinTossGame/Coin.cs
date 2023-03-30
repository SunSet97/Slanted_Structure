using System;
using System.Collections;
using UnityEngine;

namespace Episode.EP2.CoinTossGame
{
    public class Coin : MonoBehaviour
    {
        private Action checkAction;
        public void Init(Vector3 dir, float force, float timer, Action action)
        {
            checkAction = action;
            var myRigidbody = GetComponent<Rigidbody>();
            myRigidbody.AddForce(dir * force, ForceMode.VelocityChange);

            StartCoroutine(WaitStop(timer));
        }

        private IEnumerator WaitStop(float timer)
        {
            yield return new WaitForSeconds(timer);
            Check();
        }

        private void Check()
        {
            checkAction?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            // 바닥인가 분수인가
            
            StopAllCoroutines();
            
            Check();
        }
    }
}