﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    public class ScriptableLabeledButton : ScriptableUIControl<LabeledButton>
    {
        public event Action OnButtonClicked;

        public override bool Interactable => CanvasGroup ? base.Interactable : UIComponent.interactable;

        public override void SetInteractable (bool interactable)
        {
            if (CanvasGroup) base.SetInteractable(interactable);
            else UIComponent.interactable = interactable;
        }

        protected override void BindUIEvents ()
        {
            UIComponent.onClick.AddListener(OnButtonClick);
            UIComponent.onClick.AddListener(InvokeOnButtonClicked);
        }

        protected override void UnbindUIEvents ()
        {
            UIComponent.onClick.RemoveListener(OnButtonClick);
            UIComponent.onClick.RemoveListener(InvokeOnButtonClicked);
        }

        protected virtual void OnButtonClick () { }

        private void InvokeOnButtonClicked ()
        {
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke();
        }
    }
}
