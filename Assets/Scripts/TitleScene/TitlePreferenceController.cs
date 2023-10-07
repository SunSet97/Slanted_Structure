using UnityEngine;
using UnityEngine.UI;

namespace TitleScene
{
    public class TitlePreferenceController : MonoBehaviour
    {
        private enum PreferencePanelType
        {
            Audio, Info
        }
    
        [SerializeField] private Button preferenceButton;
        [SerializeField] private Button exitButton;
    
        [SerializeField] private GameObject preferencePanel;
    
        [Space(10)]
        [SerializeField] private Button soundButton;
        [SerializeField] private Button infoButton;
    
        [SerializeField] private GameObject soundPanel;
        [SerializeField] private GameObject infoPanel;
    
        [SerializeField] private Transform focusTransform;
        [SerializeField] private Transform backgroundTransform;

        private PreferencePanelType preferencePanelType;
        
        void Start()
        {
            preferenceButton.onClick.AddListener(() =>
            {
                UpdatePanelFocus(PreferencePanelType.Audio);
                preferencePanel.SetActive(true);
            });
        
            exitButton.onClick.AddListener(() =>
            {
                preferencePanel.SetActive(false);
            });
        
            soundButton.onClick.AddListener(() =>
            {
                UpdatePanelFocus(PreferencePanelType.Audio);
            });
            infoButton.onClick.AddListener(() =>
            {
                UpdatePanelFocus(PreferencePanelType.Info);
            });
        }

        private void UpdatePanelFocus(PreferencePanelType preferencePanelType)
        {
            for (var i = 0; i < focusTransform.childCount; i++)
            {
                focusTransform.GetChild(i).GetComponent<Button>().interactable = true;
                focusTransform.GetChild(i).SetParent(backgroundTransform);   
            }

            Button button = null;
            if (preferencePanelType == PreferencePanelType.Audio)
            {
                button = soundButton;
                soundPanel.SetActive(true);
                infoPanel.SetActive(false);
            }
            else if (preferencePanelType == PreferencePanelType.Info)
            {
                button = infoButton;
                infoPanel.SetActive(true);
                soundPanel.SetActive(false);
            }

            if (button != null)
            {
                button.interactable = false;
                button.transform.SetParent(focusTransform);
            }
        }
    }
}
