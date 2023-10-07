using UnityEngine;

namespace Utility.Audio
{
    public class AudioLoader : MonoBehaviour
    {
        public static void SaveVibe(bool viveState)
        {
            PlayerPrefs.SetString("vibe", viveState.ToString());
        }
        
        public static void SaveAudio(float audioValue)
        {
            PlayerPrefs.SetFloat("sound", audioValue);
        }

        public static void LoadPreference(out float audioValue, out bool isVibe)
        {
            isVibe = false;
            audioValue = 0.5f;
            if (PlayerPrefs.HasKey("Vibe"))
            {
                var vibeString = PlayerPrefs.GetString("Vibe");
                if (vibeString.Equals("true"))
                {
                    isVibe = true;
                }
                else if (vibeString.Equals("false"))
                {
                    isVibe = false;
                }
            }

            if (PlayerPrefs.HasKey("Audio"))
            {
                audioValue = PlayerPrefs.GetFloat("Audio");
            }
        }
    }
}