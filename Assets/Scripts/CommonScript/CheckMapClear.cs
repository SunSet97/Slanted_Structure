using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static Data.CustomEnum;


[CustomEditor(typeof(CheckMapClear))]
class DebugMapClear : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CheckMapClear checkMapClear = (CheckMapClear)target;
        if (GUILayout.Button("맵 클리어", GUILayout.Height(30), GUILayout.Width(100)))
            if (Application.isPlaying)
                checkMapClear.Clear();
    }
}


public class CheckMapClear : MonoBehaviour
{
    public TextAsset jsonFile;

    public string nextSelectMapcode = "000000"; //어디 스토리로 갈 건지.

    public void Clear()
    {
        if (!nextSelectMapcode.Equals("000000"))
            DataController.instance.currentMap.nextMapcode = nextSelectMapcode;
        if (jsonFile != null)
        {
            CanvasControl.instance.StartConversation(jsonFile.text);
            CanvasControl.instance.SetDialougueEndAction(() =>
                DataController.instance.ChangeMap(DataController.instance.currentMap.nextMapcode));
        }
        else
        {
            DataController.instance.ChangeMap(DataController.instance.currentMap.nextMapcode);
        }
    }

    // 캐릭터 확인 후 트리거 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (DataController.instance.GetCharacter(Character.Main).name.Equals(other.name))
        {
            Clear();
        }
    }
}
