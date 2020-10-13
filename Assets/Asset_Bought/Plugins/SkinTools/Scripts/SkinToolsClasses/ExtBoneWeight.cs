using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyflowStudioSkinTools{ 

	public class ExtBoneWeight  {

	 	public class Influence{
			public int BoneIdx;
			public float Weight;
			public Transform Tr;
			public int Priority = 0;

			public Influence (int boneIdx, float weight){
				BoneIdx = boneIdx;
				Weight = weight;
			}

			public Influence (Transform tr, float weight, int priority){
				BoneIdx = -1;
				Tr = tr;
				Weight = weight;
				Priority = priority;
			}
		}

		public ExtBoneWeight(){}

		public ExtBoneWeight ( BoneWeight bw, Transform[] bones ){
			float w0 = bw.GetWeight( 0 );
			Transform b0 = bones[bw.GetBoneIdx(0)];
			Add( b0, w0, 0);

			float w1 = bw.GetWeight( 1 );
			Transform b1 = bones[bw.GetBoneIdx(1)];
			Add( b1, w1, 0);

			float w2 = bw.GetWeight( 2 );
			Transform b2 = bones[bw.GetBoneIdx(2)];
			Add( b2, w2, 0);

			float w3 = bw.GetWeight( 3 );
			Transform b3 = bones[bw.GetBoneIdx(3)];
			Add( b3, w3, 0);
		}


		public void Blend( ExtBoneWeight other, float t ){
			for ( int i = 0; i<other.Influences.Count; i++ ){
				Add( other.Influences[i].Tr, other.Influences[i].Weight * t, 0); 
			}
		}


		public List<Influence> Influences = new List<Influence>();

		int InfluenceComparer(Influence a, Influence b){
			if(a.Priority == b.Priority){
				return a.Weight<=b.Weight ? 1:-1;
			} else if(a.Priority<b.Priority ){
				return 1;
			} else {
				return -1;
			}

		}

		Influence GetByBoneIdx(int boneIdx){
			for(int i = 0; i<Influences.Count; i++){
				if(Influences[i].BoneIdx == boneIdx){
					return Influences[i];
				}
			}
			return null;
		}

		Influence GetByTransform(Transform tr){
			for(int i = 0; i<Influences.Count; i++){
				if(Influences[i].Tr == tr){
					return Influences[i];
				}
			}
			return null;
		}


		public void AddMask(Transform tr, float weight){
			float summ = 0;
			for(int i = 0; i<Influences.Count; i++){
				summ += Influences[i].Weight;
			}
			weight = Mathf.Clamp(weight, 0, 1f-summ);
			Add(tr, weight, 1);
		}

		public void Add(Transform tr, float weight, int priority){
			if(weight<0.001f){
				return;
			}
			Influence existing = GetByTransform(tr);
			if(existing == null){
				existing =  new Influence(tr, weight, priority);
				Influences.Add (existing );
			} else{
				existing.Weight += weight;	
			}
			Influences.Sort( InfluenceComparer );
		}

		public void FillIndeces(Transform[] bones){
			for(int i = 0; (i<4 && i<Influences.Count); i++){
				for(int b = 0; b<bones.Length; b++){
					if(Influences[i].Tr == bones[b]){
						Influences[i].BoneIdx = b;
						break;
					}
				}
			}
		}

		public BoneWeight GetClampedBW(){
			float free = 1f;
			BoneWeight bw = new BoneWeight();
			for(int i = 0; (i<4 && i<Influences.Count); i++){
	 			BoneWeightExtension.SetBoneIdx( ref bw, i, Influences[i].BoneIdx);
				float clampedWeight = Mathf.Clamp( Influences[i].Weight, 0, free);
				BoneWeightExtension.SetWeight( ref bw, i, clampedWeight);
	 			free -=  clampedWeight;
			}
			float summ = 0;
			for(int i = 0; i<4; i++){
				summ += bw.GetWeight(i);
			}
			if(summ<1f){
	 			for(int i = 0; i<4; i++){
					BoneWeightExtension.SetWeight( ref bw, i, bw.GetWeight(i) / summ );
				}
			}
	 		return bw;
		}

	}

	public static class BoneWeightExtension{

		public static int GetBoneIdx( this BoneWeight bw, int idx){
			if(idx == 0){
				return bw.boneIndex0;
			}
			if(idx == 1){
				return bw.boneIndex1;
			}
			if(idx == 2){
				return bw.boneIndex2;
			}
			if(idx == 3){
				return bw.boneIndex3;
			}
			return -1;
		}

		public static void SetBoneIdx( ref BoneWeight bw, int idx, int boneIdx){
			if(idx == 0){
				bw.boneIndex0 = boneIdx;
				return;
			}
			if(idx == 1){
				bw.boneIndex1 = boneIdx;
				return;
			}
			if(idx == 2){
				bw.boneIndex2 = boneIdx;
				return;
			}
			if(idx == 3){
				bw.boneIndex3 = boneIdx;
				return;
			}
		}


		public static float GetWeight( this BoneWeight bw, int idx){
			if(idx == 0){
				return bw.weight0;
			}
			if(idx == 1){
				return bw.weight1;
			}
			if(idx == 2){
				return bw.weight2;
			}
			if(idx == 3){
				return bw.weight3;
			}
			return 0;
		}

		public static void SetWeight( ref BoneWeight bw, int idx, float weight){
			if(idx == 0){
				bw.weight0 = weight;
			}
			if(idx == 1){
				bw.weight1 = weight;
			}
			if(idx == 2){
				bw.weight2 = weight;
			}
			if(idx == 3){
				bw.weight3 = weight;
			}
		}

		public static void PrintDebugInfo(this BoneWeight bw){
			Debug.LogFormat( "bones: {0}, {1}, {2}, {3} weights:{4}, {5}, {6}, {7}", bw.boneIndex0,  bw.boneIndex1,  bw.boneIndex2,  bw.boneIndex3,  bw.weight0.ToString("F3"),  bw.weight1.ToString("F3"), bw.weight2.ToString("F3"), bw.weight3.ToString("F3") );
		}
	}
}
