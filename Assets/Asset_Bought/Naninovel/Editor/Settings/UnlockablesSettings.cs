﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEditor;

namespace Naninovel
{
    public class UnlockablesSettings : ResourcefulSettings<UnlockablesConfiguration>
    {
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override string ResourcesSelectionTooltip => "In naninovel scripts use `@unlock %name%` to unlock or `@lock %name%` to lock selected unlockable item.";

        [MenuItem("Naninovel/Resources/Unlockables")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
