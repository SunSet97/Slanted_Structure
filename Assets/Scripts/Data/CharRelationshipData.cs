using System;
using Utility.Serialize;

namespace Data
{
    [Serializable]
    public struct CharRelationshipData
    {
        public int selfEstm;
        public int intimacySpRau;
        public int intimacyOunRau;
    }

    [Serializable]
    public struct CharData
    {
        public CustomEnum.Character character;
        public SerializableVector3 pos;
        public SerializableQuaternion rot;
    }
}