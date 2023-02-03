using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public string mapCode;

        public CharData charData;

        public List<InteractionData> InteractionDatas;
    }
}