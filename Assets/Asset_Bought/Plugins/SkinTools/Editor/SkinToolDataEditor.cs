using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PolyflowStudioSkinTools{

	[CustomEditor(typeof(SkinToolsData))]
	public class SkinToolsDataEditor : Editor {

		void OnEnable () {
			SkinToolsData t = target as SkinToolsData;
 			t.hideFlags = HideFlags.HideInInspector;
		}
 
	}

}