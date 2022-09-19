using System;
using Data;
using UnityEngine;
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
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("Ingame_set");
        });
        // saveItems = new SaveData[diarySaveParent.childCount];
        for (var idx = 0; idx < diarySaveParent.childCount; idx++)
        {
            if (SaveManager.Load(idx))
            {
                var saveData = SaveManager.GetSaveData();
                var button = diarySaveParent.GetComponentInChildren<Button>();
                button.onClick.AddListener(() =>
                {
                    SceneLoader.Instance.LoadScene(saveData.mapCode);
                });
                var text = diarySaveParent.GetComponentInChildren<Text>();
                text.text = idx + "번입니다";
            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    private void LoadButton(int idx)
    {
        if (SaveManager.Load(idx))
        {
            var saveData = SaveManager.GetSaveData();
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
