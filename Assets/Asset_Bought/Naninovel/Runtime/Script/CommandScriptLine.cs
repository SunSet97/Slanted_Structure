// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing a <see cref="Commands.Command"/>.
    /// </summary>
    [System.Serializable]
    public class CommandScriptLine : ScriptLine
    {
        /// <summary>
        /// Literal used to identify this type of lines.
        /// </summary>
        public const string IdentifierLiteral = "@";
        /// <summary>
        /// The command which this line represents.
        /// </summary>
        public Command Command => command;

        [SerializeReference] private Command command = default;

        /// <inheritdoc/>
        public CommandScriptLine (string scriptName, int lineIndex, string lineText, List<ScriptParseError> errors = null)
            : base(scriptName, lineIndex, lineText, errors) { }

        protected override void ParseLineText (string lineText, out string error)
        {
            var commandText = lineText.GetAfterFirst(IdentifierLiteral);
            command = Command.FromScriptText(ScriptName, LineIndex, 0, commandText, out error);
        }
    }
}
