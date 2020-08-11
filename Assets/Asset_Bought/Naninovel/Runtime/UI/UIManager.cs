// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="IUIManager"/>
    [InitializeAtRuntime]
    public class UIManager : IUIManager, IStatefulService<SettingsStateMap>
    {
        [Serializable]
        public class Settings
        {
            public string FontName = default;
            public int FontSize = -1;
        }

        private readonly struct ManagedUI
        {
            public readonly string Id;
            public readonly string PrefabName;
            public readonly GameObject GameObject;
            public readonly IManagedUI UIComponent;
            public readonly Type ComponentType;

            public ManagedUI (string prefabName, GameObject gameObject, IManagedUI uiComponent)
            {
                PrefabName = prefabName;
                GameObject = gameObject;
                UIComponent = uiComponent;
                ComponentType = UIComponent?.GetType();
                Id = $"{PrefabName}<{ComponentType.FullName}>";
            }
        }

        public UIConfiguration Configuration { get; }
        public string Font { get => ObjectUtils.IsValid(customFont) ? customFont.name : null; set => SetFont(value); }
        public int FontSize { get => fontSize; set => SetFontSize(value); }

        private const int defaultFontSize = 32; // used when creating dynamic fonts from OS fonts

        private readonly List<ManagedUI> managedUI = new List<ManagedUI>();
        private readonly Dictionary<Type, IManagedUI> cachedGetUIResults = new Dictionary<Type, IManagedUI>();
        private readonly Dictionary<IManagedUI, bool> modalState = new Dictionary<IManagedUI, bool>();
        private readonly ICameraManager cameraManager;
        private readonly IInputManager inputManager;
        private readonly IResourceProviderManager providersManager;
        private ResourceLoader<GameObject> loader;
        private Camera customCamera;
        private IInputSampler toggleUIInput;
        private Font customFont;
        private int fontSize = -1;

        public UIManager (UIConfiguration config, IResourceProviderManager providersManager, ICameraManager cameraManager, IInputManager inputManager)
        {
            Configuration = config;
            this.providersManager = providersManager;
            this.cameraManager = cameraManager;
            this.inputManager = inputManager;

            // Instatiating the UIs after the engine itialization so that UIs can use Engine API in Awake() and OnEnable() methods.
            Engine.AddPostInitializationTask(InstantiateUIsAsync);
        }

        public UniTask InitializeServiceAsync ()
        {
            loader = Configuration.Loader.CreateFor<GameObject>(providersManager);

            toggleUIInput = inputManager.GetToggleUI();
            if (toggleUIInput != null)
                toggleUIInput.OnStart += ToggleUI;

            return UniTask.CompletedTask;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            if (toggleUIInput != null)
                toggleUIInput.OnStart -= ToggleUI;

            foreach (var managedUI in managedUI)
            {
                if (ObjectUtils.IsValid(managedUI.GameObject))
                    ObjectUtils.DestroyOrImmediate(managedUI.GameObject);
            }
            managedUI.Clear();
            cachedGetUIResults.Clear();

            loader.UnloadAll();

            Engine.RemovePostInitializationTask(InstantiateUIsAsync);
        }

        public void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                FontName = Font,
                FontSize = FontSize
            };
            stateMap.SetState(settings);
        }

        public UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.GetState<Settings>() ?? new Settings();
            Font = string.IsNullOrEmpty(settings.FontName) ? null : settings.FontName;
            FontSize = settings.FontSize;

            return UniTask.CompletedTask;
        }

        public async UniTask<IManagedUI> InstantiatePrefabAsync (GameObject prefab)
        {
            var gameObject = Engine.Instantiate(prefab, prefab.name, Configuration.ObjectsLayer);

            if (!gameObject.TryGetComponent<IManagedUI>(out var uiComponent))
            {
                Debug.LogError($"Failed to instatiate `{prefab.name}` UI prefab: the prefab doesn't contain a `CustomUI` or `IManagedUI` component on the root object.");
                return null;
            }

            uiComponent.SortingOrder += Configuration.SortingOffset;
            uiComponent.RenderMode = Configuration.RenderMode;
            uiComponent.RenderCamera = ObjectUtils.IsValid(customCamera) ? customCamera : ObjectUtils.IsValid(cameraManager.UICamera) ? cameraManager.UICamera : cameraManager.Camera;

            if (ObjectUtils.IsValid(customFont))
                uiComponent.SetFont(customFont);
            uiComponent.SetFontSize(FontSize);

            var managedUI = new ManagedUI(prefab.name, gameObject, uiComponent);
            this.managedUI.Add(managedUI);

            await uiComponent.InitializeAsync();

            return uiComponent;
        }

        public T GetUI<T> () where T : class, IManagedUI => GetUI(typeof(T)) as T;

        public IManagedUI GetUI (Type type)
        {
            if (cachedGetUIResults.TryGetValue(type, out var cachedResult))
                return cachedResult;

            foreach (var managedUI in managedUI)
                if (type.IsAssignableFrom(managedUI.ComponentType))
                {
                    var result = managedUI.UIComponent;
                    cachedGetUIResults[type] = result;
                    return managedUI.UIComponent;
                }

            return null;
        }

        public IManagedUI GetUI (string prefabName)
        {
            foreach (var managedUI in managedUI)
                if (managedUI.PrefabName == prefabName)
                    return managedUI.UIComponent;
            return null;
        }

        public bool RemoveUI (IManagedUI managedUI)
        {
            if (!this.managedUI.Any(u => u.UIComponent == managedUI))
            {
                Debug.LogError("Failed to remove managed UI: provided instance not found. Make sure the UI was instatiated by UI Manager.");
                return false;
            }

            var ui = this.managedUI.FirstOrDefault(u => u.UIComponent == managedUI);
            this.managedUI.Remove(ui);
            foreach (var kv in cachedGetUIResults.ToList())
            {
                if (kv.Value == managedUI)
                    cachedGetUIResults.Remove(kv.Key);
            }

            if (ObjectUtils.IsValid(ui.GameObject))
                ObjectUtils.DestroyOrImmediate(ui.GameObject);

            return true;
        }

        public void SetRenderMode (RenderMode renderMode, Camera renderCamera)
        {
            customCamera = renderCamera;
            foreach (var managedUI in managedUI)
            {
                managedUI.UIComponent.RenderMode = renderMode;
                managedUI.UIComponent.RenderCamera = renderCamera;
            }
        }

        public void SetUIVisibleWithToggle (bool visible, bool allowToggle = true)
        {
            cameraManager.RenderUI = visible;

            var clickThroughPanel = GetUI<ClickThroughPanel>();
            if (ObjectUtils.IsValid(clickThroughPanel))
            {
                if (visible) clickThroughPanel.Hide();
                else
                {
                    if (allowToggle) clickThroughPanel.Show(true, ToggleUI, InputConfiguration.SubmitName, InputConfiguration.ToggleUIName, InputConfiguration.RollbackName);
                    else clickThroughPanel.Show(false, null, InputConfiguration.RollbackName);
                }
            }
        }

        public void SetModalUI (IManagedUI modalUI)
        {
            if (modalState.Count > 0) // Restore previous state.
            {
                foreach (var kv in modalState)
                    kv.Key.Interactable = kv.Value || (kv.Key is CustomUI customUI && customUI.ModalUI && customUI.Visible);
                modalState.Clear();
            }

            if (modalUI is null) return;

            foreach (var ui in managedUI)
            {
                modalState[ui.UIComponent] = ui.UIComponent.Interactable;
                ui.UIComponent.Interactable = false;
            }

            modalUI.Interactable = true;
        }

        private void SetFont (string fontName)
        {
            if (Font == fontName) return;

            if (string.IsNullOrEmpty(fontName))
            {
                customFont = null;
                foreach (var ui in managedUI)
                    ui.UIComponent.SetFont(customFont);
                return;
            }

            customFont = UnityEngine.Font.CreateDynamicFontFromOSFont(fontName, defaultFontSize);
            if (!ObjectUtils.IsValid(customFont))
            {
                Debug.LogError($"Failed to create `{fontName}` font.");
                return;
            }

            foreach (var ui in managedUI)
                ui.UIComponent.SetFont(customFont);
        }

        private void SetFontSize (int size)
        {
            if (fontSize == size) return;

            fontSize = size;

            foreach (var ui in managedUI)
                ui.UIComponent.SetFontSize(size);
        }

        private void ToggleUI () => SetUIVisibleWithToggle(!cameraManager.RenderUI);

        private async UniTask InstantiateUIsAsync ()
        {
            var resources = await loader.LoadAllAsync();
            var tasks = resources.Select(r => InstantiatePrefabAsync(r));
            await UniTask.WhenAll(tasks);
        }
    }
}
