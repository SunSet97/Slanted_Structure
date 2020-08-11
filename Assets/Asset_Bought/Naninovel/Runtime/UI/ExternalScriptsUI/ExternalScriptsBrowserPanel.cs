// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UniRx.Async;

namespace Naninovel.UI
{
    public class ExternalScriptsBrowserPanel : ScriptNavigatorPanel, IExternalScriptsUI
    {
        protected override async UniTask LoadScriptsAsync ()
        {
            var scripts = await ScriptManager.LoadExternalScriptsAsync();
            GenerateScriptButtons(scripts);
        }
    } 
}
