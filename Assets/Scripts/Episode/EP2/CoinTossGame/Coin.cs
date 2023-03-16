using System;
using UnityEngine;

namespace Episode.EP2.CoinTossGame
{
    public class Coin : MonoBehaviour
    {
        [NonSerialized] private CoinTossGameManager coinTossGameManager;
        public void Init(CoinTossGameManager gameManager, Vector3 dir, float force)
        {
            coinTossGameManager = gameManager;
            var myRigidbody = GetComponent<Rigidbody>();
            myRigidbody.AddForce(dir * force);
            // myRigidbody.AddTorque(dir * force, ForceMode.VelocityChange);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name != "water_upper")
            {
                return;
            }
            // else if(tossButton >=5) EndPlay()
            // else 몇초후? Button.SetActive(true)
            // coinTossGameManager.EndPlay();
        }
    }
}