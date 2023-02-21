using System;
using UnityEngine;
using UnityEngine.UI;

namespace Data
{
    [Serializable]
    public class CamInfo {
        public Vector3 camDis;
        public Vector3 camRot;
    }

    [Serializable]
    public struct Ending
    {
        public string ending_id;
        public string ending_content;
        public Color color;

        public Image image;
        public CustomEnum.EndingType endingType;
    }
}