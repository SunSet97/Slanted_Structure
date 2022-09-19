using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(EditJson))]
public class CustomEditJson : Editor
{
    public override void OnInspectorGUI()
    {
        EditJson generator = (EditJson)target;
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("SaveJson", GUILayout.Height(30), GUILayout.Width(100)))
        {
            generator.SaveJson();
        }
        if (GUILayout.Button("LoadJson", GUILayout.Height(30), GUILayout.Width(100)))
        {
            generator.LoadJson();
        }
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Reset", GUILayout.Height(20), GUILayout.Width(200)))
        {
            generator.ResetData();
        }
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}
#endif

[ExecuteInEditMode]
public class EditJson : MonoBehaviour
{
    enum JsonType
    {
        NONE,
        DIALOGUE,
        TASK
    }

    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private JsonType type;
    [SerializeField] private Dialogue[] dialogues;
    [SerializeField] private List<TaskData> taskDatas;
#if UNITY_EDITOR
    public void LoadJson()
    {
        if (jsonFile)
        {
            if (type == JsonType.DIALOGUE)
            {
                dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
                taskDatas = null;
            }
            else if (type == JsonType.TASK)
            {
                dialogues = null;
                taskDatas.Clear();
                TaskData data = new TaskData
                {
                    tasks = JsontoString.FromJsonArray<Task>(jsonFile.text)
                };
                taskDatas.Add(data);
                for (int index = 0; index < taskDatas.Count; index++)
                {
                    //Debug.Log(taskDatas[index]);
                    for (int i = 0; i < taskDatas[index].tasks.Length; i++)
                    {
                        if (taskDatas[index].tasks[i].taskContentType == CustomEnum.TaskContentType.NEW ||
                            taskDatas[index].tasks[i].taskContentType == CustomEnum.TaskContentType.TEMP)
                        {
                            //Debug.Log(taskDatas[index].tasks[i].nextFile);
                            int count = int.Parse(taskDatas[index].tasks[i].nextFile);
                            //Debug.Log(count);
                            for (int j = 1; j <= count; j++)
                            {
                                //Debug.Log(index + "  " + i + "  " + j);
                                string path = taskDatas[index].tasks[i + j].nextFile;
                                //Debug.Log(i + "  " + j + "  " + path);
                                //Debug.Log(taskData.tasks[0].name);
                                string jsonString = (Resources.Load(path) as TextAsset)?.text;


                                data = new TaskData
                                {
                                    tasks = JsontoString.FromJsonArray<Task>(jsonString)
                                };
                                // 오류가 발생해서 사용 왜 그럴까..
                                if (taskDatas.Count > 50)
                                {
                                    return;
                                }

                                //Debug.Log(taskDatas.IndexOf(taskData) + "에서 " + data.tasks + " 추가");
                                taskDatas.Add(data);
                            }

                            i += count + 1;
                        }
                    }
                }

            }
            else
            {
                taskDatas = null;
                dialogues = null;
            }
        }
    }

    public void ResetData()
    {
        type = JsonType.NONE;
        taskDatas = null;
        dialogues = null;
    }

    public void SaveJson()
    {
        string json = default;
        if (type == JsonType.DIALOGUE)
        {
            if (dialogues != null && dialogues.Length > 0)
                json = JsontoString.ToJsonArray(dialogues);
        }
        else if (type == JsonType.TASK)
        {
            //if (tasks != null && tasks.Length > 0)
            //json = JsontoString.toJsonArray(tasks);
        }

        if (json != default)
        {
            File.WriteAllText(AssetDatabase.GetAssetPath(jsonFile), json);
            AssetDatabase.Refresh();
        }
        else
        {
            if (type != JsonType.NONE)
            {
                Debug.Log("Json 파일 편집 실패");
                Debug.Log(
                    $"현재 타입: {type}\n길이: {dialogues.Length}, Dialogue: {dialogues}\n길이: {taskDatas.Count}, TaskDatas: {taskDatas}");
                foreach (TaskData taskData in taskDatas)
                    Debug.Log($"길이: {taskData.tasks.Length}, Tasks: {taskData.tasks}");
            }
        }
    }
#endif
}