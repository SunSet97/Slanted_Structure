using System;
using System.Collections;
using UnityEngine;

namespace Episode.EP2.CoinTossGame
{
    public class Coin : MonoBehaviour
    {
        private Action<bool> endAction;
        private bool isTriggered;
        
        public void Init(Vector3 dir, float force, float timer, Action<bool> endAction)
        {
            this.endAction = endAction;

            isTriggered = false;
            var myRigidbody = GetComponent<Rigidbody>();
            myRigidbody.AddForce(dir.normalized * force, ForceMode.VelocityChange);

            StartCoroutine(WaitStop(timer));
        }

        private IEnumerator WaitStop(float timer)
        {
            yield return new WaitForSeconds(timer);
            Check(false);
        }

        private void Check(bool isClear)
        {
            endAction?.Invoke(isClear);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered)
            {
                return;
            }
            
            isTriggered = true;
            // 바닥인가 분수인가

            StopAllCoroutines();
            
            // Debug.Log($"other - {other.gameObject}");
            Check(true);
            // GetComponent<Collider>().
        }
    }
}