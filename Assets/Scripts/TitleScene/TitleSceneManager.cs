using System;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Save;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button newStartButton;
    [SerializeField] private Button diaryButton;
    [SerializeField] private Button diaryCloseButton;
    
    [SerializeField] private GameObject diaryPanel;
    
    [SerializeField] private Transform diarySaveParent;
    // private SaveData[] saveItems;
    
    void Start()
    {
        // Debug.Log("하이");
        // var ttestSaveData = SaveManager.GetSaveData();
        // ttestSaveData.mapCode = "001010";
        // ttestSaveData.scenario = "몰라용";
        // SaveManager.Save(0);
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("Ingame_set");
                    
            SceneLoader.Instance.AddListener(() =>
            {
                Debug.Log("로드!!!");
                DataController.instance.GameStart();
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
        // saveItems = new SaveData[diarySaveParent.childCount];
        for (var idx = 0; idx < diarySaveParent.childCount; idx++)
        {
            if (SaveManager.Load(idx, out SaveData saveData))
            {
                var button = diarySaveParent.GetComponentInChildren<Button>();
                button.onClick.AddListener(() =>
                {
                    SaveManager.SetSaveData(saveData);
                    SceneLoader.Instance.LoadScene("Ingame_set");
                });
                var text = diarySaveParent.GetComponentInChildren<Text>();
                text.text = idx + "번입니다" + "\n" + saveData.mapCode;
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
