using System;
using System.Collections.Generic;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        public CharData charData;

        public List<InteractionSaveData> interactionDatas;
    }
    
    [Serializable]
    public class SaveCoverData
    {
        public string mapCode;
    }
}