﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a single line in a <see cref="Script"/>.
    /// </summary>
    [System.Serializable]
    public abstract class ScriptLine
    {
        /// <summary>
        /// Name of the naninovel script to which the line belongs.
        /// </summary>
        public string ScriptName => scriptName;
        /// <summary>
        /// Index of the line in naninovel script.
        /// </summary>
        public int LineIndex => lineIndex;
        /// <summary>
        /// Number of the line in naninovel script (index + 1).
        /// </summary>
        public int LineNumber => LineIndex + 1;
        /// <summary>
        /// Persistent hash code of the original text line (before any define replacements).
        /// </summary>
        public string LineHash => lineHash;

        [SerializeField] private string scriptName = default;
        [SerializeField] private int lineIndex = -1;
        [SerializeField] private string lineHash = default;

        /// <summary>
        /// Creates new instance by parsing the provided serialized script line text.
        /// </summary>
        /// <param name="scriptName">Name of the script asset which contains the line.</param>
        /// <param name="lineIndex">Index of the line in naninovel script.</param>
        /// <param name="lineText">The script line text to parse.</param>
        /// <param name="errors">When provided and an error occurs while parsing the line, will add the error to the list.</param>
        public ScriptLine (string scriptName, int lineIndex, string lineText, List<ScriptParseError> errors = null)
        {
            this.scriptName = scriptName;
            this.lineIndex = lineIndex;
            this.lineHash = CryptoUtils.PersistentHexCode(lineText.TrimFull());

            ParseLineText(lineText, out var error);
            if (error != null)
            {
                var errorData = new ScriptParseError(this, lineText, error);
                errors?.Add(errorData);
                Debug.LogError(errorData);
            }
        }

        /// <summary>
        /// Performs parsing of the script line text initializing the instance; invoked on construction.
        /// </summary>
        /// <param name="lineText">The text to parse.</param>
        /// <param name="error">Parse error description (if an error occured) or null when the parse has succeeded.</param>
        protected abstract void ParseLineText (string lineText, out string error);
    } 
}
