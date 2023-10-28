using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Utils;
#if UNITY_EDITOR
#endif

namespace Utility.Map
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
#pragma warning disable 0649
        [SerializeField] private bool isTrigger;

        [FormerlySerializedAs("nextSelectMapcode")] public string nextSelectMapCode = "000000";
#pragma warning restore 0649
        
        public void Clear()
        {
            MobileAdsManager.ADCount++;
            if (!nextSelectMapCode.Equals("000000") && !nextSelectMapCode.Equals(""))
            {
                DataController.Instance.CurrentMap.nextMapcode = nextSelectMapCode;
            }
        
            if (DataController.Instance.CurrentMap.useFadeOut)
            {
                FadeEffect.Instance.OnFadeOver += () =>
                {
                    DataController.Instance.ChangeMap(DataController.Instance.CurrentMap.nextMapcode);
                };
                
                FadeEffect.Instance.FadeOut(DataController.Instance.CurrentMap.fadeOutSec);
            }
            else
            {
                DataController.Instance.ChangeMap(DataController.Instance.CurrentMap.nextMapcode);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTrigger)
            {
                return;
            }

            if (DataController.Instance.GetCharacter(Character.CharacterType.Main).name.Equals(other.name))
            {
                Clear();
            }
        }
    }
}