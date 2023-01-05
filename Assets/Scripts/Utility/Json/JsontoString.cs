using System.Text;
using UnityEngine;

namespace Utility.Json
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
