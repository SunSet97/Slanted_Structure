using UnityEngine;
using UnityEngine.UI;

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
    
    [Header("사운드 패널")]
    [Space(10)]
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Button vibeToggle;
    
    [SerializeField] private Image mute;
    [SerializeField] private Sprite muteOnSprite;
    [SerializeField] private Sprite muteOffSprite;
    
    [SerializeField] private Image vibe;
    [SerializeField] private Sprite vibeOnSprite;
    [SerializeField] private Sprite vibeOffSprite;
    [SerializeField] private Sprite vibeToggleOnSprite;
    [SerializeField] private Sprite vibeToggleOffSprite;

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
        
        vibeToggle.onClick.AddListener(() =>
        {
            if (vibeToggle.image.sprite == vibeToggleOnSprite)
            {
                //진동끄기
                vibeToggle.image.sprite = vibeToggleOffSprite;
                vibe.sprite = vibeOffSprite;
            }
            else
            {
                //진동키기
                vibeToggle.image.sprite = vibeToggleOnSprite;
                vibe.sprite = vibeOnSprite;
            }

            SavePreference();
        });
        
        soundSlider.onValueChanged.AddListener(value =>
        {
            if (Mathf.Approximately(value, 0))
            {
                mute.sprite = muteOffSprite;
            }
            else
            {
                mute.sprite = muteOnSprite;
            }

            SavePreference();
        });
        
        LoadPreference();
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

    private void UpdateUI()
    {
        if (vibeToggle.image.sprite == vibeToggleOnSprite)
        {
            vibeToggle.image.sprite = vibeToggleOnSprite;
            vibe.sprite = vibeOnSprite;
        }
        else
        {
            vibeToggle.image.sprite = vibeToggleOffSprite;
            vibe.sprite = vibeOffSprite;
        }

        if (Mathf.Approximately(soundSlider.value, 0))
        {
            mute.sprite = muteOffSprite;
        }
        else
        {
            mute.sprite = muteOnSprite;
        }
    }
    
    private void SavePreference()
    {
        string vibeState;
        if (vibeToggle.image.sprite == vibeToggleOnSprite)
        {
            vibeState = "on";
        }
        else
        {
            vibeState = "off";
        }
        AudioLoader.SavePreference(vibeState, soundSlider.value);
    }
    
    private void LoadPreference()
    {
        if (AudioLoader.LoadPreference(out float soundValue, out bool isVibe))
        {
            if (isVibe)
            {
                vibeToggle.image.sprite = vibeToggleOnSprite;
                vibe.sprite = vibeOnSprite;
            }
            else
            {
                vibeToggle.image.sprite = vibeToggleOffSprite;
                vibe.sprite = vibeOffSprite;
            }
            soundSlider.value = soundValue;
        }

        UpdateUI();
    }
}
