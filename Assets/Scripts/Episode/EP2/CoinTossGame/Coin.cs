using System;
using System.Collections;
using UnityEngine;

namespace Episode.EP2.CoinTossGame
{
    public class Coin : MonoBehaviour
    {
        private Action checkAction;
        
        private Action<bool> endAction;
        
        public void Init(Vector3 dir, float force, float timer, Action action1, Action<bool> action2)
        {
            checkAction = action1;
            endAction = action2;
            
            var myRigidbody = GetComponent<Rigidbody>();
            myRigidbody.AddForce(dir * force, ForceMode.VelocityChange);

            StartCoroutine(WaitStop(timer));
        }

        private IEnumerator WaitStop(float timer)
        {
            yield return new WaitForSeconds(timer);
            Check(false);
        }

        private void Check(bool isClear)
        {
            checkAction?.Invoke();

            endAction?.Invoke(isClear);
        }

        private void OnTriggerEnter(Collider other)
        {
            // 바닥인가 분수인가

            StopAllCoroutines();
            
            Check(true);
        }
    }
}