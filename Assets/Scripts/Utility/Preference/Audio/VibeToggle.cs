using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;

namespace Utility.Preference.Audio
{
    public class VibeToggle : MonoBehaviour
    {
        [SerializeField] private Button vibeToggle;
        [SerializeField] private Animator vibeAnimator;
        
        private static readonly int Vibe = Animator.StringToHash("Vibe");

        public void Awake()
        {
            vibeToggle.onClick.AddListener(() =>
            {
                var isVibe = vibeAnimator.GetBool(Vibe);
                vibeAnimator.SetBool(Vibe, !isVibe);
                AudioLoader.SaveVibe(!isVibe);
            });
        }

        private void OnEnable()
        {
            AudioLoader.LoadVibe(out var isVibe);
            vibeAnimator.SetBool(Vibe, isVibe);
        }
    }
}