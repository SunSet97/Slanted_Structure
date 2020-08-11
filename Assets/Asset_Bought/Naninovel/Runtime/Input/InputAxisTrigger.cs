﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public class InputAxisTrigger : IEquatable<InputAxisTrigger>
    {
        [Tooltip("Name of the axis.")]
        public string AxisName = string.Empty;
        [Tooltip("Whether trigger should happen when axis value is positive, negative or both.")]
        public InputAxisTriggerMode TriggerMode = InputAxisTriggerMode.Both;
        [Tooltip("When axis value is below or equal to this value, the trigger won't be activated."), Range(0, .999f)]
        public float TriggerTolerance = .001f;

        /// <summary>
        /// Returns the current axis value when it's above the trigger tolerance; zero otherwise.
        /// </summary>
        public float Sample ()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            if (string.IsNullOrEmpty(AxisName)) return 0;

            var value = Input.GetAxis(AxisName);

            if (TriggerMode == InputAxisTriggerMode.Positive && value <= 0) return 0;
            if (TriggerMode == InputAxisTriggerMode.Negative && value >= 0) return 0;

            if (Mathf.Abs(value) < TriggerTolerance) return 0;

            return value;
            #else
            return 0;
            #endif
        }

        public override bool Equals (object obj)
        {
            return Equals(obj as InputAxisTrigger);
        }

        public bool Equals (InputAxisTrigger other)
        {
            return other != null &&
                   AxisName == other.AxisName &&
                   TriggerMode == other.TriggerMode;
        }

        public override int GetHashCode ()
        {
            var hashCode = 1471448403;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AxisName);
            hashCode = hashCode * -1521134295 + TriggerMode.GetHashCode();
            return hashCode;
        }

        public static bool operator == (InputAxisTrigger trigger1, InputAxisTrigger trigger2)
        {
            return EqualityComparer<InputAxisTrigger>.Default.Equals(trigger1, trigger2);
        }

        public static bool operator != (InputAxisTrigger trigger1, InputAxisTrigger trigger2)
        {
            return !(trigger1 == trigger2);
        }
    }
}
