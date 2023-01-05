using UnityEngine;
using UnityEngine.UI;

public class PlayUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject preferencePanel;
    [SerializeField]
    private GameObject diaryPanel;
    
    [SerializeField]
    private Animator menuAnimator;
    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private Button diaryButton;
    [SerializeField]
    private Button preferenceButton;
    [SerializeField]
    private Button preferenceExitButton;
    
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
    
    void Start()
    {
        menuButton.onClick.AddListener(() =>
        {
            if (menuAnimator.GetBool("IsOpen"))
            {
                menuAnimator.SetBool("IsOpen", false);
            }
            else
            {
                menuAnimator.SetBool("IsOpen", true);   
            }
        });
        
        preferenceButton.onClick.AddListener(() =>
        {
            preferencePanel.SetActive(!preferencePanel.activeSelf);
            diaryPanel.SetActive(false);
        });
        
        preferenceExitButton.onClick.AddListener(() =>
        {
            preferencePanel.SetActive(!preferencePanel.activeSelf);
        });
        
        diaryButton.onClick.AddListener(() =>
        {
            preferencePanel.SetActive(false);
            diaryPanel.SetActive(!diaryPanel.activeSelf);
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
