using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Audio
{
    public class AudioController : MonoBehaviour
    {
        private static AudioController _instance;
        public static AudioController Instance
        {
            get
            {
                if (!_instance)
                {
                    var obj = FindObjectOfType<AudioController>();
                    if (obj)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Resources.Load<AudioController>("AudioController");   
                    }
                    DontDestroyOnLoad(_instance);
                    _instance.audioClips = new List<AudioClip>();
                }
                return _instance;
            }
        }

        private List<AudioClip> audioClips;

        [SerializeField]
        private AudioSource bgmSource;
        [SerializeField]
        private AudioSource sfxSource;

        public void PlayBgm(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                bgmSource.clip = null;
                bgmSource.Stop();
                return;
            }
        
            if (!audioClip.Equals(bgmSource.clip))
            {
                bgmSource.clip = audioClip;
                bgmSource.Play();   
            }
        }

        public void StopBgm()
        {
            bgmSource.clip = null;
            bgmSource.Stop();
        }

        private void PlayOneShot(AudioClip audioClip)
        {
            if (!audioClips.Contains(audioClip))
            {
                audioClips.Add(audioClip);
            }
            sfxSource.PlayOneShot(audioClip);
        }
        
        public void PlayOneShot(string audioClipName)
        {
            if (String.IsNullOrEmpty(audioClipName))
            {
                return;
            }
            var audioClip = audioClips.Find(item => item.name == audioClipName);
            if (!audioClip)
            {
                audioClip = Resources.Load<AudioClip>(audioClipName);
                audioClips.Add(audioClip);
            }
            PlayOneShot(audioClip);
        }
    }
}