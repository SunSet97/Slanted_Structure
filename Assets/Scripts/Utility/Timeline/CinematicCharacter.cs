using System;
using UnityEngine;
using Utility.Character;
using Utility.Dialogue;

namespace Utility.Timeline
{
    public class CinematicCharacter : MonoBehaviour
    {
        public CharacterType who;

        [NonSerialized] public Animator Animator;
        private SkinnedMeshRenderer skinnedMesh;
        private Texture[] faceExpression;

        private static readonly int Emotion = Animator.StringToHash("Emotion");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void Awake()
        {
            if (Animator == null)
                Animator = GetComponent<Animator>();
            if (skinnedMesh == null)
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (faceExpression == null)
            {
                if (who.Equals(CharacterType.Speat) || who.Equals(CharacterType.Oun) ||
                    who.Equals(CharacterType.Rau))
                {
                    faceExpression = Resources.LoadAll<Texture>("Face");
                }
                else if (who.Equals(CharacterType.Speat_Adolescene) ||
                         who.Equals(CharacterType.Speat_Adult) || who.Equals(CharacterType.Speat_Child))
                {
                    faceExpression = Resources.LoadAll<Texture>($"Speat_Face/{who}");
                }
            }
        }


        public void EmotionAnimationSetting(int emotionInt)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Animator.SetInteger(Emotion, emotionInt);
        }

        public void ExpressionSetting(Expression emotion)
        {
            if(!Application.isPlaying){
                return;
            }
            
            if (faceExpression.Length <= (int) emotion)
            {
                return;
            }

            skinnedMesh.materials[1].SetTexture(MainTex, faceExpression[(int) emotion]); // 현재 감정으로 메터리얼 변경
        }
    }
}
