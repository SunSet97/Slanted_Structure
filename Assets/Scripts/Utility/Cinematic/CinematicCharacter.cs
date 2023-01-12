﻿using Data;
using UnityEngine;

namespace Utility.Cinematic
{
    public class CinematicCharacter : MonoBehaviour
    {
        public CustomEnum.Character who;
    
        private Animator anim; // 애니메이션
        private SkinnedMeshRenderer skinnedMesh; // 캐릭터 머테리얼
        private Texture[] faceExpression;//표정 메터리얼
        //Resources/Face/다 넣업
        private static readonly int Emotion = Animator.StringToHash("Emotion");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        void Start()
        {
            if(anim == null)
                anim = GetComponent<Animator>();
            if(skinnedMesh == null)
                if (who.Equals(CustomEnum.Character.Speat) || who.Equals(CustomEnum.Character.Oun) || who.Equals(CustomEnum.Character.Rau))
                    skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (faceExpression == null)
                faceExpression = Resources.LoadAll<Texture>("Face");
        }


        public void EmotionAnimationSetting(int emotionInt)
        {
            anim.SetInteger(Emotion, emotionInt); // 애니메이션실행
        }
        public void ExpressionSetting(int emotionInt)
        {
            if (who.Equals(CustomEnum.Character.Speat) || who.Equals(CustomEnum.Character.Oun) || who.Equals(CustomEnum.Character.Rau))
                skinnedMesh.materials[1].SetTexture(MainTex, faceExpression[emotionInt]); // 현재 감정으로 메터리얼 변경
        }

    }
}
