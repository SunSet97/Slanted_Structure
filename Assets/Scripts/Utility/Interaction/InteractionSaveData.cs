using System;
using Utility.Serialize;

namespace Utility.Interaction
{
    [Serializable]
    public struct InteractionSaveData
    {
        public SerializableVector3 pos;
        public SerializableQuaternion rot;
        public SerializedInteractionData serializedInteractionData;
        public int interactIndex;
    }
}