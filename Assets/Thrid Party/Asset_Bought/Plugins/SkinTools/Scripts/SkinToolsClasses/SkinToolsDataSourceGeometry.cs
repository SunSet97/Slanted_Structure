using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{
	public partial class SkinToolsData : MonoBehaviour {

		public MeshFilter SourceGeometryMF;

		[System.Serializable]
		public class SourceGeometryInfoClass{
			Dictionary< PVert, int> posVertItemsDict = new Dictionary< PVert, int>();
			public List <PVert> posVertsList = new List<PVert>();
			public List<UVert> unityVertices = new List<UVert>();

			[System.Serializable]
			public class PVert{
				public Vector3 Value;
				public int Hashcode;
				public List<int> AdjacentTris;
				public List<int> AdjacentPV;
	 			public float[] BonesWeights;
	 			public ExtBoneWeight ebw;	

				public PVert (Vector3 pos){
					Value = pos;
					Hashcode = pos.GetHashCode();
					AdjacentTris = new List<int>();
					AdjacentPV = new List<int>();
				}

				public override bool Equals (object obj){
					PVert other = (PVert)obj;
					return  other.Hashcode == Hashcode ;
				}

				public override int GetHashCode (){
					return Hashcode; 
				}

				public void AddAdjasentTris(int trisIdx){
					if(!AdjacentTris.Contains(trisIdx)){
						AdjacentTris.Add(trisIdx);
					}
				}
			}

			[System.Serializable]
			public struct UVert{
				public int PVertIdx;
			}
	 

			void AddUVert(Vector3 position){
				UVert nUnityVert = new UVert();
				//nUnityVert.AdjacentTris = new List<int>();
				PVert temp = new PVert( position ); 
				int idx = -1;
				if ( posVertItemsDict.TryGetValue( temp, out idx )){
					nUnityVert.PVertIdx = idx;
				} else {
					posVertItemsDict.Add( temp, posVertsList.Count );
					posVertsList.Add( temp );
					nUnityVert.PVertIdx =  posVertsList.Count-1;
				}
				unityVertices.Add( nUnityVert );
			}


			public int VertsCount;
			public int TrisCount;

			public bool HasUV0;
			public bool HasUV1;
			public Material[] InitialMaterials;
 
			public Color[] InitialVertexColors;
			public float MaxBoundSize;

			public static SourceGeometryInfoClass ConstructNew( MeshFilter mf ){
				if(mf == null){
					return null;
				}
				if(mf.sharedMesh == null){
					return null;
				}
				SourceGeometryInfoClass result = new SourceGeometryInfoClass();
				MeshRenderer mr = mf.GetComponent<MeshRenderer>();

				if(mr != null){
					result.InitialMaterials = mr.sharedMaterials;
				}

				result.MaxBoundSize =  Mathf.Max(mf.sharedMesh.bounds.size.z, Mathf.Max( mf.sharedMesh.bounds.size.x, mf.sharedMesh.bounds.size.y));  
				result.InitialVertexColors = mf.sharedMesh.colors;
				result.HasUV0 = (mf.sharedMesh.uv != null) && (mf.sharedMesh.uv.Length == mf.sharedMesh.vertexCount) ;
				result.HasUV1 = (mf.sharedMesh.uv2 != null) && (mf.sharedMesh.uv2.Length == mf.sharedMesh.vertexCount);
				result.VertsCount = mf.sharedMesh.vertexCount;
				result.TrisCount = mf.sharedMesh.triangles.Length/3;

				Vector3[] _vertices = mf.sharedMesh.vertices;
				for(int i = 0; i<_vertices.Length; i++) {
					result.AddUVert( _vertices[i] );
				}

				int[] trisIndeces = mf.sharedMesh.triangles;
				int tc = trisIndeces.Length/3;

				for(int t = 0; t<tc; t++) {
					int idxA = trisIndeces[ t*3 ];
					int idxB = trisIndeces[ t*3+1 ];
					int idxC = trisIndeces[ t*3+2 ];
					result.posVertsList[ result.unityVertices[idxA].PVertIdx ].AddAdjasentTris (t);
					result.posVertsList[ result.unityVertices[idxB].PVertIdx ].AddAdjasentTris (t);
					result.posVertsList[ result.unityVertices[idxC].PVertIdx ].AddAdjasentTris (t);
	 			}

	 			for(int v = 0; v<result.posVertsList.Count; v++){
					PVert pv = result.posVertsList[v];
					for(int t = 0; t<pv.AdjacentTris.Count; t++){
						int uvertA =  trisIndeces [ pv.AdjacentTris[t]*3 ];
						int pvA =  result.unityVertices[uvertA].PVertIdx  ;
						int uvertB =  trisIndeces [ pv.AdjacentTris[t]*3+1 ];
						int pvB =   result.unityVertices[uvertB].PVertIdx  ;
						int uvertC =  trisIndeces [ pv.AdjacentTris[t]*3+2 ];
						int pvC =   result.unityVertices[uvertC].PVertIdx  ;

						if(pvA != v && !pv.AdjacentPV.Contains(pvA)){
							pv.AdjacentPV.Add(pvA);
						}

						if(pvB != v && !pv.AdjacentPV.Contains(pvB)){
							pv.AdjacentPV.Add(pvB);
						}

						if(pvC != v && !pv.AdjacentPV.Contains(pvC)){
							pv.AdjacentPV.Add(pvC);
						}
					}
	 			}	
				return result;
			}
	 	}

		public SourceGeometryInfoClass SourceGeometry;
	}
}
