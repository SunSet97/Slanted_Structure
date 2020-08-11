﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Linq;
using UniRx.Async;

namespace Naninovel.Commands
{
    /// <summary>
    /// Navigates naninovel script playback to the provided path.
    /// When the path leads to another (not the currently played) naninovel script, will also [reset state](/api/#resetstate) 
    /// before loading the target script, unless [ResetStateOnLoad](https://naninovel.com/guide/configuration.html#state) is disabled in the configuration.
    /// </summary>
    /// <example>
    /// ; Loads and starts playing a naninovel script with the name `Script001` from the start.
    /// @goto Script001
    /// 
    /// ; Save as above, but start playing from the label `AfterStorm`.
    /// @goto Script001.AfterStorm
    /// 
    /// ; Navigates the playback to the label `Epilogue` in the currently played script.
    /// @goto .Epilogue
    /// 
    /// ; Load Script001, but don't reset audio manager (any playing audio won't be interrupted).
    /// ; Be aware, that excluding a service form state reset will leave related resources in memory.
    /// @goto Script001 reset:IAudioManager
    /// </example>
    public class Goto : Command, Command.IForceWait
    {
        /// <summary>
        /// When applied to an <see cref="IEngineService"/> implementation, the service won't be reset
        /// while executing the goto command and <see cref="StateConfiguration.ResetOnGoto"/> is enabled.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class DontResetAttribute : Attribute { }

        /// <summary>
        /// When assigned to <see cref="ResetState"/>, forces reset of all the services, 
        /// except the ones with <see cref="DontResetAttribute"/>.
        /// </summary>
        public const string ResetAllFlag = "*";
        /// <summary>
        /// When assigned to <see cref="ResetState"/>, forces no reset.
        /// </summary>
        public const string NoResetFlag = "-";

        /// <summary>
        /// Path to navigate into in the following format: `ScriptName.LabelName`.
        /// When label name is ommited, will play provided script from the start.
        /// When script name is ommited, will attempt to find a label in the currently played script.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public NamedStringParameter Path;
        /// <summary>
        /// When specified, will control whether to reset the engine services state before loading a script (in case the path is leading to another script):<br/>
        /// - Specify `*` to reset all the services, except the ones with `DontResetAttribute`.<br/>
        /// - Specify service type names (separated by comma) to exclude from reset; all the other services will be reset, including the ones with `DontResetAttribute`.<br/>
        /// - Specify `-` to force no reset (even if it's enabled by default in the configuration).
        /// </summary>
        [ParameterAlias("reset")]
        public StringListParameter ResetState;

        private static readonly Type[] dontResetTypes = ReflectionUtils.ExportedDomainTypes.Where(t => t.IsDefined(typeof(DontResetAttribute), false)).ToArray();

        public override async UniTask ExecuteAsync (CancellationToken cancellationToken = default)
        {
            var player = Engine.GetService<IScriptPlayer>();

            var scriptName = Path.Name;
            var label = Path.NamedValue;

            if (string.IsNullOrWhiteSpace(scriptName) && !ObjectUtils.IsValid(player.PlayedScript))
            {
                LogErrorWithPosition($"Failed to execute `@goto` command: script name is not specified and no script is currently played.");
                return;
            }

            // Just navigate to a label inside current script.
            if (string.IsNullOrWhiteSpace(scriptName) || (ObjectUtils.IsValid(player.PlayedScript) && scriptName.EqualsFastIgnoreCase(player.PlayedScript.Name)))
            {
                if (!player.PlayedScript.LabelExists(label))
                {
                    LogErrorWithPosition($"Failed navigating script playback to `{label}` label: label not found in `{player.PlayedScript.Name}` script.");
                    return;
                }
                var startLineIndex = player.PlayedScript.GetLineIndexForLabel(label);
                player.Play(player.PlayedScript, startLineIndex);
                return;
            }

            // Load another script and start playing from label.
            var stateManager = Engine.GetService<IStateManager>();

            // Reset is not requested, but configured to reset by default; or reset `all` is requested.
            if ((!Assigned(ResetState) && stateManager.Configuration.ResetOnGoto) || (Assigned(ResetState) && ResetState.Length == 1 && ResetState[0] == ResetAllFlag))
            {
                await stateManager.ResetStateAsync(
                    dontResetTypes,
                    () => player.PreloadAndPlayAsync(scriptName, label: label));
                return;
            }

            // Requested reset excluding specific services.
            if (Assigned(ResetState) && ResetState.Length > 0 && ResetState[0] != NoResetFlag)
            {
                await stateManager.ResetStateAsync(ResetState, () => player.PreloadAndPlayAsync(scriptName, label: label));
                return;
            }

            // No reset is needed, just loading the script.
            await player.PreloadAndPlayAsync(scriptName, label: label);
        }
    } 
}
