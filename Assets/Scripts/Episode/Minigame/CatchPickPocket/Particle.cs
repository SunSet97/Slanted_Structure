using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;

public class Particle : MonoBehaviour
{
    private ParticleSystem ps;
    public CharacterManager rau;
    public Vector3 offset;
    bool check;
    public MiniGameManager miniGameManager;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
        //offset = transform.position + rau.transform.position;
        offset = new Vector3(0, 1.5f, 10);
        check = true;
    }

    
    void Update()
    {
        //transform.position = new Vector3(rau.transform.position.x, rau.transform.position.y + 1.5f, rau.transform.position.z + 10);
        transform.position = rau.transform.position + offset;

        if (miniGameManager.conflict)
        {
            check = false;
            ps.Stop();
        } else if (!check && !miniGameManager.conflict) {
            ps.Play();
            check = true;
        }
        
    }
}
