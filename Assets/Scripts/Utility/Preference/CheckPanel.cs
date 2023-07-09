using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility.Preference
{
    public class CheckPanel : MonoBehaviour
    {
        public enum ButtonType
        {
            Yes,
            No
        }

        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        
        [SerializeField] private Text checkText;

        public void SetText(string text)
        {
            checkText.text = text;
        }
        
        public void SetListener(ButtonType buttonType, UnityAction action)
        {
            Button button = null;
            if (buttonType == ButtonType.No)
            {
                button = noButton;
            }
            else if (buttonType == ButtonType.Yes)
            {
                button = yesButton;
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
