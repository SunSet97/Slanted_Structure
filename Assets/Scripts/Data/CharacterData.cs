using System;
using Utility.Serialize;

namespace Data
{
    [Serializable]
    public class CharacterData
    {
        public CustomEnum.Character character;
        public SerializableVector3 pos;
        public SerializableQuaternion rot;
    }
}