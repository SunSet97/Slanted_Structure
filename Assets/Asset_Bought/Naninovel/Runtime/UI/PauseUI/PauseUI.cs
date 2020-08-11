﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.UI
{
    /// <inheritdoc cref="IPauseUI"/>
    public class PauseUI : CustomUI, IPauseUI 
    {
        private IInputSampler pauseInput;

        protected override void Awake ()
        {
            base.Awake();

            pauseInput = Engine.GetService<IInputManager>()?.GetPause();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (pauseInput != null)
                pauseInput.OnStart += ToggleVisibility;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (pauseInput != null)
                pauseInput.OnStart -= ToggleVisibility;
        }
    }
}
