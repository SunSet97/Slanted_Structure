using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpObstacle : MonoBehaviour
{
    public Transform jump;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out CharacterManager character))
        {
            //character.button.onClick.AddListener()
        }
    }
}
