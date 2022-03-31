using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



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
    public string nextSelectMapcode = "000000";//어디 스토리로 갈 건지.
    // public CharacterManager who;  // 트리거 신호를 줄 캐릭터

    public void Clear()
    {
        if (!nextSelectMapcode.Equals("000000"))
            DataController.instance_DataController.currentMap.nextMapcode = nextSelectMapcode;
        if(jsonFile != null)
        {
            CanvasControl.instance_CanvasControl.StartConversation(jsonFile.text);
            CanvasControl.instance_CanvasControl.SetDialougueEndAction(() => DataController.instance_DataController.ChangeMap(DataController.instance_DataController.currentMap.nextMapcode));
        }
        else
        {
            DataController.instance_DataController.ChangeMap(DataController.instance_DataController.currentMap.nextMapcode);
        }
    }

    // 캐릭터 확인 후 트리거 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (DataController.instance_DataController.GetCharacter(MapData.Character.Main).name
            .Equals(other.name))
        {
            Clear();
        }
        // if (who)
        //     if (other.GetComponent<CharacterManager>() == who)
        //     {
        //         Clear();
        //     }
    }

}
