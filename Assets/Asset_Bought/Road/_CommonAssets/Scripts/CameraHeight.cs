using UnityEngine;
using System.Collections;

// /-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\
//
// 							City Streets 1.5, Copyright © 2017, Ripcord Development
//										   CameraHeight.cs
//										 info@ripcorddev.com
//
// \-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/

//ABOUT - This script is for demonstration purposes only.  It is not required component of the CityStreets package.  
//		- This script adjusts the height of the camera between a specified range when certain keys are held down.

public class CameraHeight : MonoBehaviour {

	public float cameraHeightMin;
	public float cameraHeightMax;
	public float moveSpeed;


	void Update () {
	
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			if (transform.position.y < cameraHeightMax) {
				transform.position += Vector3.up * moveSpeed * Time.deltaTime;
			}
		}

		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			if (transform.position.y > cameraHeightMin) {
				transform.position += Vector3.down * moveSpeed * Time.deltaTime;
			}
		}
	}
}
