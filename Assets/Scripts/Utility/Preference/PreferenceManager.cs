using UnityEngine;
using UnityEngine.UI;

public class PreferenceManager : MonoBehaviour
{
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
    [SerializeField] private Slider slider;
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
        preferenceButton.onClick.AddListener(() =>
        {
            preferencePanel.SetActive(true);
        });
        
        exitButton.onClick.AddListener(() =>
        {
            preferencePanel.SetActive(false);
        });
        
        soundButton.onClick.AddListener(() =>
        {
            soundPanel.SetActive(true);
            infoPanel.SetActive(false);
            UpdateButtonFocus(soundButton);
        });
        infoButton.onClick.AddListener(() =>
        {
            soundPanel.SetActive(false);
            infoPanel.SetActive(true);
            UpdateButtonFocus(infoButton);
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
        });
        
        slider.onValueChanged.AddListener((value =>
        {
            if (Mathf.Approximately(value, 0))
            {
                mute.sprite = muteOffSprite;
            }
            else
            {
                mute.sprite = muteOnSprite;
            }
        }));

        UpdateButtonFocus(soundButton);
    }

    void UpdateButtonFocus(Button button)
    {
        for (var i = 0; i < focusTransform.childCount; i++)
        {
            focusTransform.GetChild(i).GetComponent<Button>().interactable = true;
            focusTransform.GetChild(i).SetParent(backgroundTransform);   
        }

        button.interactable = false;
        button.transform.SetParent(focusTransform);
    }
}
