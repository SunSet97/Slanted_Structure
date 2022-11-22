using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CharData
    {
        // 스토리 진행 관련
        public int story = 1;
        public int storyBranch = 1;
        public int storyBranch_scnd = 1;
        public int dialogue_index = 1;
   

        // 얻은 아이템들
        public List<string> item = new List<string>();

        // 날짜  
        //public int date = 0;
    
        // 연필 갯수: 게임 데이터 저장하는 데에 사용
        public int pencilCnt = 0;

        public int selfEstm = 0; // 자존감 
        public int intimacy_spRau = 0; // 스핏-라우 친밀도
        public int intimacy_ounRau = 0; // 오운-라우 친밀도
    }
}
