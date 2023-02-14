using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.SceneLoader;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string mapCode;
    
    [SerializeField] private Button newStartButton;
    [SerializeField] private Button diaryButton;
    
    [SerializeField] private GameObject diaryPanel;
    
    void Start()
    {
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("Ingame_set");
                    
            SceneLoader.Instance.AddListener(() =>
            {
                DataController.Instance.GameStart(mapCode);
            });
        });
        
        diaryButton.onClick.AddListener(() =>
        {
            diaryPanel.SetActive(true);
        });
    }
}
