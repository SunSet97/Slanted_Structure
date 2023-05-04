using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Utility.Serialize;

namespace Utility.Interaction
{
    [Serializable]
    public struct InteractionSaveData
    {
        public string id;
        
        public SerializableVector3 pos;

        public SerializableQuaternion rot;

        [FormerlySerializedAs("serializedInteractionData")] public List<SerializedInteractionData> serializedInteractionDatas;
        
        public int interactIndex;
    }
}