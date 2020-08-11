// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsFontSizeDropdown : ScriptableDropdown
    {
        [ManagedText("DefaultUI")]
        protected static string Small = "Small";
        [ManagedText("DefaultUI")]
        protected static string Default = "Default";
        [ManagedText("DefaultUI")]
        protected static string Large = "Large";
        [ManagedText("DefaultUI")]
        protected static string ExtraLarge = "Extra Large";

        [Tooltip("Index of the dropdown list associated with default font size.")]
        [SerializeField] private int defaultSizeIndex = 1;

        private IUIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<IUIManager>();
        }

        protected override void Start ()
        {
            base.Start();

            InitializeOptions();
        }

        protected override void OnValueChanged (int index)
        {
            base.OnValueChanged(index);

            uiManager.FontSize = index == defaultSizeIndex ? -1 : index;
        }

        private void InitializeOptions ()
        {
            var options = new List<string> { Small, Default, Large, ExtraLarge };
            UIComponent.ClearOptions();
            UIComponent.AddOptions(options);

            var index = uiManager.FontSize == -1 ? defaultSizeIndex : uiManager.FontSize;
            if (!UIComponent.options.IsIndexValid(index))
            {
                Debug.LogError($"Failed to initialize font size dropdown: current index `{index}` is not available in `Font Sizes` list.");
                return;
            }
            UIComponent.value = index;
            UIComponent.RefreshShownValue();
        }
    }
}
