﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    /// <summary>
    /// Manages <see cref="AudioTrack"/> objects.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        public AudioListener Listener => listenerCache ? listenerCache : FindOrAddListener();
        public float Volume { get => AudioListener.volume; set => AudioListener.volume = value; }
        public bool Mute { get => AudioListener.pause; set => AudioListener.pause = value; }

        private readonly Dictionary<AudioClip, AudioTrack> audioTracks = new Dictionary<AudioClip, AudioTrack>();
        private readonly Stack<AudioSource> sourcesPool = new Stack<AudioSource>();
        private AudioListener listenerCache;
        private Tweener<FloatTween> listenerVolumeTweener;

        private void Awake ()
        {
            listenerVolumeTweener = new Tweener<FloatTween>();
            FindOrAddListener();
        }

        /// <summary>
        /// Sets transform of the current <see cref="Listener"/> as a child of the provided target.
        /// </summary>
        public void AttachListener (Transform target)
        {
            Listener.transform.SetParent(target);
            Listener.transform.localPosition = Vector3.zero;
        }

        public void FadeVolume (float volume, float time)
        {
            if (listenerVolumeTweener.Running)
                listenerVolumeTweener.CompleteInstantly();

            var tween = new FloatTween(Volume, volume, time, value => Volume = value, ignoreTimeScale: true, target: this);
            listenerVolumeTweener.Run(tween);
        }

        public bool ClipPlaying (AudioClip clip)
        {
            if (!clip) return false;
            return audioTracks.ContainsKey(clip) && audioTracks[clip].Playing;
        }

        public void PlayClip (AudioClip clip, AudioSource audioSource = null, float volume = 1f, 
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null)
        {
            if (!clip) return;

            if (audioTracks.ContainsKey(clip)) StopClip(clip);
            PoolUnusedSources();

            // In case user somehow provided one of our pooled sources, don't use it.
            if (audioSource && IsOwnedByController(audioSource)) audioSource = null;
            if (!audioSource) audioSource = GetPooledSource();

            var track = new AudioTrack(clip, audioSource, volume, loop, mixerGroup, introClip);
            audioTracks.Add(clip, track);
            track.Play();
        }

        public async UniTask PlayClipAsync (AudioClip clip, float fadeInTime, AudioSource audioSource = null, float volume = 1f,
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, CancellationToken cancellationToken = default)
        {
            if (!clip) return;

            if (audioTracks.ContainsKey(clip)) StopClip(clip);
            PoolUnusedSources();

            // In case user somehow provided one of our pooled sources, don't use it.
            if (audioSource && IsOwnedByController(audioSource)) audioSource = null;
            if (!audioSource) audioSource = GetPooledSource();

            var track = new AudioTrack(clip, audioSource, volume, loop, mixerGroup, introClip);
            audioTracks.Add(clip, track);
            await track.PlayAsync(fadeInTime, cancellationToken);
        }

        public void StopClip (AudioClip clip)
        {
            if (!clip || !ClipPlaying(clip)) return;
            GetTrack(clip).Stop();
        }

        public void StopAllClips ()
        {
            foreach (var track in audioTracks.Values)
                track.Stop();
        }

        public async UniTask StopClipAsync (AudioClip clip, float fadeOutTime, CancellationToken cancellationToken = default)
        {
            if (!clip || !ClipPlaying(clip)) return;
            await GetTrack(clip).StopAsync(fadeOutTime, cancellationToken);
        }

        public async UniTask StopAllClipsAsync (float fadeOutTime, CancellationToken cancellationToken = default)
        {
            await UniTask.WhenAll(audioTracks.Values.Select(t => t.StopAsync(fadeOutTime, cancellationToken)));
        }

        public AudioTrack GetTrack (AudioClip clip)
        {
            if (!clip) return null;
            return audioTracks.ContainsKey(clip) ? audioTracks[clip] : null;
        }

        private AudioListener FindOrAddListener ()
        {
            listenerCache = FindObjectOfType<AudioListener>();
            if (!listenerCache) listenerCache = gameObject.AddComponent<AudioListener>();
            return listenerCache;
        }

        private bool IsOwnedByController (AudioSource audioSource)
        {
            return GetComponents<AudioSource>().Contains(audioSource);
        }

        private AudioSource GetPooledSource ()
        {
            if (sourcesPool.Count > 0) return sourcesPool.Pop();
            return gameObject.AddComponent<AudioSource>();
        }

        private void PoolUnusedSources ()
        {
            foreach (var track in audioTracks.Values.ToList())
                if (!track.Playing)
                {
                    if (IsOwnedByController(track.Source))
                        sourcesPool.Push(track.Source);
                    audioTracks.Remove(track.Clip);
                }
        }
    }
}
