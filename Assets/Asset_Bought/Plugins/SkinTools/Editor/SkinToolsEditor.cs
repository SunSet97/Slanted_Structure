using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AnimatedValues;

namespace PolyflowStudioSkinTools{

	[CustomEditor(typeof(SkinTools))]
	public class SkinToolsEditor : Editor {

		static float brushRadius = 0.05f;
		static float boneGizmoSize = 0.01f;

	 	static AnimBool _mapBonesFoldoutAB;
	 	static bool _mapBonesFoldout;

		static AnimBool _sourceGeometryFoldoutAB;
		static bool _sourceGeometryFoldout;

		static AnimBool _sourceSkinFoldoutAB;
		static bool _sourceSkinFoldout;

		static AnimBool _sourceSkinBonesFoldoutAB;
		static bool _sourceSkinBonesFoldout;

		static AnimBool _resultFoldoutAB;
		static bool _resultFoldout;

		static AnimBool _paramFoldoutAB;
		static bool _paramFoldout;

		bool paintExcludedTriangles;
		bool stroke = false;

		void OnEnable(){
			_mapBonesFoldoutAB = new AnimBool(_mapBonesFoldout, Repaint );
			_sourceGeometryFoldoutAB = new AnimBool(_sourceGeometryFoldout, Repaint );
			_sourceSkinFoldoutAB = new AnimBool(false, Repaint );
			_resultFoldoutAB =  new AnimBool(_resultFoldout, Repaint );
			_sourceSkinBonesFoldoutAB = new AnimBool(false, Repaint);
			_paramFoldoutAB = new AnimBool(_paramFoldout, Repaint);
		}
	 
		class SourceSMRCollider{
			public MeshCollider _Collider;
			public SourceSMRCollider (SkinnedMeshRenderer sourceSMR, bool inverted){
				GameObject colliderGO = new GameObject(sourceSMR.name + "mCollider");
				colliderGO.transform.position = sourceSMR.transform.position;
				colliderGO.transform.rotation = sourceSMR.transform.rotation;
		
				Mesh nMesh = new Mesh();
				sourceSMR.BakeMesh( nMesh );
				_Collider = colliderGO.AddComponent<MeshCollider>();
				if(inverted){
					int[] tris = nMesh.triangles;
					for(int r = 0; r<tris.Length; r+=3){
						int t0 = tris[r+2];
						int t1 = tris[r+1];
						int t2 = tris[r ];
						tris[r] = t0;
						tris[r+1] = t1;
						tris[r+2] = t2;
					}
					nMesh.triangles = tris;
				}

				_Collider.sharedMesh = nMesh; 
			}

			public void Destroy(){
				DestroyImmediate( _Collider.gameObject );
			}
		}
	 
		public override void OnInspectorGUI(){
			SkinTools t = target as SkinTools;
			SkinToolsData data = t.Data;

			_mapBonesFoldout =  EditorGUILayout.Foldout( _mapBonesFoldout, "Map Bones" );
			_mapBonesFoldoutAB.target = _mapBonesFoldout;
			if(EditorGUILayout.BeginFadeGroup(_mapBonesFoldoutAB.faded)){
				DrawMapBonesProperties(t,data);
	 		}
			EditorGUILayout.EndFadeGroup();

			_sourceSkinFoldoutAB.target = EditorGUILayout.Foldout( _sourceSkinFoldoutAB.target, ("Reference skin: "+ (data.ReferenceSMR == null? "none":data.ReferenceSMR.name)), true );
			if(EditorGUILayout.BeginFadeGroup(_sourceSkinFoldoutAB.faded)){
				DrawReferenceSkinProperties(t,data);
	 		}
			EditorGUILayout.EndFadeGroup();



			_sourceGeometryFoldout =  EditorGUILayout.Foldout( _sourceGeometryFoldout, ("Source geometry: "+(data.SourceGeometryMF == null? "none":data.SourceGeometryMF.name )) );
			_sourceGeometryFoldoutAB.target = _sourceGeometryFoldout;
			if(EditorGUILayout.BeginFadeGroup(_sourceGeometryFoldoutAB.faded)){
				DrawSourceGeometryProperties(t,data);
	 		}
			EditorGUILayout.EndFadeGroup();

			_resultFoldout =  EditorGUILayout.Foldout( _resultFoldout, ("Result mesh: "+(data.ResultMesh == null? "none":data.ResultMesh.name )) );
			_resultFoldoutAB.target = _resultFoldout;
			if(EditorGUILayout.BeginFadeGroup(_resultFoldoutAB.faded)){
				DrawResultProperties(t,data);
	 		}
			EditorGUILayout.EndFadeGroup();


			GUILayout.Space(6);

			if(EditorGUILayout.BeginFadeGroup(_paramFoldoutAB.faded)){
				GUILayout.BeginHorizontal();
				GUILayout.Label( "Projection quality:" );
				int nQuality = EditorGUILayout.Popup(data.BakingQuality, data.BakingQualityNames );
				if(nQuality !=  data.BakingQuality){
					data.BakingQuality = nQuality;
					ApplyChangesToScene(t);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Blur multiplier: "+data.BlurMultiplier.ToString("F2") , GUILayout.Width(120));
				float nBlurMult = GUILayout.HorizontalSlider( data.BlurMultiplier, 0, 1f);
				if(data.BlurMultiplier != nBlurMult){
					data.BlurMultiplier = nBlurMult;
					ApplyChangesToScene(t);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Blur iterations:"+data.BlurIterations.ToString(), GUILayout.Width(120));
				int nIterations = (int)GUILayout.HorizontalSlider( data.BlurIterations, 0, 10);
				if(data.BlurIterations != nIterations){
					data.BlurIterations = nIterations;
					ApplyChangesToScene(t);
				}
	 			GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Bone gizmo width:", GUILayout.Width(120));
				float nBoneGizmoSize = GUILayout.HorizontalSlider( boneGizmoSize, 0.002f, 0.1f);
				if(nBoneGizmoSize != boneGizmoSize){
					boneGizmoSize = nBoneGizmoSize;
					SceneView.RepaintAll();
				}
				GUILayout.EndHorizontal();
			}
			EditorGUILayout.EndFadeGroup();

			GUILayout.BeginHorizontal();
			_paramFoldout = GUILayout.Toggle( _paramFoldout,  _paramFoldout?   ResourceHolder.ParamIkonOn : ResourceHolder.ParamIkonOff, ResourceHolder.IkonStyle );
			_paramFoldoutAB.target = _paramFoldout; 
			if(GUILayout.Button("Generate Skinning")){
			 	GenerateFilter();	
			}
			GUILayout.EndHorizontal();
	 	}

		void BlurPass(){
			SkinTools t = target as SkinTools;
			SkinToolsData data = t.Data;
			data.UpdateResultInfo();

			for (int v = 0; v<data.SourceGeometry.posVertsList.Count; v++ ){
				data.SourceGeometry.posVertsList[v].BonesWeights = new float[data.ResultBones.Length];
			}

			for (int i = 0; i<data.SourceGeometry.unityVertices.Count; i++ ){
				int parent = data.SourceGeometry.unityVertices[i].PVertIdx;

				for(int b = 0; b<data.ResultBones.Length; b++){
					data.SourceGeometry.posVertsList[parent].BonesWeights[b] =  data.ResultBones[b].Weights[i];
				}
			}

			for( int b = 0; b<data.ResultBones.Length; b++ ){
				for(int i = 0; i<data.BlurIterations; i++){
					for (int v = 0; v<data.SourceGeometry.posVertsList.Count; v++ ){
						float lerpVal = (1f/data.SourceGeometry.posVertsList[v].AdjacentPV.Count) * data.BlurMultiplier;
						for(int a = 0; a<data.SourceGeometry.posVertsList[v].AdjacentPV.Count; a++){
							int apv =   data.SourceGeometry.posVertsList[v].AdjacentPV[a];
							data.SourceGeometry.posVertsList[v].BonesWeights[b] = Mathf.Lerp( data.SourceGeometry.posVertsList[v].BonesWeights[b], data.SourceGeometry.posVertsList[apv].BonesWeights[b], lerpVal );
						}
					}
				}
			}

			Transform[] bones = data.resultSMR.bones;
			for (int v = 0; v<data.SourceGeometry.posVertsList.Count; v++ ){
				data.SourceGeometry.posVertsList[v].ebw = new ExtBoneWeight();
				for(int b = 0; b<data.ResultBones.Length; b++){
					data.SourceGeometry.posVertsList[v].ebw.Add( bones[b], data.SourceGeometry.posVertsList[v].BonesWeights[b], 0);
				}
			}

			BoneWeight[] weights =  data.ResultMesh.boneWeights ;
			for (int i = 0; i<data.SourceGeometry.unityVertices.Count; i++ ){
				int parent = data.SourceGeometry.unityVertices[i].PVertIdx;
				data.SourceGeometry.posVertsList[parent].ebw.FillIndeces( bones );
				weights[i] = data.SourceGeometry.posVertsList[parent].ebw.GetClampedBW();
			}
	 		data.ResultMesh.boneWeights = weights;
			data.resultSMR.sharedMesh = data.ResultMesh;
	 		data.UpdateResultInfo();
	 		data.ApplyResultVisual();
		}

		void DrawReferenceSkinProperties(SkinTools t, SkinToolsData data){
			GUILayout.BeginHorizontal();
			GUILayout.Label("select:", GUILayout.Width(100));
			SkinnedMeshRenderer nSourceSkin = (SkinnedMeshRenderer)EditorGUILayout.ObjectField( data.ReferenceSMR,  typeof(SkinnedMeshRenderer), true   );
			if(nSourceSkin != data.ReferenceSMR){
				data.ReferenceSMR = nSourceSkin;
				data.OnChangeReferenceSMR( nSourceSkin );
				ApplyChangesToScene(t);
			}
		 	GUILayout.EndHorizontal();

			if(data.ReferenceMeshIsValid){
				EditorGUI.indentLevel +=1;
				GUILayout.Label("Excluded triangles");

				GUILayout.BeginHorizontal();
				bool npaintExcludedTriangles = GUILayout.Toggle( paintExcludedTriangles, "Paint", "Button", GUILayout.Width(80));
				if(npaintExcludedTriangles != paintExcludedTriangles){
					paintExcludedTriangles = npaintExcludedTriangles;
					SceneView.RepaintAll(); 
				}
				GUILayout.Label("Brush radius:", GUILayout.Width(100));
				brushRadius = GUILayout.HorizontalSlider( brushRadius,  0.002f, 1f);

				GUILayout.EndHorizontal();

				_sourceSkinBonesFoldout = EditorGUILayout.Foldout( _sourceSkinBonesFoldout,  "Reference bones");
				_sourceSkinBonesFoldoutAB.target = _sourceSkinBonesFoldout;

				if(EditorGUILayout.BeginFadeGroup(_sourceSkinBonesFoldoutAB.faded)){
					for(int i = 0; i<data.ReferenceSMRBones.Count; i++ ){
						GUILayout.BeginHorizontal();
						GUILayout.Space(24);
						GUILayout.Label(data.ReferenceSMRBones[i].Tr.name, GUILayout.Width(160));
						bool nVal = GUILayout.Toggle( data.ReferenceSMRBones[i].Excluded, ResourceHolder.ExcludeIkon, ResourceHolder.IkonStyle);
						if(  nVal != data.ReferenceSMRBones[i].Excluded ){
							data.ReferenceSMRBones[i].Excluded = nVal;
							ApplyChangesToScene(t);
						}
						GUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUI.indentLevel -=1; 
			}
		}

		void DrawMapBonesProperties(SkinTools t, SkinToolsData data){
	 
			if(GUILayout.Button(ResourceHolder.AddIkon, ResourceHolder.IkonStyle )){
				data.MapBones.Insert( 0, new SkinToolsData.MapBone());
			}

			for(int b = 0; b < data.MapBones.Count; b++){
				GUILayout.BeginHorizontal();
				Transform nTr = (Transform)EditorGUILayout.ObjectField( data.MapBones[b].Tr, typeof(Transform), true  );
				if(data.MapBones[b].Tr != nTr){
					data.MapBones[b].Tr = nTr;
					ApplyChangesToScene(t);
				}

				Texture2D nTex = (Texture2D)EditorGUILayout.ObjectField( data.MapBones[b].Map, typeof(Texture2D), true );
				if(data.MapBones[b].Map != nTex){
					data.MapBones[b].Map = nTex;
					ApplyChangesToScene(t);
				}

				 
				if( GUILayout.Button(ResourceHolder.UpIkon, ResourceHolder.IkonStyle ) ){
					int prevIdx = b-1;
					if(prevIdx<0){
						prevIdx = data.MapBones.Count - 1;
					}
					SkinToolsData.MapBone _prev = data.MapBones[ prevIdx ];
					SkinToolsData.MapBone _this = data.MapBones[b];
					data.MapBones[b] = _prev;
					data.MapBones[ prevIdx ] = _this;
					ApplyChangesToScene(t);
				}
				

				
				if( GUILayout.Button(ResourceHolder.DownIkon, ResourceHolder.IkonStyle ) ){
					int nextIdx = b+1;
					if(nextIdx>=data.MapBones.Count){
						nextIdx = 0;
					}
					SkinToolsData.MapBone _next = data.MapBones[nextIdx];
					SkinToolsData.MapBone _this = data.MapBones[b];
					data.MapBones[b] = _next;
					data.MapBones[nextIdx] = _this;
					ApplyChangesToScene(t);
				}
				 

				if(GUILayout.Button(ResourceHolder.DelIkon, ResourceHolder.IkonStyle )){
					t.Data.MapBones.RemoveAt( b );
					ApplyChangesToScene(t);
				}

				GUILayout.EndHorizontal();
			}
		}

		void DrawSourceGeometryProperties(SkinTools t, SkinToolsData data){
			GUILayout.BeginHorizontal();
			GUILayout.Label("select:", GUILayout.Width(100) );
			MeshFilter nSourceMeshMF  = (MeshFilter)EditorGUILayout.ObjectField( data.SourceGeometryMF, (typeof(MeshFilter)), true);
			if( nSourceMeshMF != data.SourceGeometryMF){
				data.SourceGeometryMF = nSourceMeshMF;
				ApplyChangesToScene(t);
				data.OnChangeSourceGeometry(nSourceMeshMF);
			}
			GUILayout.EndHorizontal();
			if(data.SourceGeometry != null){
				GUILayout.Label(string.Format("{0} verts, {1} tris", data.SourceGeometry.VertsCount, data.SourceGeometry.TrisCount  ) +  (data.SourceGeometry.HasUV0? ", uv ":" ") +  (data.SourceGeometry.HasUV1? " UV1 ":" ")  );
			 
	 		}
		}

		void DrawResultProperties( SkinTools t, SkinToolsData data ){
			GUILayout.Label("path:"+ data.OutputMeshPath );
			GUILayout.BeginHorizontal();
			GUILayout.Label("select:", GUILayout.Width(45));
			Mesh nOutputMesh  = (Mesh)EditorGUILayout.ObjectField( data.ResultMesh, (typeof(Mesh)), false);
			 

			if( nOutputMesh != data.ResultMesh){
				string nPath = AssetDatabase.GetAssetPath( nOutputMesh );
				System.Type x =  AssetDatabase.GetMainAssetTypeAtPath( nPath );

 				if( x != typeof(UnityEngine.Mesh) ){
					EditorUtility.DisplayDialog( "SkinTools", "Output mesh can not be part of prefab. Select other mesh or create new", "Ok" );
 					return;
				} else {
					data.ResultMesh =  nOutputMesh;
					data.OutputMeshPath = nPath;
				}
				ApplyChangesToScene(t);
			}

			if(GUILayout.Button("new", GUILayout.Width(40))){
				string selectedFilePath = EditorUtility.SaveFilePanelInProject("Save mesh", "", "asset", "Please select a file to save mesh to");
				if( !string.IsNullOrEmpty(selectedFilePath)  ){
					
	 				Mesh nMesh = new Mesh();
					nMesh.vertices = new Vector3[4];
					AssetDatabase.CreateAsset(nMesh, selectedFilePath);
					data.OutputMeshPath = selectedFilePath;
					data.ResultMesh =  AssetDatabase.LoadAssetAtPath( data.OutputMeshPath , typeof (Mesh)) as Mesh;
					ApplyChangesToScene(t);
				}
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			GUILayout.Label("Affected bones:");
			GUILayout.EndHorizontal();

			for(int i = 0; i<data.ResultBones.Length; i++){
				GUILayout.BeginHorizontal();
				bool isSel = data.SelectedResultBone == i;
				GUILayout.Space(20);
				GUILayout.Label("#"+i.ToString(), GUILayout.Width(26));
				bool nIsSel = GUILayout.Toggle(isSel, data.ResultBones[i].Tr.name, "Button", GUILayout.Width(160) );
				if( nIsSel != isSel ) {
					if(nIsSel){
						data.SelectedResultBone = i;
					} else {
						data.SelectedResultBone = -1;
					}
					data.ApplyResultVisual();
	 				ApplyChangesToScene(t);
	 			}
				if(data.SelectedResultBone == i){
					if(GUILayout.Button(ResourceHolder.MoveToExcludeIkon, ResourceHolder.IkonStyle)){
						SkinToolsData.ReferenceSMRBonesClass rsmb = data.ReferenceSMRBones;
						if(rsmb != null){
							SkinToolsData.ReferenceBone rb = data.ReferenceSMRBones[ data.ResultBones[i].Tr ];
							if(rb != null){
								rb.Excluded = true;
							}
						}
						GenerateFilter();
		 			}

					if(GUILayout.Button(ResourceHolder.SelectBoneIkon, ResourceHolder.IkonStyle)){
	 					Selection.activeGameObject = data.ResultBones[i].Tr.gameObject;
		 			}
	 			}

				GUILayout.EndHorizontal();
			}
		}

		void GenerateFilter(){
			SkinTools t = target as SkinTools;
			SkinToolsData data = t.Data;

			if(ResourceHolder == null){
				EditorUtility.DisplayDialog( "SkinTools", "Requared prefab SkinBakerResourcesHolder_SBRTSGJHKNBHD not found. \nPlease restore it or reimport SkinTools package. \n Operation canceled", "Ok" );
 				return;
			}

			for(int b = 0; b<data.MapBones.Count; b++){
				if(data.MapBones[b].Map == null){
					EditorUtility.DisplayDialog( "SkinTools", string.Format( "Empty texture field at #{0} map bone. \nPlease assign texture or delete bone. \nOperation canceled.", b), "Ok" );
					return;
				}

				if(data.MapBones[b].Tr == null){
					EditorUtility.DisplayDialog( "Skin baker", string.Format( "Empty transform field at #{0} map bone. Please assign bone transform or delete bone. Operation canceled.", b), "Ok" );
					return;
				}
				string p = AssetDatabase.GetAssetPath(data.MapBones[b].Map) ;
				TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath( p );
				bool needReimport = false;

				if(ti.isReadable == false){
					ti.isReadable = true;
					needReimport = true;
				}

				if(ti.alphaSource != TextureImporterAlphaSource.None){
					ti.alphaSource = TextureImporterAlphaSource.None;
					needReimport = true;
				}

				if(needReimport){
					Debug.LogFormat("{0} reimported", p);
					ti.SaveAndReimport();
				}
			}

			if(data.SourceGeometryMF == null || data.SourceGeometryMF.sharedMesh == null ){
				EditorUtility.DisplayDialog( "Skin Tools", "Source Geometry not found. Please assign Source Geometry field.  Operation canseled", "Ok" );
				return;
			}

	 		 GenerateSkinning();
	 		 if(data.BlurIterations>0 ){
	 		 	BlurPass();
	 		 }

	 		 ApplyChangesToScene(t);
		}

		void GenerateSkinning (){
			SkinTools t = target as SkinTools;
			SkinToolsData data = t.Data;
	 		bool referenceExist = data.ReferenceSMR != null;
			Vector3[] rVertices = data.SourceGeometryMF.sharedMesh.vertices;
			Vector3[] rNormals = data.SourceGeometryMF.sharedMesh.normals;
			Vector3[] rWorldVertices = new Vector3[rVertices.Length]; 
			Vector2[] rMapVertices = data.SourceGeometryMF.sharedMesh.uv;
			Matrix4x4 smLTW = data.SourceGeometryMF.transform.localToWorldMatrix;
			Matrix4x4 tWTL = t.transform.worldToLocalMatrix;

			for(int v = 0; v<rVertices.Length; v++){
				rWorldVertices[v] = smLTW.MultiplyPoint3x4(rVertices[v]);
				rVertices[v] = tWTL.MultiplyPoint3x4( rWorldVertices[v] );
				rNormals[v] = smLTW.MultiplyVector(rNormals[v]); 
				rNormals[v] = tWTL.MultiplyVector( rNormals[v] ); 
			}

	 
			data.ResultMesh = Instantiate(data.SourceGeometryMF.sharedMesh) as Mesh;
			string[] splittedPath = data.OutputMeshPath.Split("/.".ToCharArray());
			string meshName = splittedPath[splittedPath.Length-2];
			data.ResultMesh.name = meshName;

			data.ResultMesh.vertices = rVertices;
			data.ResultMesh.normals = rNormals;
	 		data.ResultMesh.RecalculateBounds();

			data.resultSMR.localBounds = new Bounds( data.ResultMesh.bounds.center, data.ResultMesh.bounds.size*2 );

			SourceSMRCollider _collider = null;
			SourceSMRCollider _icollider = null;
			BoneWeight[] existingWeights = null;
			Transform[] sourceBones = null;
			int[] sourceSMRTris = null;
			Ray[] frays = null;
			bool[] excludedTriangles = null;

			if(referenceExist){
				_collider = new SourceSMRCollider( data.ReferenceSMR, false); 
				_icollider = new SourceSMRCollider( data.ReferenceSMR, true); 
				existingWeights = data.ReferenceSMR.sharedMesh.boneWeights;
				sourceBones = data.ReferenceSMR.bones;
				sourceSMRTris = data.ReferenceSMR.sharedMesh.triangles;
				excludedTriangles = new bool[data.ReferenceSMR.sharedMesh.triangles.Length/3];
				if(data.ReferenceSMRGeometry != null && data.ReferenceSMRGeometry.IsValid){
					excludedTriangles = data.ReferenceSMRGeometry.ExcludedTris;
				}

				if(data.BakingQuality == 0){
					frays = new Ray[ResourceHolder.Rays362.Length];
					for(int i = 0; i<frays.Length; i++){
						frays[i] = new Ray( Vector3.zero, ResourceHolder.Rays362[i] );
					}
				} else if(data.BakingQuality == 1){
					frays = new Ray[ResourceHolder.Rays642.Length];
					for(int i = 0; i<frays.Length; i++){
						frays[i] = new Ray( Vector3.zero, ResourceHolder.Rays642[i] );
					}
				}  else if(data.BakingQuality == 2){
					frays = new Ray[ResourceHolder.Rays1002.Length];
					for(int i = 0; i<frays.Length; i++){
						frays[i] = new Ray( Vector3.zero, ResourceHolder.Rays1002[i] );
					}
				}
			}

			ExtBoneWeight[] ebws = new ExtBoneWeight[rWorldVertices.Length];
			List<Transform> newBonesList = new List<Transform>();

			for(int b = 0; b<data.MapBones.Count; b++){
				newBonesList.Add(  data.MapBones[b].Tr  );
			}

			EditorUtility.DisplayProgressBar( "Bake", "Baking vertices", 0);
			int progressPercent = 0;

			for(int v = 0; v<rWorldVertices.Length; v++){
				ebws[v] = new ExtBoneWeight(  );

				if(data.SourceGeometry.HasUV0){
					Vector2 uv = rMapVertices[v];
					for(int b = 0; b<data.MapBones.Count; b++){
						float val = data.MapBones[b].Map.GetPixelBilinear( uv.x, uv.y ).grayscale;
						ebws[v].AddMask (  data.MapBones[b].Tr, val );
					}
				}

				if(referenceExist){
					RaycastHit hit = GetNearestHit(frays, _collider._Collider, _icollider._Collider, rWorldVertices[v], excludedTriangles);
					BoneWeight bwA = existingWeights [sourceSMRTris[ hit.triangleIndex * 3 ]];
					BoneWeight bwB = existingWeights [sourceSMRTris[ hit.triangleIndex * 3+1 ]];
					BoneWeight bwC = existingWeights [sourceSMRTris[ hit.triangleIndex * 3+2 ]];
					for(int i = 0; i<4; i++){
						Transform nbA = sourceBones[bwA.GetBoneIdx(i)];
						Transform nbB = sourceBones[bwB.GetBoneIdx(i)];
						Transform nbC = sourceBones[bwC.GetBoneIdx(i)];

						if( !data.ReferenceSMRBones[nbA].Excluded ){
	 						if(!newBonesList.Contains(nbA)){ 
								newBonesList.Add(nbA);
							}
							ebws[v].Add( nbA, bwA.GetWeight(i) * hit.barycentricCoordinate.x, 0 ); 
						}
			
						if( !data.ReferenceSMRBones[nbB].Excluded  ){ 
							if(!newBonesList.Contains(nbB)) {
								newBonesList.Add(nbB);
							}
							ebws[v].Add( nbB, bwB.GetWeight(i) * hit.barycentricCoordinate.y, 0 ); 
						}

						if( !data.ReferenceSMRBones[nbC].Excluded ){ 
							if(!newBonesList.Contains(nbC)) { 
								newBonesList.Add(nbC);
							}
							ebws[v].Add( nbC, bwC.GetWeight(i) * hit.barycentricCoordinate.z, 0);
						}
					}
				}
				float progress = v/(float)rWorldVertices.Length;
				int percent = Mathf.FloorToInt (progress * 100);
				if(progressPercent != percent){
					progressPercent = percent;
					string progressBarName =  string.Format( "Bake {0}", data.ResultMesh.name );
					string progressInfo = string.Format("baking vertices {0} of {1} with {2} quality ", v, rWorldVertices.Length, data.BakingQualityNames[data.BakingQuality] );
					EditorUtility.DisplayProgressBar( progressBarName, progressInfo , progress);
				}
	 		}

			EditorUtility.ClearProgressBar();

			BoneWeight[] newweights = new BoneWeight[rWorldVertices.Length];
	 
	 		Transform[] newBonesArray = newBonesList.ToArray();
	 		for(int w = 0; w<ebws.Length; w++){
	 			ebws[w].FillIndeces( newBonesArray);
	 			newweights[w] = ebws[w].GetClampedBW();
	 		}

			data.resultSMR.bones = newBonesArray;
			Matrix4x4[] bindBones = new Matrix4x4[newBonesArray.Length];

			for(int b = 0; b<newBonesArray.Length; b++){
				bindBones[b] = newBonesArray[b].worldToLocalMatrix * t.transform.localToWorldMatrix;
			}

			data.ResultMesh.bindposes = bindBones;
			data.ResultMesh.boneWeights = newweights;

			if(referenceExist){
				_collider.Destroy();
				_icollider.Destroy();
			}
 
			AssetDatabase.DeleteAsset(   data.OutputMeshPath );
			AssetDatabase.CreateAsset(data.ResultMesh, data.OutputMeshPath );
			AssetDatabase.SaveAssets();
 			data.ResultMesh =  AssetDatabase.LoadAssetAtPath( data.OutputMeshPath, typeof(Mesh)) as Mesh;
			data.resultSMR.sharedMesh = data.ResultMesh;
	 		data.UpdateResultInfo();
			data.ApplyResultVisual();
		}

		internal void ApplyChangesToScene (SkinTools b){
			if(!Application.isPlaying){
				EditorUtility.SetDirty(b);
				EditorSceneManager.MarkAllScenesDirty();
			}
		}
	 
		RaycastHit GetNearestHit( Ray[] rays, MeshCollider col, MeshCollider icol, Vector3 vertPos , bool[] excludedTriangles){
			RaycastHit tempHit = new RaycastHit();
			RaycastHit nearestHit = new RaycastHit();
			float minDistance = float.MaxValue;
			int hitsCount = 0;
			for(int d = 0; d<rays.Length; d++){
				rays[d].origin = vertPos;
	 			if(  col.Raycast( rays[d], out tempHit, 100)){
					if(tempHit.distance< minDistance){
						if( !excludedTriangles [ tempHit.triangleIndex ] ){
							nearestHit = tempHit;
							minDistance = nearestHit.distance;
						}
					}
				}
				if(  icol.Raycast( rays[d], out tempHit, 100)){
					hitsCount ++;
					if(tempHit.distance< minDistance){
						if( !excludedTriangles [ tempHit.triangleIndex ] ){
							nearestHit = tempHit;
							minDistance = nearestHit.distance;
						}
				 
					}
				}
			}
			if(hitsCount == 0){
				Debug.LogError("No hits!");
			}
			return nearestHit;
		}

		void OnSceneGUI(){
			SkinTools t = target as SkinTools;
			SkinToolsData data = t.Data;

			if(_resultFoldout){
				DrawResultBonesOnSceneGUI( data );
			}


	 		if(paintExcludedTriangles && data.ReferenceSMRGeometry != null && data.ReferenceSMRGeometry.IsValid){ 
	 			PaintSourceSMRExcludedTriangles(data);
	 		}


		}

		void DrawResultBonesOnSceneGUI ( SkinToolsData data ){
			for(int r = 0; r<data.ResultBones.Length; r++){
				Color col = Color.red;
				if(data.SelectedResultBone == r){
					col = Color.green;
				}
				col.a = 0.3f;
				Handles.color = col;
				for(int c = 0; c<data.ResultBones[r].Childs.Length; c++){ 
					data.ResultBones[r].Childs[c].UpdateTriangles(boneGizmoSize);
					Handles.DrawAAConvexPolygon( data.ResultBones[r].Childs[c].htris0 );
					Handles.DrawAAConvexPolygon( data.ResultBones[r].Childs[c].htris1 );
					Handles.DrawAAConvexPolygon( data.ResultBones[r].Childs[c].htris2 );
					Handles.DrawAAConvexPolygon( data.ResultBones[r].Childs[c].htris3 );
					Handles.DrawAAConvexPolygon( data.ResultBones[r].Childs[c].hcap );
				}
	 		}
		}

		void PaintSourceSMRExcludedTriangles(SkinToolsData data){
			SceneView.lastActiveSceneView.orthographic = true;
			Color selectedColor = Color.Lerp( Color.red, Color.yellow , 0.2f);
			selectedColor.a = 0.4f;

			bool _shift = !Event.current.shift;
			bool _alt =  Event.current.alt;
			 
			Color brushColor = _shift?	selectedColor : new Color(0.5f, 0.5f, 0.5f, 0.5f);
 
		 

			if (  Event.current.rawType == EventType.MouseDown && Event.current.button == 0){
				stroke = true;
			}

			if (  Event.current.rawType == EventType.MouseUp && Event.current.button == 0){
				stroke = false;
			}

			if(paintExcludedTriangles){  
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				Ray _ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
				if(stroke && !_alt){
					data.ReferenceSMRGeometry.PaintMask( _shift,  _ray, brushRadius);
	 			}

	 			Handles.color = selectedColor;
				for(int i = 0; i<data.ReferenceSMRGeometry.triangles.Length; i++){
					if(data.ReferenceSMRGeometry.triangles[i].Masked){
						Handles.DrawAAConvexPolygon( data.ReferenceSMRGeometry.triangles[i].Corners);
					}
				}

 
				Handles.color = brushColor;
				Handles.DrawSolidDisc( _ray.origin+_ray.direction*0.1f, _ray.direction, brushRadius);
				HandleUtility.Repaint();

				Handles.BeginGUI();
				//GUILayout.BeginArea();
				Rect _rect = new Rect( Screen.width/2-200, Screen.height-60, 300, 50);
				GUI.Label( _rect, "Paint excluded triangles. Use shift for clear selection", ResourceHolder.OnSceneGUILabel);
		        Handles.EndGUI();


			}
		}

		SkinBakerResourcesHolder _resourceHolder;
		public SkinBakerResourcesHolder ResourceHolder{
			get{
				if(_resourceHolder == null){
					string[] result = AssetDatabase.FindAssets( "SkinBakerResourcesHolder_SBRTSGJHKNBHD t:GameObject"); 
					if(result != null && result.Length>0){
						string path = AssetDatabase.GUIDToAssetPath(result[0]);
						_resourceHolder =  (SkinBakerResourcesHolder)AssetDatabase.LoadAssetAtPath(path, typeof(SkinBakerResourcesHolder)   );
					}
				}
				return _resourceHolder;
			}
		}
	}


}
