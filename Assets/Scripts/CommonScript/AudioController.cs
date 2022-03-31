using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance { get; private set; }

    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private AudioSource sfxSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    public void PlayBgm(AudioClip audioClip)
    {
        bgmSource.clip = audioClip;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        bgmSource.clip = null;
        bgmSource.Stop();
    }
    
    public void PlayOneShot(AudioClip audioClip)
    {
        sfxSource.PlayOneShot(audioClip);
    }
}