﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// The mode in which <see cref="IScriptPlayer"/> should handle commands skipping.
    /// </summary>
    public enum PlayerSkipMode
    {
        /// <summary>
        /// Skip only the commands that has already been executed.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// Skip all commands.
        /// </summary>
        Everything
    }
}
