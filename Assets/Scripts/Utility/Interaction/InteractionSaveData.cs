using System;
using System.Collections.Generic;
using Utility.Serialize;

namespace Utility.Interaction
{
    [Serializable]
    public struct InteractionSaveData
    {
        public string id;
        
        public SerializableVector3 pos;

        public SerializableQuaternion rot;

        public List<SerializedInteractionData> serializedInteractionData;
        
        public int interactIndex;
    }
}