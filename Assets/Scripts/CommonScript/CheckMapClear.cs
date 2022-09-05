using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static Data.CustomEnum;

#if UNITY_EDITOR
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
#endif

/// <summary>
/// 맵 클리어 (이동) 오브젝트
/// 사용방법 1. isTrigger와 Collider를 사용
/// 사용방법 2. isTrigger 미체크 및 Mapdata.Clearbox.Clear로 사용 
/// </summary>
public class CheckMapClear : MonoBehaviour
{
    public bool isTrigger;
    ///클리어 직전 대화 json파일
    public TextAsset jsonFile;

    //000000이 아닌 null로 사용하고 싶으나 맵이 많아 보류 
    public string nextSelectMapcode = "000000"; //이동할 맵, 000000인 경우 MapData의 nextMapCode 사용

    public void Clear()
    {
        if (!nextSelectMapcode.Equals("000000"))
            DataController.instance.currentMap.nextMapcode = nextSelectMapcode;
        
        if (jsonFile != null)
        {
            DialogueController.instance.StartConversation(jsonFile.text);
            DialogueController.instance.SetDialougueEndAction(() =>
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
        if(!isTrigger){ Debug.LogError("이상하게 사용하고 있습니다"); return;}
        if (DataController.instance.GetCharacter(Character.Main).name.Equals(other.name))
        {
            Clear();
        }
    }
}