using System;
using UnityEngine;

namespace Utility.Audio
{
    public class AudioLoader : MonoBehaviour
    {
        public static void SaveVibe(bool viveState)
        {
            PlayerPrefs.SetString("Vibe", viveState.ToString());
        }
        
        public static void SaveAudio(float audioValue)
        {
            PlayerPrefs.SetFloat("Audio", audioValue);
        }

        public static void LoadVibe(out bool isVibe)
        {
            isVibe = false;
            if (PlayerPrefs.HasKey("Vibe"))
            {
                var vibeString = PlayerPrefs.GetString("Vibe");
                if (vibeString.Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    isVibe = true;
                }
                else if (vibeString.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    isVibe = false;
                }
            }
        }
        
        public static void LoadAudio(out float audioValue)
        {
            audioValue = 0.5f;

            if (PlayerPrefs.HasKey("Audio"))
            {
                audioValue = PlayerPrefs.GetFloat("Audio");
            }
        }
    }
}