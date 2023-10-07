using UnityEngine;
using Utility.Audio;

namespace Utility.Preference.Audio
{
    public class AudioUiController : MonoBehaviour
    {
        [SerializeField] private AudioSlider audioSlider;
        
        [SerializeField] private VibeToggle vibeToggle;

        private void Start()
        {
            vibeToggle.Init();
            audioSlider.Init();
            LoadPreference();
        }

        private void LoadPreference()
        {
            AudioLoader.LoadPreference(out var audioValue, out var isVibe);
            vibeToggle.SetVibe(isVibe);
            audioSlider.SetAudio(audioValue);
        }
    }
}