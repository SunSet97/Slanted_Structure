﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UniRx.Async;
using Naninovel.UI;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IManagedUI"/>.
    /// </summary>
    public static class ManagedUIExtensions
    {
        /// <summary>
        /// Shows the UI gradually over default duration by invoking <see cref="IManagedUI.ChangeVisibilityAsync(bool, float?)"/> in "fire and forget" fashion.
        /// </summary>
        public static void Show (this IManagedUI ui) => ui.ChangeVisibilityAsync(true).Forget();

        /// <summary>
        /// Hides the UI gradually over default duration by invoking <see cref="IManagedUI.ChangeVisibilityAsync(bool, float?)"/> in "fire and forget" fashion.
        /// </summary>
        public static void Hide (this IManagedUI ui) => ui.ChangeVisibilityAsync(false).Forget();
    }
}
