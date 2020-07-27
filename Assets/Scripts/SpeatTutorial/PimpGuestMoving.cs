using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PimpGuestMoving : MonoBehaviour
{
    public float speed;
    public float changeDirectionCycle;
    CharacterController npc;
    int nextDirection;
    float rotation;

    void Start()
    {
        npc = GetComponent<CharacterController>();
        rotation = gameObject.transform.rotation.y;
        Invoke("Think", 0);
    }

    void Update()
    {
        if (!npc.isGrounded)
        {
            npc.Move(transform.up * -1); // 중력
        }
        else
        {
            npc.Move(Vector3.right * nextDirection * speed * Time.deltaTime);
        }

    }


    void Think() // 방향 설정.
    {
        nextDirection = (int)Random.Range(-1, 2);

        if (nextDirection == 1)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        else if (nextDirection == -1)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }

        Invoke("Think", changeDirectionCycle);

    }
}
