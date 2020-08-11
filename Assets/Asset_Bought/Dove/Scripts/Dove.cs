using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dove : MonoBehaviour
{
    private Animator dove;
    public GameObject MainCamera;


	void Start ()
    {
        dove = GetComponent<Animator>();
	}
	
	void Update ()
    {
        if (dove.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        {
            dove.SetBool("takeoff", false);
            dove.SetBool("landing", false);
            dove.SetBool("fly", false);
            dove.SetBool("falling", false);
        }
        if ((Input.GetKeyUp(KeyCode.W))||(Input.GetKeyUp(KeyCode.A))||
            (Input.GetKeyUp(KeyCode.D))||(Input.GetKeyUp(KeyCode.F))||(Input.GetKeyUp(KeyCode.E))||(Input.GetKeyUp(KeyCode.R)))
        {
            dove.SetBool("idle", true);
            dove.SetBool("fly", true);
            dove.SetBool("walk", false);
            dove.SetBool("turnleft", false);
            dove.SetBool("turnright", false);
            dove.SetBool("flyleft", false);
            dove.SetBool("flyright", false);
            dove.SetBool("falling", false);
            dove.SetBool("eat", false);
            dove.SetBool("preen", false);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            dove.SetBool("walk", true);
            dove.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dove.SetBool("takeoff", true);
            dove.SetBool("idle", false);
            dove.SetBool("falling", false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dove.SetBool("landing", true);
            dove.SetBool("fly", false);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            dove.SetBool("turnleft", true);
            dove.SetBool("flyleft", true);
            dove.SetBool("walk", false);
            dove.SetBool("turnright", false);
            dove.SetBool("flyright", false);
            dove.SetBool("idle", false);
            dove.SetBool("fly", false);
            dove.SetBool("falling", false);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            dove.SetBool("turnright", true);
            dove.SetBool("flyright", true);
            dove.SetBool("turnleft", false);
            dove.SetBool("flyleft", false);
            dove.SetBool("walk", false);
            dove.SetBool("idle", false);
            dove.SetBool("fly", false);
            dove.SetBool("falling", false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            dove.SetBool("falling", true);
            dove.SetBool("fly", false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            dove.SetBool("eat", true);
            dove.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            dove.SetBool("idle", false);
            dove.SetBool("preen", true);
        }
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            MainCamera.GetComponent<CameraFollow>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.RightControl))
        {
            MainCamera.GetComponent<CameraFollow>().enabled = true;
        }
	}
}
