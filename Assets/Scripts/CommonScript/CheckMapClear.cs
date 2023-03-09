using Data;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Utility.Core;

namespace CommonScript
{
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
        [SerializeField] private bool isTrigger;

        public string nextSelectMapcode = "000000";

        public void Clear()
        {
            if (!nextSelectMapcode.Equals("000000") && !nextSelectMapcode.Equals(""))
            {
                DataController.Instance.CurrentMap.nextMapcode = nextSelectMapcode;
            }
        
            if (DataController.Instance.CurrentMap.useFadeOut)
            {
                FadeEffect.Instance.OnFadeOver.AddListener(() =>
                {
                    FadeEffect.Instance.OnFadeOver.RemoveAllListeners();
                    DataController.Instance.ChangeMap(DataController.Instance.CurrentMap.nextMapcode);
                });
                FadeEffect.Instance.FadeOut(DataController.Instance.CurrentMap.fadeOutSec);
            }
            else
            {
                Debug.Log(nextSelectMapcode);
                Debug.Log(DataController.Instance.CurrentMap.nextMapcode);
                DataController.Instance.ChangeMap(DataController.Instance.CurrentMap.nextMapcode);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTrigger)
            {
                return;
            }

            if (DataController.Instance.GetCharacter(CustomEnum.Character.Main).name.Equals(other.name))
            {
                Clear();
            }
        }
    }
}