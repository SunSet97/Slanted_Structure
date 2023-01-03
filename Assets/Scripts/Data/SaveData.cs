using System;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public string mapCode;
        public string scenario;

        //저장을 어떻게 할지 생각해보자
        
        public int selfEstm;
        public int intimacySpRau;
        public int intimacyOunRau;
    }
}