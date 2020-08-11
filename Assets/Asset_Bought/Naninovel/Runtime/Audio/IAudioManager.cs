﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UniRx.Async;
using UnityEngine.Audio;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage the audio: SFX, BGM and voice.
    /// </summary>
    public interface IAudioManager : IEngineService<AudioConfiguration>
    {
        /// <summary>
        /// Audio mixer asset currently used by the service.
        /// Custom mixers can be assigned in the audio configuration.
        /// </summary>
        AudioMixer AudioMixer { get; }
        /// <summary>
        /// Current volume (in 0.0 to 1.0 range) of a master channel.
        /// </summary>
        float MasterVolume { get; set; }
        /// <summary>
        /// Current volume (in 0.0 to 1.0 range) of a BGM channel.
        /// </summary>
        float BgmVolume { get; set; }
        /// <summary>
        /// Current volume (in 0.0 to 1.0 range) of an SFX channel.
        /// </summary>
        float SfxVolume { get; set; }
        /// <summary>
        /// Current volume (in 0.0 to 1.0 range) of a voice channel.
        /// </summary>
        float VoiceVolume { get; set; }

        /// <summary>
        /// Whether a BGM track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        bool BgmPlaying (string path);
        /// <summary>
        /// Whether an SFX track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        bool SfxPlaying (string path);
        /// <summary>
        /// Whether a voice track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the voice resource.</param>
        bool VoicePlaying (string path);
        /// <summary>
        /// Returns currently played BGM track resource paths.
        /// </summary>
        IEnumerable<string> GetPlayedBgmPaths ();
        /// <summary>
        /// Returns currently played SFX track resource paths.
        /// </summary>
        IEnumerable<string> GetPlayedSfxPaths ();
        /// <summary>
        /// Returns currently played voice track resource path or null if not playing any.
        /// </summary>
        string GetPlayedVoicePath ();
        /// <summary>
        /// Checks whether an audio (BGM or SFX) resource with the provided path exists (can be loaded).
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        UniTask<bool> AudioExistsAsync (string path);
        /// <summary>
        /// Checks whether a voice resource with the provided path exists (can be loaded).
        /// </summary>
        /// <param name="path">Name (local path) of the voice resource.</param>
        UniTask<bool> VoiceExistsAsync (string path);
        /// <summary>
        /// Modifies properties of a BGM track with the provided resource path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="volume">Volume to set for the modified audio.</param>
        /// <param name="loop">Whether the audio should loop.</param>
        /// <param name="time">Animation (fade) time of the modification.</param>
        UniTask ModifyBgmAsync (string path, float volume, bool loop, float time, CancellationToken cancellationToken = default);
        /// <summary>
        /// Modifies properties of an SFX track with the provided resource path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="volume">Volume to set for the modified audio.</param>
        /// <param name="loop">Whether the audio should loop.</param>
        /// <param name="time">Animation (fade) time of the modification.</param>
        UniTask ModifySfxAsync (string path, float volume, bool loop, float time, CancellationToken cancellationToken = default);
        /// <summary>
        /// Will play an SFX track with the provided resource path if it's already loaded and won't save the state.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="volume">Volume of the audio playback.</param>
        /// <param name="group">Path of an <see cref="AudioMixerGroup"/> of the current <see cref="AudioMixer"/> to use when playing the audio.</param>
        /// <param name="restartIfPlaying">Whether to start playing the audio from start in case it's already playing.</param>
        void PlaySfxFast (string path, float volume = 1f, string group = default, bool restartIfPlaying = true);
        /// <summary>
        /// Starts playing a BGM track with the provided path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="volume">Volume of the audio playback.</param>
        /// <param name="fadeTime">Animation (fade-in) time to reach the target volume.</param>
        /// <param name="loop">Whether to loop the playback.</param>
        /// <param name="introPath">Name (local path) to an audio resource to play before the main audio; can be used as an intro before looping the main audio clip.</param>
        /// <param name="group">Path of an <see cref="AudioMixerGroup"/> of the current <see cref="AudioMixer"/> to use when playing the audio.</param>
        UniTask PlayBgmAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = true, string introPath = null, string group = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops playing a BGM track with the provided path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="fadeTime">Animation (fade-out) time to reach zero volume before stopping the playback.</param>
        UniTask StopBgmAsync (string path, float fadeTime = 0f, CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops playback of all the BGM tracks.
        /// </summary>
        /// <param name="fadeTime">Animation (fade-out) time to reach zero volume before stopping the playback.</param>
        UniTask StopAllBgmAsync (float fadeTime = 0f, CancellationToken cancellationToken = default);
        /// <summary>
        /// Starts playing an SFX track with the provided path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="volume">Volume of the audio playback.</param>
        /// <param name="fadeTime">Animation (fade-in) time to reach the target volume.</param>
        /// <param name="loop">Whether to loop the playback.</param>
        /// <param name="group">Path of an <see cref="AudioMixerGroup"/> of the current <see cref="AudioMixer"/> to use when playing the audio.</param>
        UniTask PlaySfxAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = false, string group = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops playing an SFX track with the provided path.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        /// <param name="fadeTime">Animation (fade-out) time to reach zero volume before stopping the playback.</param>
        UniTask StopSfxAsync (string path, float fadeTime = 0f, CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops playback of all the SFX tracks.
        /// </summary>
        /// <param name="fadeTime">Animation (fade-out) time to reach zero volume before stopping the playback.</param>
        UniTask StopAllSfxAsync (float fadeTime = 0f, CancellationToken cancellationToken = default);
        /// <summary>
        /// Starts playing an SFX track with the provided path.
        /// </summary>
        /// <param name="path">Name (local path) of the voice resource.</param>
        /// <param name="volume">Volume of the voice playback.</param>
        /// <param name="group">Path of an <see cref="AudioMixerGroup"/> of the current <see cref="AudioMixer"/> to use when playing the voice.</param>
        UniTask PlayVoiceAsync (string path, float volume = 1f, string group = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Plays voice clips with the provided resource paths in sequence.
        /// </summary>
        /// <param name="pathList">Names (local paths) of the voice resources.</param>
        /// <param name="volume">Volume of the voice playback.</param>
        /// <param name="group">Path of an <see cref="AudioMixerGroup"/> of the current <see cref="AudioMixer"/> to use when playing the voice.</param>
        UniTask PlayVoiceSequenceAsync (List<string> pathList, float volume = 1f, string group = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops playing a voice track with the provided path.
        /// </summary>
        void StopVoice ();
        /// <summary>
        /// Returns <see cref="IAudioTrack"/> assoicated with a playing audio (SFX or BGM) resource 
        /// with the provided path; returns null if not found or the audio is not currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        IAudioTrack GetAudioTrack (string path);
        /// <summary>
        /// Returns <see cref="IAudioTrack"/> assoicated with a playing voice resource 
        /// with the provided path; returns null if not found or the voice is not currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the voice resource.</param>
        IAudioTrack GetVoiceTrack (string path);
        /// <summary>
        /// Returns current voice volume (in 0.0 to 1.0 range) of a printed message author (character actor) with the provided ID.
        /// When volume for an author with the specified ID not specified, returns -1.
        /// </summary>
        float GetAuthorVolume (string authorId);
        /// <summary>
        /// Sets current voice volume (in 0.0 to 1.0 range) of a printed message author (character actor) with the provided ID.
        /// </summary>
        void SetAuthorVolume (string authorId, float volume);

        /// <summary>
        /// Preloads and holds resources required to play an audio (SFX or BGM) with the provided path.
        /// </summary>
        UniTask HoldAudioResourcesAsync (object holder, string path);
        /// <summary>
        /// Releases resources required to play an audio (SFX or BGM) with the provided path.
        /// </summary>
        void ReleaseAudioResources (object holder, string path);
        /// <summary>
        /// Preloads and holds resources required to play a voice with the provided path.
        /// </summary>
        UniTask HoldVoiceResourcesAsync (object holder, string path);
        /// <summary>
        /// Releases resources required to play a voice with the provided path.
        /// </summary>
        void ReleaseVoiceResources (object holder, string path);
    }
}
