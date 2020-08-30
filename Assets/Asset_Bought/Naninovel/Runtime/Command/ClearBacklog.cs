// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UniRx.Async;

namespace Naninovel.Commands
{
    /// <summary>
    /// Removes all the messages from [printer backlog](/guide/text-printers.md#printer-backlog).
    /// </summary>
    /// <example>
    /// @clearBacklog
    /// </example>
    public class ClearBacklog : Command
    {
        public override UniTask ExecuteAsync (CancellationToken cancellationToken = default)
        {
            Engine.GetService<IUIManager>()?.GetUI<UI.IBacklogUI>()?.Clear();
            return UniTask.CompletedTask;
        }
    }
}
