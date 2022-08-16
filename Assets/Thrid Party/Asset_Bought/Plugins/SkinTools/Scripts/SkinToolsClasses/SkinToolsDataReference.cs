using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{

	public partial class SkinToolsData : MonoBehaviour {
		[System.Serializable]
		public class ReferenceBone{
			public Transform Tr;
			public bool Excluded;
		}

	 	public SkinnedMeshRenderer ReferenceSMR;

		public bool ReferenceMeshIsValid{
			get{
				return ReferenceSMR != null && ReferenceSMR.sharedMesh != null;
			}
		}

		[System.Serializable]
		public class ReferenceSMRBonesClass{
	 		public bool IsReady = false;
			public List<ReferenceBone> items = new List<ReferenceBone>();

			public int Count{
				get{
					return items.Count;
				}
			} 	 	

			public ReferenceBone this [int idx] {
				get{
					return items[idx];
				}
			}

			public ReferenceBone this [Transform tr] {
				get{
					for(int i = 0; i<items.Count; i++){
						if(items[i].Tr == tr){
							return items[i];
						}
					}
					return null;
				}
			}

			public static ReferenceSMRBonesClass ConstructNew( SkinnedMeshRenderer smr ){
				if(smr == null){
					return null;
				}
				if(smr.sharedMesh == null){
					return null;
				}
				ReferenceSMRBonesClass result = new ReferenceSMRBonesClass();
				result.items = new List<ReferenceBone>(); 
				Transform[] bones = smr.bones;
				for(int i = 0; i<bones.Length; i++){
					ReferenceBone newItem = new ReferenceBone();
					newItem.Tr = bones[i];
					result.items.Add(newItem);
				}
				result.IsReady = true;
	 			return result;	
	 		}
	  	}

	  	[System.Serializable]
	  	public sealed class ReferenceSMRGeometryClass{

	  		[System.Serializable]
			public sealed class Triangle{
				public Vector3[] Corners;
				public Vector3 Center;
				public bool Masked;
			}

			public bool IsValid = false;
			public Triangle[] triangles;
			  

			public static ReferenceSMRGeometryClass ConstructNew (SkinnedMeshRenderer referenceSmr){
				if(referenceSmr == null){
					return null;
				}

				if(referenceSmr.sharedMesh == null){
					return null;
				}

				Mesh snapshot = new Mesh();
				referenceSmr.BakeMesh( snapshot );
				Vector3[] localVertices = snapshot.vertices;
				Vector3[] worldVertices = new Vector3[localVertices.Length];
				Matrix4x4 ltw = referenceSmr.transform.localToWorldMatrix;

				for(int v = 0; v<localVertices.Length; v++){
					worldVertices[v] = ltw.MultiplyPoint3x4(localVertices[v]);
				}

				int trisCount = snapshot.triangles.Length/3;
				int[] tindeces = snapshot.triangles;
				Triangle[] triangles = new Triangle[trisCount];
				float tDiv = 1f/3f;

				for(int i = 0; i<triangles.Length; i++){
					triangles[i] = new Triangle();
					triangles[i].Corners = new Vector3[3];
					int _a = tindeces[(i*3)];
					int _b = tindeces[(i*3)+1];
					int _c = tindeces[(i*3)+2];
					triangles[i].Corners[0] = worldVertices[_a];
					triangles[i].Corners[1] = worldVertices[_b];
					triangles[i].Corners[2] = worldVertices[_c];
					triangles[i].Center = triangles[i].Corners[0]*tDiv + triangles[i].Corners[1]*tDiv + triangles[i].Corners[2]*tDiv;
				}

				ReferenceSMRGeometryClass result = new ReferenceSMRGeometryClass();
				result.triangles = triangles;
				result.IsValid = true;
				//Debug.LogFormat( "reference smr geometry created {0} triangles", result.triangles.Length );
				return result;
	 		}

	 		public bool[] ExcludedTris{
	 			get{
	 				bool[] result = new bool[ triangles.Length ];
	 				for(int i = 0; i<triangles.Length; i++){
	 					result[i] = triangles[i].Masked;
	 				}
	 				return result;
	 			}
	 		}

			public void PaintMask(bool sign, Ray r, float brushRadius){
				Matrix4x4 rTM = Matrix4x4.LookAt( r.origin, r.GetPoint(1), Vector3.up).inverse;
				for(int t = 0; t<triangles.Length; t++){
					Vector3 lp = rTM.MultiplyPoint3x4( triangles[t].Center );
					if( Mathf.Abs( lp.x) < brushRadius && Mathf.Abs( lp.y) < brushRadius ){
						float d = ((Vector2)(lp)).magnitude;
						if(d<brushRadius){
							triangles[t].Masked = sign;
						}
					}
				}
			}
	  	}
	 
	 	public ReferenceSMRBonesClass ReferenceSMRBones;
	    public ReferenceSMRGeometryClass ReferenceSMRGeometry;
 
	}
}
