using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{

	[ExecuteInEditMode]
	public class SkinBakerResourcesHolder : MonoBehaviour {
		public Vector3[] Rays362;
		public Vector3[] Rays642;
		public Vector3[] Rays1002;
		public Texture2D DelIkon;
		public Texture2D MoveToExcludeIkon;
		public Texture2D SelectBoneIkon;
		public Texture2D AddIkon;
		public Texture2D UpIkon;
		public Texture2D DownIkon;
		public Texture2D ExcludeIkon;
		public Texture2D ParamIkonOff;
		public Texture2D ParamIkonOn;


		public GUIStyle IkonStyle;
		public GUIStyle OnSceneGUILabel;
	}
}
