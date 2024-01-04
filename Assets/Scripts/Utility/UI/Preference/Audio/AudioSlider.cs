using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;

namespace Utility.UI.Preference.Audio
{
    public class AudioSlider : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Slider audioSlider;
        [SerializeField] private Animator muteAnimator;
#pragma warning restore 0649

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
            muteAnimator.SetBool(Mute, Mathf.Approximately(audioValue, 0));
        }
    }
}