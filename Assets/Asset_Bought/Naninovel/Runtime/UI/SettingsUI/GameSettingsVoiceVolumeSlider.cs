// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsVoiceVolumeSlider : ScriptableSlider
    {
        [Tooltip("When provided, the slider will control voice volume of the printed message author (character actor) with the provided ID. When empty will control voice mixer group volume.")]
        [SerializeField] private string authorId = default;

        private IAudioManager audioMngr;

        protected override void Awake ()
        {
            base.Awake();

            audioMngr = Engine.GetService<IAudioManager>();
        }

        protected override void Start ()
        {
            base.Start();

            var authorVolume = audioMngr.GetAuthorVolume(authorId);
            UIComponent.value = authorVolume == -1 ? audioMngr.VoiceVolume : authorVolume;
        }

        protected override void OnValueChanged (float value)
        {
            if (string.IsNullOrEmpty(authorId))
                audioMngr.VoiceVolume = value;
            else audioMngr.SetAuthorVolume(authorId, value);
        }
    }
}
