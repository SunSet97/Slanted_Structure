﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    public class LineTextField : TextField
    {
        private static readonly FieldUndoData undoData;

        private readonly VisualElement inputField;

        static LineTextField ()
        {
            undoData = ScriptableObject.CreateInstance<FieldUndoData>();
            undoData.hideFlags = HideFlags.HideAndDontSave;
        }

        public LineTextField (string label = default, string value = default)
        {
            //isDelayed = true; // Bad UX when adding generic text lines: it's required to de-focus them in order to save the changes.

            this.label = label;
            this.value = value;

            labelElement.name = "InputLabel";
            inputField = this.Q<VisualElement>(textInputUssName);
            inputField.RegisterCallback<MouseDownEvent>(HandleFieldMouseDown);
            AddToClassList("Inlined");

            undoData.BindField(this);
        }

        public static void ResetPerScriptStaticData ()
        {
            undoData.ResetBindings();
        }

        private void HandleFieldMouseDown (MouseDownEvent evt)
        {
            if (focusController.focusedElement == this)
                return; // Prevent do-focusing the field on consequent clicks.

            // Propagate the event to the parent.
            var newEvt = MouseDownEvent.GetPooled(evt);
            newEvt.target = this;
            SendEvent(newEvt);
        }
    }
}
