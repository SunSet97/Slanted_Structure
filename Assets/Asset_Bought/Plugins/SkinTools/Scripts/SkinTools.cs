using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{ 

	[ExecuteInEditMode]
	[AddComponentMenu("Miscellaneous/Skin Tools")]
	public class SkinTools : MonoBehaviour {

		SkinToolsData _data;
		public SkinToolsData Data{
			get{
				if(_data == null){
					_data = GetComponent<SkinToolsData>();
				}
				if(_data == null){
					_data = gameObject.AddComponent<SkinToolsData>();
				}
				return _data;
			}
		}




	}
}
