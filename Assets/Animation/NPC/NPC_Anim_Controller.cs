﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Anim_Controller : MonoBehaviour
{
    //지금당장은 Citizen에 최적화됨.

    /*추가해야되는 변수들
    citizen>float DirX,bool Nearby_Person
    Student>float DirX, bool Talking(인터렉션할때 활성화되는 것)
    Enemy>Bool Speedup, bool Talking
    Special>Talking,bool Story_Interaction
     */

    public enum NPC_p : int{ Citizen=3, Student=4, Special=2, Enemy=2};
    public NPC_p enum_Pattern;
    int num_Pattern;//애니메이션 패턴개수

    public int num_Random;//애니메이션 랜덤개수
    public Animator anim; //할당 애니메이터
    private int _Pattern;
    private int _Random=0;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>(); //애니메이터 접근
        num_Pattern = (int)enum_Pattern;
        Pattern_Setting();
    }
    // Update is called once per frame
    void Update()
    {
        NPC_Animation(_Pattern, _Random);
    }

    public void NPC_Animation(int Pattern_, int Random_) 
    {
        anim.SetInteger("Pattern", Pattern_);
        anim.SetInteger("Random", Random_);
    }

    void Pattern_Setting() 
    {
        _Pattern = Random.Range(0, num_Pattern);
        Random_Setting();
        Invoke("Pattern_Setting", 2);
    }

    void Random_Setting() 
    { 
        _Random= Random.Range(0, num_Random);
        Invoke("Random_Setting", 2);
    }

}
