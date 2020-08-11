﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class ControlPanelSkipButton : ScriptableLabeledButton
    {
        [SerializeField] private Color activeColorMultiplier = Color.red;

        private IScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<IScriptPlayer>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();
            player.OnSkip += HandleSkipModeChange;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            player.OnSkip -= HandleSkipModeChange;
        }

        protected override void OnButtonClick ()
        {
            player.SetSkipEnabled(!player.SkipActive);
        }

        private void HandleSkipModeChange (bool enabled)
        {
            UIComponent.LabelColorMultiplier = enabled ? activeColorMultiplier : Color.white;
        }
    } 
}
