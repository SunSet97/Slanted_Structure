using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_phw : MonoBehaviour
{
    ParticleSystem ps;
    public Rau_phw rau;
    Vector3 offset;
    float speed;
    bool check;
    

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
        offset = transform.position + rau.transform.position;
        check = true;
    }

    
    void Update()
    {
        transform.position = new Vector3(rau.transform.position.x, rau.transform.position.y + 1.5f, rau.transform.position.z + 10);

        if (rau.conflictWithCrowd)
        {
            check = false;
            Debug.Log("파티클 포즈");
            ps.Pause();
        } else if (!check && !rau.conflictWithCrowd) {
            ps.Play();
            check = true;
        }
        
    }
}
