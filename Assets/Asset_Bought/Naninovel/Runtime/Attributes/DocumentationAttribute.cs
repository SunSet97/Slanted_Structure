﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Used by Naninovel editor tools to extract documentation from custom types (eg, commands).
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class DocumentationAttribute : Attribute
    {
        public readonly string Summary;
        public readonly string Remarks;

        public DocumentationAttribute (string summary, string remarks = null)
        {
            Summary = summary;
            Remarks = remarks;
        }
    }
}
