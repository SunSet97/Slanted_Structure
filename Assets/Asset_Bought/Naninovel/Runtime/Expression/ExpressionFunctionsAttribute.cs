﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When assigned to a static class, the methods of the class will be added to the available expression functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ExpressionFunctionsAttribute : Attribute { }
}
