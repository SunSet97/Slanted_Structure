using System;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Save;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string mapCode;
    
    [SerializeField] private Button newStartButton;
    [SerializeField] private Button diaryButton;
    [SerializeField] private Button diaryCloseButton;
    
    [SerializeField] private GameObject diaryPanel;
    
    [SerializeField] private Transform diarySaveParent;
    
    void Start()
    {
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("Ingame_set");
                    
            SceneLoader.Instance.AddListener(() =>
            {
                DataController.instance.GameStart(mapCode);
                SceneLoader.Instance.RemoveAllListener();
            });
        });
        
        diaryButton.onClick.AddListener(() =>
        {
            diaryPanel.SetActive(true);
        });
        
        diaryCloseButton.onClick.AddListener(() =>
        {
            diaryPanel.SetActive(false);
        });
        for (var idx = 0; idx < diarySaveParent.childCount; idx++)
        {
            if (SaveManager.Load(idx, out SaveData saveData))
            {
                var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
                button.onClick.AddListener(() =>
                {
                    SceneLoader.Instance.LoadScene("Ingame_set");
                    SceneLoader.Instance.AddListener(() =>
                    {
                        DataController.instance.GameStart(mapCode);
                        SceneLoader.Instance.RemoveAllListener();
                    });
                });
                var text = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                text.text = idx + "번입니다" + "\n" + saveData.mapCode;
                Debug.Log(text.text);
            }
            else
            {
                saveData = new SaveData();
                saveData.mapCode = "임시";
                SaveManager.Save(idx, saveData);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    // private void SaveButton(int idx)
    // {
    //     var saveData = SaveManager.GetSaveData();
    //     saveData.SetHp(idx * 100);
    //     
    //     SaveManager.Save(idx);
    //     Debug.Log(saveData.hp);
    // }
}
