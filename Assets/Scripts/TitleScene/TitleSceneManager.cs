using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button newStartButton;
    [SerializeField] private Button diaryButton;
    [SerializeField] private Button diaryCloseButton;
    
    [SerializeField] private GameObject diaryPanel;
    
    [SerializeField] private GameObject diarySaveParent;
    
    void Start()
    {
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("Ingame_set");
        });
    }
}
