using UnityEngine;

namespace Utility.Preference
{
    public class AudioLoader : MonoBehaviour
    {
        public static void SavePreference(string viveState, float soundValue)
        {
            PlayerPrefs.SetString("vibe", viveState);

            PlayerPrefs.SetFloat("sound", soundValue);
        }

        public static bool LoadPreference(out float soundValue, out bool isVibe)
        {
            isVibe = true;
            soundValue = 0.5f;
            if (PlayerPrefs.HasKey("vibe"))
            {
                var vibeString = PlayerPrefs.GetString("vibe");
                if (vibeString.Equals("on"))
                {
                    isVibe = true;
                }
                else if (vibeString.Equals("off"))
                {
                    isVibe = false;
                }
            }
            else
            {
                return false;
            }

            if (PlayerPrefs.HasKey("sound"))
            {
                soundValue = PlayerPrefs.GetFloat("sound");
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}