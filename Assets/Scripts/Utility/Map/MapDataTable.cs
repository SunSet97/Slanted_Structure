using System;
using UnityEngine;

namespace Utility.Map
{
    [CreateAssetMenu(fileName = "MapData Table", menuName = "Scriptable Object/MapData Table", order = int.MaxValue)]
    public class MapDataTable : ScriptableObject
    {
        public MapDataProps[] mapData;
    }
    
    [Serializable]
    public class MapDataProps
    {
        public string name;
        public MapCodeProps[] mapCode;
    }

    [Serializable]
    public class MapCodeProps
    {
        public string mapCode;
        public string date;
        public string time;
        public string nextMapCode;
        public string[] clearBoxNextMapCode;
    }
}