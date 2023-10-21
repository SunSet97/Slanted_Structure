using System;
using UnityEngine.Serialization;
using Utility.Utils.Serialize;

namespace Utility.Character
{
    public enum CharacterType
    {
        Main = -1,
        Speat,
        Oun,
        Rau,
        Speat_Child,
        Speat_Adult,
        Speat_Adolescene
    }
    
    [Serializable]
    public class CharacterData
    {
        [FormerlySerializedAs("character")] public CharacterType characterType;
        public SerializableVector3 pos;
        public SerializableQuaternion rot;
    }
}