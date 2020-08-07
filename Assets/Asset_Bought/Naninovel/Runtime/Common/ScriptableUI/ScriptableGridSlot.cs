﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.EventSystems;

namespace Naninovel
{
    [RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ScriptableGridSlot : ScriptableButton, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    { 
        public class Constructor<TSlot> where TSlot : ScriptableGridSlot
        {
            public readonly TSlot ConstructedSlot;

            public Constructor (TSlot prototype, string id, OnClicked onClicked = null)
            {
                ConstructedSlot = Instantiate(prototype);
                ConstructedSlot.Id = id;
                ConstructedSlot.onClickedAction = onClicked;
            }
        }

        /// <summary>
        /// Action to invoke when the slot body is clicked by the user.
        /// </summary>
        /// <param name="slotId">ID of the clicked slot.</param>
        public delegate void OnClicked (string slotId);

        public string Id { get; private set; }
        public int NumberInGrid => transform.GetSiblingIndex() + 1;

        [SerializeField] private float hoverOpacityFade = .25f;

        private Tweener<FloatTween> fadeTweener;
        private OnClicked onClickedAction;

        protected override void Awake ()
        {
            base.Awake();

            fadeTweener = new Tweener<FloatTween>();
        }

        protected override void Start ()
        {
            base.Start();

            SetOpacity(1 - hoverOpacityFade);
        }

        public virtual void OnPointerEnter (PointerEventData eventData) => FadeInSlot();

        public virtual void OnPointerExit (PointerEventData eventData) => FadeOutSlot();

        public virtual void OnSelect (BaseEventData eventData) => FadeInSlot();

        public virtual void OnDeselect (BaseEventData eventData) => FadeOutSlot();

        protected override void OnButtonClick ()
        {
            base.OnButtonClick();

            onClickedAction?.Invoke(Id);
        }

        protected virtual void FadeInSlot ()
        {
            if (fadeTweener.Running) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(Opacity, 1f, FadeTime, SetOpacity, true, target: this);
            fadeTweener.Run(tween);
        }

        protected virtual void FadeOutSlot ()
        {
            if (fadeTweener.Running) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(Opacity, 1f - hoverOpacityFade, FadeTime, SetOpacity, true, target: this);
            fadeTweener.Run(tween);
        }
    }
}
