using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility.Utils.Json
{
    public static class JsontoString
    {
        private class Wrapper<T>
        {
            public T[] wrapper;
        }

        public static T[] FromJsonArray<T>(string json)
        {
            var t = JsonUtility.FromJson<Wrapper<T>>("{\"wrapper\":" + json + "}").wrapper;
            return t;
        }
        
        public static List<T> FromJsonList<T>(string json)
        {
            return FromJsonArray<T>(json).ToList();
        }
        
        public static string ToJsonArray<T>(T[] wrapper)
        {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < wrapper.Length - 1; i++)
            {
                sb.Append(JsonUtility.ToJson(wrapper[i])).Append(",");
            }
            sb.Append(JsonUtility.ToJson(wrapper[wrapper.Length - 1])).Append("]");
            return sb.ToString();
        }
    }
}
