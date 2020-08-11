﻿using UnityEngine;
using System.Collections;

// /-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\
//
// 					Original version of MouseLook script found on Unity3D Wiki
//									modified by Ripcord Development
//										     MouseLook.cs
//										 info@ripcorddev.com
//
// \-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/-\-/

public class MouseLook : MonoBehaviour {

	Vector2 _mouseAbsolute;
	Vector2 _smoothMouse;
	
	public Vector2 clampInDegrees = new Vector2(360.0f, 180.0f);

	public CursorLockMode cursorMode;	

	public Vector2 sensitivity = new Vector2(2.0f, 2.0f);
	public Vector2 smoothing = new Vector2(3.0f, 3.0f);
	public Vector2 targetDirection;


	void Start() {
		
		targetDirection = transform.localRotation.eulerAngles;						// Set target direction to the camera's initial orientation.
	}
	
	void Update() {

		// Ensure the cursor is always locked when set
		Cursor.lockState = cursorMode;
		Cursor.visible = (CursorLockMode.Locked != cursorMode);

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.lockState = cursorMode = CursorLockMode.None;
		}

		// Allow the script to clamp based on a desired target value.
		var targetOrientation = Quaternion.Euler(targetDirection);
		
		// Get raw mouse input for a cleaner reading on more sensitive mice.
		var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		
		// Scale input against the sensitivity setting and multiply that against the smoothing value.
		mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
		
		// Interpolate mouse movement over time to apply smoothing delta.
		_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
		_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
		
		// Find the absolute mouse movement value from point zero.
		_mouseAbsolute += _smoothMouse;
		
		// Clamp and apply the local x value first, so as not to be affected by world transforms.
		if (clampInDegrees.x < 360) {
			_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);
		}

		var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
		transform.localRotation = xRotation;

		// Then clamp and apply the global y value.
		if (clampInDegrees.y < 360) {
			_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);
		}

		transform.localRotation *= targetOrientation;

		var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
		transform.localRotation *= yRotation;
	}
}
