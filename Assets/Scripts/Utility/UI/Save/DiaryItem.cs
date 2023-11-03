using UnityEngine;
using UnityEngine.UI;

namespace Utility.UI.Save
{
    public class DiaryItem : MonoBehaviour
    {
        public Button button;
        
#pragma warning disable 0649
        [SerializeField] private Text locationText;
        [SerializeField] private Text dateText;
        [SerializeField] private Text timeText;
#pragma warning restore 0649
        public void SetText(string location, string date, string time)
        {
            locationText.text = location;
            dateText.text = date;
            timeText.text = time;
        }
    }
}