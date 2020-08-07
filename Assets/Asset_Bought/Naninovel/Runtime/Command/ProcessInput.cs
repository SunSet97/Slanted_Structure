﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UniRx.Async;

namespace Naninovel.Commands
{
    /// <summary>
    /// Allows halting and resuming user input processing (eg, reacting to pressing keyboard keys).
    /// The effect of the action is persistent and saved with the game.
    /// </summary>
    /// <example>
    /// ; Halt input processing of all the samplers
    /// @processInput false
    /// 
    /// ; Resume input processing of all the samplers
    /// @processInput true
    /// 
    /// ; Mute `Rollback` and `Pause` inputs and un-mute `Continue` input
    /// @processInput set:Rollback.false,Pause.false,Continue.true
    /// </example>
    public class ProcessInput : Command
    {
        /// <summary>
        /// Whether to enable input processing of all the samplers.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias)]
        public BooleanParameter InputEnabled;
        /// <summary>
        /// Allows muting and un-muting individual input samplers.
        /// </summary>
        [ParameterAlias("set")]
        public NamedBooleanListParameter SetEnabled;

        public override UniTask ExecuteAsync (CancellationToken cancellationToken = default)
        {
            if (!Assigned(InputEnabled) && !Assigned(SetEnabled))
            {
                LogWarningWithPosition($"No parameters were specified in `@processInput`; command won't have any effect.");
                return UniTask.CompletedTask;
            }

            var inputManager = Engine.GetService<IInputManager>();

            if (Assigned(InputEnabled))
                inputManager.ProcessInput = InputEnabled;

            if (Assigned(SetEnabled))
            {
                foreach (var kv in SetEnabled)
                {
                    if (!kv.HasValue || !kv.NamedValue.HasValue)
                    {
                        LogErrorWithPosition($"An invalid item in `set` parameter detected in `@processInput` command. Make sure all items have both name and value specified.");
                        continue;
                    }

                    var sampler = inputManager.GetSampler(kv.Name);
                    if (sampler is null)
                    {
                        LogErrorWithPosition($"`{kv.Name}` input sampler wasn't found while executing `@processInput` command. Make sure a binding with that name exist in the input configuration.");
                        continue;
                    }
                    sampler.Enabled = kv.NamedValue;
                }
            }

            return UniTask.CompletedTask;
        }
    }
}
