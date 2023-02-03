using System;
using System.Collections.Generic;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public string mapCode;

        public CharData charData;

        public List<InteractionData> interactionDatas;
    }
}