using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

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

[ExecuteInEditMode]
public class EditJson : MonoBehaviour
{
    enum JsonType { NONE, DIALOGUE, TASK }

    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private JsonType type;
    [SerializeField] private Dialogue[] dialogues;
    [SerializeField] private Task[] tasks;
    public void LoadJson()
    {
        if (jsonFile)
        {
            if (type == JsonType.DIALOGUE)
            {
                dialogues = JsontoString.FromJsonArray<Dialogue>(jsonFile.text);
                tasks = null;
            }
            else if (type == JsonType.TASK)
            {
                tasks = JsontoString.FromJsonArray<Task>(jsonFile.text);
                dialogues = null;
            }
            else
            {
                tasks = null;
                dialogues = null;
            }
        }
    }
    public void ResetData()
    {
        type = JsonType.NONE;
        tasks = null;
        dialogues = null;
    }
    public void SaveJson()
    {
        string json = default;
        if (type == JsonType.DIALOGUE)
        {
            if (dialogues != null && dialogues.Length > 0)
                    json = JsontoString.toJsonArray(dialogues);
        }
        else if (type == JsonType.TASK)
        {
            if (tasks != null && tasks.Length > 0)
                    json = JsontoString.toJsonArray(tasks);
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
                Debug.Log($"현재 타입: {type}\nDialogue: {dialogues}, 길이: {dialogues.Length}\nTask: {tasks}, 길이: {tasks.Length}");
            }
        }
    }
}