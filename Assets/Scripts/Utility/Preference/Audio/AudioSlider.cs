using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;

namespace Utility.Preference.Audio
{
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private Slider audioSlider;
        [SerializeField] private Animator muteAnimator;

        private static readonly int Mute = Animator.StringToHash("Mute");

        private void Awake()
        {
            audioSlider.onValueChanged.AddListener(value =>
            {
                muteAnimator.SetBool(Mute, Mathf.Approximately(value, 0));
                AudioLoader.SaveAudio(value);
            });
        }

        private void OnEnable()
        {
            AudioLoader.LoadAudio(out var audioValue);
            audioSlider.value = audioValue;
        }
    }
}