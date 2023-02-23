using Data;
using UnityEngine;

namespace Utility.Cinematic
{
    public class CinematicCharacter : MonoBehaviour
    {
        public CustomEnum.Character who;

        private Animator animator;
        private SkinnedMeshRenderer skinnedMesh;
        private Texture[] faceExpression;

        private static readonly int Emotion = Animator.StringToHash("Emotion");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (skinnedMesh == null)
                skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (faceExpression == null)
            {
                if (who.Equals(CustomEnum.Character.Speat) || who.Equals(CustomEnum.Character.Oun) ||
                    who.Equals(CustomEnum.Character.Rau))
                {
                    faceExpression = Resources.LoadAll<Texture>("Face");
                }
                else if (who.Equals(CustomEnum.Character.Speat_Adolescene) ||
                         who.Equals(CustomEnum.Character.Speat_Adult) || who.Equals(CustomEnum.Character.Speat_Child))
                {
                    faceExpression = Resources.LoadAll<Texture>($"Speat_Face/{who}");
                }
            }
        }


        public void EmotionAnimationSetting(int emotionInt)
        {
            animator.SetInteger(Emotion, emotionInt);
        }

        public void ExpressionSetting(CustomEnum.Expression emotion)
        {
            if (faceExpression.Length <= (int) emotion)
            {
                return;
            }

            skinnedMesh.materials[1].SetTexture(MainTex, faceExpression[(int) emotion]); // 현재 감정으로 메터리얼 변경
        }
    }
}
