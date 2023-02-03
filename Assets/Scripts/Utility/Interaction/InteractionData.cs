using System;
using UnityEngine;

namespace Utility.Interaction
{
    [Serializable]
    public struct InteractionData
    {
        public Vector3 pos;
        public Quaternion rot;
        public Interaction interaction;
        public int interactIndex;
    }
}