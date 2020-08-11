// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// An implementation of <see cref="IManagedUI"/>, that
    /// can be used to create custom user managed UI objects.
    /// </summary>
    public class CustomUI : ScriptableUIBehaviour, IManagedUI
    {
        [System.Serializable]
        public class GameState
        {
            public bool Visible;
        }

        [System.Serializable]
        protected class FontChangeConfiguration
        {
            [Tooltip("The game object with a text component, which should be affected by font changes.")]
            public GameObject Object;
            [Tooltip("Whether to allow changing font of the text component.")]
            public bool AllowFontChange = true;
            [Tooltip("Whether to allow changing font size of the text component.")]
            public bool AllowFontSizeChange = true;
            [Tooltip("Sizes list should contain actual font sizes to apply for text component. Each element in the list corresponds to font size dropdown list index: Small -> 0, Default -> 1, Large -> 2, Extra Large -> 3 (can be changed via SettingsUI). Default value will be ignored and font size initially set in the prefab will be used instead.")]
            public List<int> FontSizes;
            [System.NonSerialized]
            public int DefaultSize;
            [System.NonSerialized]
            public Font DefaultFont;
            #if TMPRO_AVAILABLE
            [System.NonSerialized]
            public TMPro.TMP_FontAsset DefaultTMProFont;
            #endif
        }

        public bool HideOnLoad => hideOnLoad;
        public bool SaveVisibilityState => saveVisibilityState;
        public bool BlockInputWhenVisible => blockInputWhenVisible;
        public bool ModalUI => modalUI;

        protected virtual List<FontChangeConfiguration> FontChangeConfigurations => fontChangeConfiguration;
        protected virtual string[] AllowedSamplers => allowedSamplers;

        [Tooltip("Whether to automatically hide the UI when loading game or resetting state.")]
        [SerializeField] private bool hideOnLoad = true;
        [Tooltip("Whether to preserve visibility of the UI when saving/loading game.")]
        [SerializeField] private bool saveVisibilityState = true;
        [Tooltip("Whether to halt user input processing while the UI is visible.")]
        [SerializeField] private bool blockInputWhenVisible = false;
        [Tooltip("Which input samplers should still be allowed in case the input is blocked while the UI is visible.")]
        [SerializeField] private string[] allowedSamplers = default;
        [Tooltip("Whether to make all the other managed UIs not interactable while the UI is visible.")]
        [SerializeField] private bool modalUI = false;
        [Tooltip("Setup which game objects should be affected by font and text size changes (set in game settings).")]
        [SerializeField] private List<FontChangeConfiguration> fontChangeConfiguration = default;

        private IStateManager stateManager;
        private IInputManager inputManager;
        private IUIManager uiManager;

        public virtual UniTask InitializeAsync () => UniTask.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();

            stateManager = Engine.GetService<IStateManager>();
            inputManager = Engine.GetService<IInputManager>();
            uiManager = Engine.GetService<IUIManager>();

            InitializeFontChangeConfiguration();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (hideOnLoad)
            {
                stateManager.OnGameLoadStarted += HandleGameLoadStarted;
                stateManager.OnResetStarted += Hide;
            }

            stateManager.AddOnGameSerializeTask(SerializeState);
            stateManager.AddOnGameDeserializeTask(DeserializeState);

            if (blockInputWhenVisible)
                inputManager.AddBlockingUI(this, AllowedSamplers);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (hideOnLoad && stateManager != null)
            {
                stateManager.OnGameLoadStarted -= HandleGameLoadStarted;
                stateManager.OnResetStarted -= Hide;
            }

            if (stateManager != null)
            {
                stateManager.RemoveOnGameSerializeTask(SerializeState);
                stateManager.RemoveOnGameDeserializeTask(DeserializeState);
            }

            if (blockInputWhenVisible)
                inputManager?.RemoveBlockingUI(this);
        }

        /// <summary>
        /// Applies provided font to the <see cref="UnityEngine.UI.Text"/>
        /// and TMPro text components configured via <see cref="FontChangeConfigurations"/>.
        /// </summary>
        public virtual void SetFont (Font font)
        {
            if (FontChangeConfigurations is null || FontChangeConfigurations.Count == 0) return;

            foreach (var config in FontChangeConfigurations)
            {
                if (!config.AllowFontChange) continue;

                if (config.Object.TryGetComponent<UnityEngine.UI.Text>(out var text))
                    text.font = ObjectUtils.IsValid(font) ? font : config.DefaultFont;
                else
                {
                    #if TMPRO_AVAILABLE
                    if (!config.Object.TryGetComponent<TMPro.TextMeshProUGUI>(out var tmroComponent)) continue;
                    
                    if (!ObjectUtils.IsValid(font))
                    {
                        tmroComponent.font = config.DefaultTMProFont;
                        continue;
                    }

                    // TMPro requires font with a full path, while Unity doesn't store it by default; trying to guess it from the font name.
                    var fontPath = default(string);
                    var localFonts = Font.GetPathsToOSFonts();
                    for (int i = 0; i < localFonts.Length; i++)
                        if (localFonts[i].Replace("-", " ").Contains(font.name)) { fontPath = localFonts[i]; break; }
                    if (string.IsNullOrEmpty(fontPath)) continue;
                    var localFont = new Font(fontPath);
                    var fontAsset = TMPro.TMP_FontAsset.CreateFontAsset(localFont);
                    if (!ObjectUtils.IsValid(fontAsset)) continue;

                    var shader = tmroComponent.font.material.shader;
                    tmroComponent.font = fontAsset;
                    foreach (var mat in tmroComponent.fontMaterials)
                        mat.shader = shader; // Transfer custom material shaders to the new font.
                    #endif
                }
            }
        }

        /// <summary>
        /// Applies provided font size to the <see cref="UnityEngine.UI.Text"/>
        /// and TMPro text components configured via <see cref="FontChangeConfigurations"/>.
        /// </summary>
        public void SetFontSize (int dropdownIndex)
        {
            if (FontChangeConfigurations is null || FontChangeConfigurations.Count == 0) return;

            foreach (var config in FontChangeConfigurations)
            {
                if (!config.AllowFontSizeChange) continue;

                if (dropdownIndex != -1 && !config.FontSizes.IsIndexValid(dropdownIndex))
                {
                    Debug.LogError($"Failed to apply selected font size dropdown index (`{dropdownIndex}`) to `{gameObject.name}` UI: index is not available in `Font Sizes` list.");
                    continue;
                }

                var size = dropdownIndex == -1 ? config.DefaultSize : config.FontSizes[dropdownIndex];

                if (config.Object.TryGetComponent<UnityEngine.UI.Text>(out var text))
                    text.fontSize = size;
                else
                {
                    #if TMPRO_AVAILABLE
                    if (config.Object.TryGetComponent<TMPro.TextMeshProUGUI>(out var tmproText))
                        tmproText.fontSize = size;
                    #endif
                }
            }
        }

        protected virtual void SerializeState (GameStateMap stateMap)
        {
            if (saveVisibilityState)
            {
                var state = new GameState() {
                    Visible = Visible
                };
                stateMap.SetState(state, name);
            }
        }

        protected virtual UniTask DeserializeState (GameStateMap stateMap)
        {
            if (saveVisibilityState)
            {
                var state = stateMap.GetState<GameState>(name);
                if (state is null) return UniTask.CompletedTask;
                Visible = state.Visible;
            }
            return UniTask.CompletedTask;
        }

        protected override void HandleVisibilityChanged (bool visible)
        {
            base.HandleVisibilityChanged(visible);

            if (modalUI)
                uiManager?.SetModalUI(visible ? this : null);
        }

        protected virtual void InitializeFontChangeConfiguration ()
        {
            for (int i = 0; i < FontChangeConfigurations.Count; i++) // Store default fonts and sizes.
            {
                var item = FontChangeConfigurations[i];
                if (!ObjectUtils.IsValid(item.Object))
                {
                    Debug.LogError($"Failed to initialize font size list of `{gameObject.name}` UI: game object is missing.");
                    continue;
                }
                if (item.Object.TryGetComponent<UnityEngine.UI.Text>(out var text))
                {
                    item.DefaultSize = text.fontSize;
                    item.DefaultFont = text.font;
                }
                #if TMPRO_AVAILABLE
                if (item.Object.TryGetComponent<TMPro.TextMeshProUGUI>(out var tmproText))
                { 
                    item.DefaultSize = (int)tmproText.fontSize;
                    item.DefaultTMProFont = tmproText.font;
                }
                #endif
            }
        }

        private void HandleGameLoadStarted (GameSaveLoadArgs args) => Hide();
    }
}
