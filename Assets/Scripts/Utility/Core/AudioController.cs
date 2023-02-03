using UnityEngine;

namespace Utility.Core
{
    public class AudioController : MonoBehaviour
    {
        private static AudioController _instance;
        public static AudioController instance
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
                }
                return _instance;
            }
        }

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
    
        public void PlayOneShot(AudioClip audioClip)
        {
            sfxSource.PlayOneShot(audioClip);
        }
    }
}