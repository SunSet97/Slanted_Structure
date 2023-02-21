using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;

namespace Utility.Preference
{
    public class PlayUIController : MonoBehaviour
    {
        public static PlayUIController Instance { get; private set; }

        public Transform mapUi;
        
        [Header("메뉴")] [Space(10)]
        public GameObject menuPanel;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private Button menuButton;

        [Header("일기장")] [Space(10)]
        [SerializeField] private GameObject diaryPanel;
        [SerializeField] private Button diaryButton;
        [SerializeField] private Button diaryExitButton;
        
        [Header("설정")] [Space(10)]
        [SerializeField] private GameObject preferencePanel;
        [SerializeField] private Button preferenceButton;
        [SerializeField] private Button preferenceExitButton;

        [Header("사운드 패널")] [Space(10)] [SerializeField]
        private Slider soundSlider;

        [SerializeField] private Button vibeToggle;

        [SerializeField] private Image mute;
        [SerializeField] private Sprite muteOnSprite;
        [SerializeField] private Sprite muteOffSprite;

        [SerializeField] private Image vibe;
        [SerializeField] private Sprite vibeOnSprite;
        [SerializeField] private Sprite vibeOffSprite;
        [SerializeField] private Sprite vibeToggleOnSprite;
        [SerializeField] private Sprite vibeToggleOffSprite;
        
        [NonSerialized] public Canvas Canvas;
        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject.GetComponentInParent<Canvas>().gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject.GetComponentInParent<Canvas>().gameObject);
            }
        }
        
        private void Start()
        {
            Canvas = GetComponentInParent<Canvas>();
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
            
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
            
            diaryButton.onClick.AddListener(() =>
            {
                JoystickController.instance.StopSaveLoadJoyStick(true);
                diaryPanel.SetActive(!diaryPanel.activeSelf);
                preferencePanel.SetActive(false);
            });
            
            diaryExitButton.onClick.AddListener(() =>
            {
                JoystickController.instance.StopSaveLoadJoyStick(false);
                diaryPanel.SetActive(false);
            });

            preferenceButton.onClick.AddListener(() =>
            {
                JoystickController.instance.StopSaveLoadJoyStick(true);
                preferencePanel.SetActive(!preferencePanel.activeSelf);
                diaryPanel.SetActive(false);
            });

            preferenceExitButton.onClick.AddListener(() =>
            {
                JoystickController.instance.StopSaveLoadJoyStick(false);
                preferencePanel.SetActive(false);
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

        private void OnDialogue()
        {
            menuAnimator.SetBool("IsOpen", false);
        }
    }
}