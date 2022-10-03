using System.IO;
using UnityEngine;
using System.Text;

public class JsontoString
{
    private class Wrapper<T>
    {
        public T[] wrapper;
    }

    public static T[] FromJsonPath<T>(string path)
    {
        var t = JsonUtility.FromJson<Wrapper<T>>("{\"wrapper\":" + File.ReadAllText(path)+ "}").wrapper;
        return t;
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

    // 추후 폴더 경로, 안드로이드 경로 변경 요망
    // 맨 앞에 'Json'이라고 쓰인 타입만 받아들여 json을 불러온다.
    public static T[] LoadJsonFromClassName<T>(string mapCode)
    {
        T[] t = default;
        string type = typeof(T).Name;
        //Json이 있어 4글자 이상인 경우, 앞에 Json이 붙어있을 경우
        try
        {
            //안드로이드는 나중에 공부 후
            if (Application.platform == RuntimePlatform.Android)
            {
                //string jsonPath = Path.Combine(GetDatafolderpath(), type.substring(4));
                //www reader = new www(jsonpath);
                //while (!reader.isdone) { }
                //string jsonstring = encoding.default.getstring(reader.bytes);
                //t = jsonutility.fromjson<wrapper<t>>("{\"wrapper\":" + jsonstring + "}").wrapper;
            }
            else
            {
                //ex) JsonMessage클래스 -> 폴더경로/000000/NPC/Default/type명  -  class type명과 폴더명 동일하게
                string jsonPath = string.Format("{0}/{1}/NPC/{2}/{3}.json", GetDataFolderPath(), "000000", "Default", type.Substring(4));
                //NPC 여부 스토리여부
                string jsonString = File.ReadAllText(jsonPath);
                Debug.Log(jsonString);
                t = JsonUtility.FromJson<Wrapper<T>>("{\"wrapper\":" + jsonString + "}").wrapper;
            }
        }
        catch
        {
            Debug.LogError("존재하지 않는 json 파일명 : " + type);
        }

        return t;
    }

    private static string GetDataFolderPath()
    {
        string path;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.streamingAssetsPath;
                break;
            case RuntimePlatform.WindowsEditor:
                //경로 수정 필요
                path = Application.dataPath + "/Dialogues";
                break;
            default:
                Debug.Log("설정되지 않은 플랫폼 : " + Application.platform);
                return null;
        }
        //Debug.Log($"{Application.platform} Path : {path}");
        return path;
    }
}
