// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class BacklogMessage : ScriptableUIBehaviour
    {
        [System.Serializable]
        public struct State
        {
            public string MessageText;
            public string ActorNameText;
            public List<string> VoiceClipNames;
            public PlaybackSpot RollbackSpot;
        }

        public virtual string Message => messageText.text;
        public virtual string ActorName => actorNameText.text;

        protected virtual Text MessageText => messageText;
        protected virtual Text ActorNameText => actorNameText;
        protected virtual Button PlayVoiceButton => playVoiceButton;
        protected virtual Button RollbackButton => rollbackButton;

        [SerializeField] private Text messageText = default;
        [SerializeField] private Text actorNameText = default;
        [SerializeField] private Button playVoiceButton = default;
        [SerializeField] private Button rollbackButton = default;

        private readonly List<string> voiceClipNames = new List<string>();
        private PlaybackSpot rollbackSpot = PlaybackSpot.Invalid;
        private IAudioManager audioManager;
        private IStateManager stateManager;

        public virtual State GetState () => new State { 
            MessageText = MessageText.text, 
            ActorNameText = ActorNameText.text, 
            VoiceClipNames = voiceClipNames,
            RollbackSpot = rollbackSpot
        };

        /// <summary>
        /// Initializes the backlog message.
        /// </summary>
        /// <param name="message">Text of the message.</param>
        /// <param name="authorName">Name of the message author.</param>
        /// <param name="voiceClipNames">Voice replay clip names associated with the message. Provide null to disable voice replay.</param>
        /// <param name="rollbackSpot">Rollback spot associated with the message. Provide <see cref="PlaybackSpot.Invalid"/> to disable rollback.</param>
        public virtual void Initialize (string message, string authorName, List<string> voiceClipNames, PlaybackSpot rollbackSpot)
        {
            MessageText.text = message;
            if (string.IsNullOrWhiteSpace(authorName))
            {
                ActorNameText.text = null;
                ActorNameText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                ActorNameText.text = authorName;
                ActorNameText.transform.parent.gameObject.SetActive(true);
            }

            this.voiceClipNames.Clear();
            if (voiceClipNames?.Count > 0)
            {
                this.voiceClipNames.AddRange(voiceClipNames);
                if (ObjectUtils.IsValid(PlayVoiceButton))
                    PlayVoiceButton.gameObject.SetActive(true);
            }
            else
            {
                if (ObjectUtils.IsValid(PlayVoiceButton))
                    PlayVoiceButton.gameObject.SetActive(false);
            }

            this.rollbackSpot = rollbackSpot;
            var canRollback = rollbackSpot.Valid && stateManager.CanRollbackTo(s => s.PlaybackSpot == rollbackSpot);
            if (ObjectUtils.IsValid(RollbackButton))
                RollbackButton.gameObject.SetActive(canRollback);
        }

        public virtual void Append (string text, string voiceClipName = null)
        {
            MessageText.text += text;

            if (!string.IsNullOrEmpty(voiceClipName))
            {
                voiceClipNames.Add(voiceClipName);
                if (ObjectUtils.IsValid(PlayVoiceButton))
                    PlayVoiceButton.gameObject.SetActive(true);
            }
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(messageText, actorNameText);
            audioManager = Engine.GetService<IAudioManager>();
            stateManager = Engine.GetService<IStateManager>();

            ActorNameText.text = null;
            ActorNameText.transform.parent.gameObject.SetActive(false);
            if (ObjectUtils.IsValid(PlayVoiceButton))
                PlayVoiceButton.gameObject.SetActive(false);
            if (ObjectUtils.IsValid(RollbackButton))
                RollbackButton.gameObject.SetActive(false);
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (ObjectUtils.IsValid(PlayVoiceButton))
                PlayVoiceButton.onClick.AddListener(HandlePlayVoiceButtonClicked);

            if (ObjectUtils.IsValid(RollbackButton))
                RollbackButton.onClick.AddListener(HandleRollbackButtonClicked);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (ObjectUtils.IsValid(PlayVoiceButton))
                PlayVoiceButton.onClick.RemoveListener(HandlePlayVoiceButtonClicked);

            if (ObjectUtils.IsValid(RollbackButton))
                RollbackButton.onClick.RemoveListener(HandleRollbackButtonClicked);
        }

        protected virtual async void HandlePlayVoiceButtonClicked ()
        {
            PlayVoiceButton.interactable = false;
            await audioManager.PlayVoiceSequenceAsync(voiceClipNames);
            PlayVoiceButton.interactable = true;
        }

        protected virtual async void HandleRollbackButtonClicked ()
        {
            RollbackButton.interactable = false;
            await stateManager.RollbackAsync(s => s.PlaybackSpot == rollbackSpot);
            RollbackButton.interactable = true;

            var backlogUI = GetComponentInParent<UI.IBacklogUI>();
            if (ObjectUtils.IsValid(backlogUI))
                backlogUI.Hide();
        }
    }
}
