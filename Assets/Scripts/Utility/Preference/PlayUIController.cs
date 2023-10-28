using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Utils;

namespace Utility.Preference
{
    public class PlayUIController : MonoBehaviour
    {
        public static PlayUIController Instance { get; private set; }

#pragma warning disable 0649
        public Transform mapUi;
        public Transform worldSpaceUI;

        [Header("메뉴")] [Space(10)] [SerializeField]
        private GameObject menuPanel;
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
        [SerializeField] private Button titleButton;
        
        [Header("Check Panel")] [Space(10)] [SerializeField]
        private CheckPanel checkPanel;
#pragma warning restore 0649
        
        [NonSerialized] public Canvas Canvas;
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject.GetComponentInParent<Canvas>().gameObject);
            }
            else
            {
                Instance = this;
            }

            Init();
        }
        
        private void Init()
        {
            Canvas = GetComponentInParent<Canvas>();
            var canvasScaler = GetComponentInParent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
            
            menuButton.onClick.AddListener(() =>
            {
                if (menuAnimator.GetBool(IsOpen))
                {
                    menuAnimator.SetBool(IsOpen, false);
                }
                else
                {
                    menuAnimator.SetBool(IsOpen, true);
                }
            });
            
            diaryButton.onClick.AddListener(() =>
            {
                JoystickController.Instance.StopSaveLoadJoyStick(true);
                diaryPanel.SetActive(!diaryPanel.activeSelf);
                preferencePanel.SetActive(false);
            });
            
            diaryExitButton.onClick.AddListener(() =>
            {
                JoystickController.Instance.StopSaveLoadJoyStick(false);
                diaryPanel.SetActive(false);
            });

            preferenceButton.onClick.AddListener(() =>
            {
                JoystickController.Instance.StopSaveLoadJoyStick(true);
                preferencePanel.SetActive(!preferencePanel.activeSelf);
                diaryPanel.SetActive(false);
            });

            preferenceExitButton.onClick.AddListener(() =>
            {
                JoystickController.Instance.StopSaveLoadJoyStick(false);
                preferencePanel.SetActive(false);
            });
            
            checkPanel.SetListener(CheckPanel.ButtonType.No, () => { checkPanel.gameObject.SetActive(false);});
            
            titleButton.onClick.AddListener(() =>
            {
                checkPanel.SetText($"저장되지 않은 데이터가 있을 수 있습니다.{System.Environment.NewLine}타이틀 화면으로 돌아가시겠습니까?");
                checkPanel.SetListener(CheckPanel.ButtonType.Yes, () =>
                {
                    SceneLoader.Instance.LoadScene("TitleScene");  
                });
                checkPanel.gameObject.SetActive(true);
            });
        }
        
        public void SetMenuActive(bool isActive)
        {
            Debug.Log($"메뉴 패널 {isActive}");
            if (!isActive)
            {
                menuAnimator.SetBool(IsOpen, false);
            }
            menuPanel.SetActive(isActive);
        }
    }
}