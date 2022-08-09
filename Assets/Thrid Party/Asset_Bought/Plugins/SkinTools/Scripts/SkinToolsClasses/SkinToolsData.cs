using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{
	[ExecuteInEditMode]
	public partial class SkinToolsData : MonoBehaviour {
 
		public int BakingQuality = 1;
		public string[] BakingQualityNames = new string[3]{"Draft", "Normal", "High"};

		[System.Serializable]
		public class MapBone {
			public Transform Tr;
			public Texture2D Map;
		}
		public List<MapBone> MapBones = new List<MapBone>();

		[System.Serializable]
		public class ResultBone{

			[System.Serializable]
			public class ChildInfo{
				public Transform ParentTr;
				public Transform ThisTr;
				public Vector3[] htris0 = new Vector3[3];
				public Vector3[] htris1 = new Vector3[3];
				public Vector3[] htris2 = new Vector3[3];
				public Vector3[] htris3 = new Vector3[3];
				public Vector3[] hcap = new Vector3[4];

				public ChildInfo (Transform parentTr, Transform thisTr){
					ParentTr = parentTr;
					ThisTr = thisTr;
					htris0 = new Vector3[3];
					htris1 = new Vector3[3];
					htris2 = new Vector3[3];
					htris3 = new Vector3[3];
					hcap = new Vector3[4];
				}

				public void UpdateTriangles( float size ){
					Matrix4x4 tm = Matrix4x4.LookAt( ParentTr.position, ThisTr.position, ParentTr.up );
					Vector3 v0 =  ThisTr.position;
	 				Vector3 c0 = tm.MultiplyPoint3x4( new Vector3(-1,-1,0) * size );
					Vector3 c1 = tm.MultiplyPoint3x4( new Vector3(1,-1,0) * size );
					Vector3 c2 = tm.MultiplyPoint3x4( new Vector3(1,1,0) * size );
					Vector3 c3 = tm.MultiplyPoint3x4( new Vector3(-1,1,0) * size );

					htris0[0] = v0;
					htris0[1] = c0;
					htris0[2] = c1;

					htris1[0] = v0;
					htris1[1] = c1;
					htris1[2] = c2;

					htris2[0] = v0;
					htris2[1] = c2;
					htris2[2] = c3;

					htris0[0] = v0;
					htris0[1] = c3;
					htris0[2] = c0;

					hcap[0] = c0;
					hcap[1] = c1;
					hcap[2] = c2;
					hcap[3] = c3;
				} 
			}

			public ChildInfo[] Childs; 
			public int TargetChild;
			public Transform Tr;
	 		public float[] Weights;

			public void FindTargetChild(){
				int childCount = Tr.childCount;
				if(Childs == null || Childs.Length != childCount){
					Childs = new ChildInfo[childCount];
					for(int c = 0; c<Childs.Length; c++){
						Childs[c] = new ChildInfo(Tr, Tr.GetChild(c));
					}
				}
	 		}
		}

		[SerializeField]
		private SkinnedMeshRenderer _resultSmr; 
		public SkinnedMeshRenderer resultSMR{
			get{
				if(_resultSmr == null){
					_resultSmr = GetComponent<SkinnedMeshRenderer>();
					if(_resultSmr == null){
						_resultSmr = gameObject.AddComponent<SkinnedMeshRenderer>();

					}
				}
				return _resultSmr;
			}
		}

		public int ResultPreviewMode;
		public float BlurMultiplier = 0.5f;
		public int BlurIterations = 0;

		public Mesh ResultMesh;
		public string OutputMeshPath;
		public ResultBone[] ResultBones = new ResultBone[0]; 
		public int SelectedResultBone;

		Material _PreviewWeightMat;
		public Material PreviewWeightMat{
			get{
				if(_PreviewWeightMat == null){
					_PreviewWeightMat = new Material( Shader.Find("Unlit/VertexColor"));
				}
				return _PreviewWeightMat;
			}
		}

		public void UpdateResultInfo() {
			Transform[] _bones = resultSMR.bones;
			ResultBones = new ResultBone[_bones.Length];
			BoneWeight[] weights = resultSMR.sharedMesh.boneWeights;
			for(int i = 0; i<_bones.Length; i++){
				ResultBones[i] = new ResultBone();
				ResultBones[i].Tr = _bones[i]; 
				ResultBones[i].Weights = new float[weights.Length];

				for(int w = 0; w<weights.Length; w++){
					if(weights[w].boneIndex0 == i){
						ResultBones[i].Weights[w] =  Mathf.Max( ResultBones[i].Weights[w],  weights[w].weight0);
					}
					if(weights[w].boneIndex1 == i){
						ResultBones[i].Weights[w] = Mathf.Max( ResultBones[i].Weights[w],  weights[w].weight1);
					}
					if(weights[w].boneIndex2 == i){
						ResultBones[i].Weights[w] = Mathf.Max( ResultBones[i].Weights[w],  weights[w].weight2);
					}
					if(weights[w].boneIndex3 == i){
						ResultBones[i].Weights[w] = Mathf.Max( ResultBones[i].Weights[w],  weights[w].weight3);
					}
				}
				ResultBones[i].FindTargetChild();
			}
		}

		public void ApplyResultVisual(){
			if(SelectedResultBone>=ResultBones.Length-1){
				SelectedResultBone = ResultBones.Length-1;
			}

			if(SourceGeometry == null){
				return;
			}

			if(SelectedResultBone<0){
				resultSMR.sharedMaterials = SourceGeometry.InitialMaterials;
	 			resultSMR.sharedMesh.colors = SourceGeometry.InitialVertexColors;
	 		} else {
				Color[] colors = new Color[ResultMesh.vertexCount];
				for(int i = 0; i<colors.Length; i++){
					float val = ResultBones[SelectedResultBone].Weights[i];
					colors[i] = Color.Lerp( colors[i], Color.white, val );
	 			}
				Material[] vcMatArr = new Material[SourceGeometry.InitialMaterials.Length];
				for(int i = 0; i<vcMatArr.Length; i++){
					vcMatArr[i] = PreviewWeightMat;
				}

				resultSMR.sharedMaterials = vcMatArr;
				resultSMR.sharedMesh.colors = colors;
			}

	 	}

		public void OnChangeSourceGeometry ( MeshFilter MF ){
			SourceGeometry = SourceGeometryInfoClass.ConstructNew( MF );
 	 	}

		public void OnChangeReferenceSMR( SkinnedMeshRenderer smr){
			ReferenceSMRBones = ReferenceSMRBonesClass.ConstructNew( smr );
			ReferenceSMRGeometry = ReferenceSMRGeometryClass.ConstructNew( smr );
		}


		void Update(){
 
			if( !Application.isPlaying ){
				SkinTools _st = GetComponent<SkinTools>();
				if(_st == null){
					DestroyImmediate (this);
				}
			}
		 
		}
	}
}
