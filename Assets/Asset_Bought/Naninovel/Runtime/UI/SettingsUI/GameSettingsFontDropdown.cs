// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsFontDropdown : ScriptableDropdown
    {
        private const string defaultFontName = "Default";
        
        private IUIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<IUIManager>();
        }

        protected override void Start ()
        {
            base.Start();

            var systemFonts = Font.GetOSInstalledFontNames().ToList();
            systemFonts.Insert(0, defaultFontName);
            InitializeOptions(systemFonts);
        }

        protected override void OnValueChanged (int value)
        {
            var fontName = UIComponent.options[value].text;
            uiManager.Font = fontName == defaultFontName ? null : fontName;
        }

        private void InitializeOptions (List<string> availableOptions)
        {
            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableOptions);
            UIComponent.value = GetCurrentIndex();
            UIComponent.RefreshShownValue();
        }

        private int GetCurrentIndex ()
        {
            if (string.IsNullOrEmpty(uiManager.Font))
                return 0;
            var option = UIComponent.options.Where(o => o.text == uiManager.Font).FirstOrDefault();
            return UIComponent.options.IndexOf(option);
        }
    }
}
