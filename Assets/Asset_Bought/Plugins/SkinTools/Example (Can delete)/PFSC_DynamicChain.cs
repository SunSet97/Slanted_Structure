using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

public class PFSC_DynamicChain : MonoBehaviour {

	[System.Serializable]
	public class Node{
 		public Matrix4x4 lRigidTM;
		public Matrix4x4 WorldTM;
		public float Length;
		public Quaternion rot;
		public Vector3 Effector;
 		public float FreedomRadius = 0.1f;
 		public Vector3 SPoint;
		public Matrix4x4 SkinlTM;
		public Vector3 wMoveDir;
	}
 

	public Transform Root;
	public Transform End;


	public Node[] Nodes;
	public float AverageLength;
 
	public Transform[] Hierarchy;
	public float Tension = 2;
	public float Dampening = 2;
	public float RotSpeed = 2;
	int firstFrame = 0;
 
	public float FR_From = 0.02f;
	public float FR_To = 0.06f;
	public float FR_Visiblity = 0.5f;
	public bool RootStrict;
 

 	public void StartThis (){
		Matrix4x4 prevTM =  Root.localToWorldMatrix;
		for(int n = 0; n<Nodes.Length; n++){
			Node _n = Nodes[n];
			_n.WorldTM = prevTM * _n.lRigidTM;
			_n.Effector = _n.WorldTM.MultiplyPoint3x4( new Vector3(0,0,_n.Length));
			prevTM = _n.WorldTM;
			 
		}
 
 	}

	void LateUpdate(){
 		if(Application.isPlaying){
			if(firstFrame < 3){
				StartThis();
				firstFrame ++;
			}
			Matrix4x4 prevTM =  Root.localToWorldMatrix;
			Nodes[0].WorldTM = Hierarchy[0].localToWorldMatrix;
			for(int n = 0; n<Nodes.Length; n++){
				Node _n = Nodes[n];
				_n.WorldTM = prevTM * Nodes[n].lRigidTM;
				Vector3 worldTarget = _n.WorldTM.MultiplyPoint3x4( new Vector3(0,0,_n.Length)) ;
				Vector3 toTarget =  (worldTarget - _n.Effector)   ;
				_n.wMoveDir = Vector3.Lerp( _n.wMoveDir, toTarget,   Time.deltaTime * Dampening );
				_n.Effector += _n.wMoveDir * Time.deltaTime * Tension   ;
				Vector3 wSP = RootStrict? Root.localToWorldMatrix.MultiplyPoint3x4 ( _n.SPoint ) : worldTarget;
				Vector3 effectorDelta = _n.Effector -  wSP;
				effectorDelta = effectorDelta.normalized * Mathf.Clamp( effectorDelta.magnitude, 0, _n.FreedomRadius );
				_n.Effector  =  wSP + effectorDelta;
				Vector3 effectorLocal = _n.WorldTM.inverse.MultiplyPoint3x4( _n.Effector );
				Quaternion rawJerk = Quaternion.FromToRotation( Vector3.forward, effectorLocal );
				_n.rot = Quaternion.Slerp( _n.rot , rawJerk, RotSpeed*Time.deltaTime);
				Vector3 zAxis =  (Vector3)_n.WorldTM.GetColumn(3) + _n.WorldTM.MultiplyVector( _n.rot * Vector3.forward );
				_n.WorldTM = Matrix4x4.LookAt( _n.WorldTM.GetColumn(3), zAxis, _n.WorldTM.GetColumn(1));
				prevTM = _n.WorldTM;
				Matrix4x4 btm =    _n.WorldTM * _n.SkinlTM;
				Hierarchy[n].position = btm.GetColumn(3);
				Hierarchy[n].rotation = Quaternion.LookRotation( btm.GetColumn(2), btm.GetColumn(1));
			}
 
		}  
	}

 
}
 
