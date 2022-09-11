using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpFence : MonoBehaviour
{
    public PlatformerGame platformer;
    //  public GameObject[] outlines;
    public PlatformerGame.ButtonType moveType;
    
    //가까워졌을 때
    private void OnTriggerEnter(Collider other)
    {
        //버튼 activeButton() 
        //platformer.ActiveButton();

        //platformer.btnType = moveType;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
