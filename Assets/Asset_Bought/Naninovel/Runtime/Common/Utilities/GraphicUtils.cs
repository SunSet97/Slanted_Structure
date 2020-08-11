﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public static class GraphicUtils
    {
        public static void SetOpacity (this Graphic graphic, float opacity)
        {
            Debug.Assert(graphic != null);
            var spriteColor = graphic.color;
            graphic.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, opacity);
        }

        public static bool IsTransparent (this Graphic graphic)
        {
            Debug.Assert(graphic != null);
            return Mathf.Approximately(graphic.color.a, 0f);
        }

        public static bool IsOpaque (this Graphic graphic)
        {
            Debug.Assert(graphic != null);
            return Mathf.Approximately(graphic.color.a, 1f);
        }
    }
}
