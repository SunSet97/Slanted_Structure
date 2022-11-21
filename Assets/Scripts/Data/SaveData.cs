using System;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public string mapCode;
        public string scenario;
        
        
    }

    public class PreferenceData
    {
        public float soundValue;
        public bool isMute;
        public bool isVibe;
    }
}