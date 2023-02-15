using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.SceneLoader;

namespace TitleScene
{
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] private string mapCode;

        [SerializeField] private Button newStartButton;

        [SerializeField] private Button diaryButton;
        [SerializeField] private Button diaryExitButton;

        [SerializeField] private GameObject diaryPanel;

        private void Start()
        {
            newStartButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("Ingame_set");

                SceneLoader.Instance.AddListener(() =>
                {
                    if (Application.isEditor)
                    {
                        DataController.Instance.GameStart(mapCode);
                    }
                    else
                    {
                        DataController.Instance.GameStart();
                    }
                });
            });
            diaryButton.onClick.AddListener(() => { diaryPanel.SetActive(true); });
            diaryExitButton.onClick.AddListener(() => { diaryPanel.SetActive(false); });
        }
    }
}