// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represent a <see cref="ScriptLine"/> parse error.
    /// </summary>
    public readonly struct ScriptParseError
    {
        /// <summary>
        /// Name of the naninovel script to which the line belongs.
        /// </summary>
        public readonly string ScriptName;
        /// <summary>
        /// Index of the line in naninovel script.
        /// </summary>
        public readonly int LineIndex;
        /// <summary>
        /// Number of the line in naninovel script (index + 1).
        /// </summary>
        public int LineNumber => LineIndex + 1;
        /// <summary>
        /// Original text representation of the line.
        /// </summary>
        public readonly string LineText;
        /// <summary>
        /// Description of the parse error.
        /// </summary>
        public readonly string ErrorDescription;

        public ScriptParseError (string scriptName, int lineIndex, string lineText, string errorDescription)
        {
            ScriptName = scriptName;
            LineIndex = lineIndex;
            LineText = lineText;
            ErrorDescription = errorDescription;
        }

        public ScriptParseError (ScriptLine line, string lineText, string errorDescription) 
            : this(line.ScriptName, line.LineIndex, lineText, errorDescription) { }

        public override string ToString () => $"Error parsing `{ScriptName}` script at line #{LineNumber}{(ErrorDescription == string.Empty ? "." : $": {ErrorDescription}")}";
    }
}