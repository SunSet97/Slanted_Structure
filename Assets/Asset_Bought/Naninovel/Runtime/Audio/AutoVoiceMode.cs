// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

namespace Naninovel
{
    /// <summary>
    /// Represent available methods to associate voice clips with @print commands,
    /// when using <see cref="AudioConfiguration.EnableAutoVoicing"/>.
    /// </summary>
    public enum AutoVoiceMode
    {
        /// <summary>
        /// Voice clips are associated by <see cref="Commands.Command.PlaybackSpot"/> of the @print commands.
        /// </summary>
        PlaybackSpot,
        /// <summary>
        /// Voice clips are associated by <see cref="Commands.PrintText.AutoVoiceId"/>, 
        /// <see cref="Commands.PrintText.AuthorId"/> and <see cref="Commands.PrintText.Text"/>.
        /// </summary>
        ContentHash
    }
}