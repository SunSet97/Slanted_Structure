﻿using Data;
using UnityEngine;
using UnityEngine.UI;
using Utility.Save;
using Utility.SceneLoader;

public class Diary : MonoBehaviour
{
    private enum ButtonType { 
        Save, Load, Delete
    }
    

    [SerializeField] private ButtonType defaultButtonType;
    
    [SerializeField] private GameObject diaryPanel;
    [SerializeField] private Button diaryCloseButton;
    [SerializeField] private Transform diarySaveParent;
    
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    
    private RectMask2D saveMask;
    private RectMask2D loadMask;
    private RectMask2D deleteMask;

    private ButtonType focusedButton;
    
    private void Awake()
    {
        saveMask = saveButton.transform.parent.GetComponent<RectMask2D>();
        loadMask = loadButton.transform.parent.GetComponent<RectMask2D>();
        deleteMask = deleteButton.transform.parent.GetComponent<RectMask2D>();
        
        saveButton.onClick.AddListener(() =>
        {
            FocusButton(ButtonType.Save);
        });
        loadButton.onClick.AddListener(() =>
        {
            FocusButton(ButtonType.Load);
        });
        deleteButton.onClick.AddListener(() =>
        {
            FocusButton(ButtonType.Delete);
        });
        
        diaryCloseButton.onClick.AddListener(() =>
        {
            diaryPanel.SetActive(false);
        });

        for (var idx = 0; idx < diarySaveParent.childCount; idx++)
        {
            var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
            var t = idx;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (focusedButton == ButtonType.Save)
                {
                    if (SaveManager.Has(t))
                    {
                        //다시한번 묻기
                    }
                    else
                    {
                        SaveData saveData = new SaveData();
                        saveData.mapCode = "임시";
                        SaveManager.Save(t, saveData);
                        Debug.Log(t + "저장");
                        UpdateDiary();
                    }
                }
                else if (focusedButton == ButtonType.Load)
                {
                    if (SaveManager.Load(t, out SaveData save))
                    {
                        SceneLoader.Instance.LoadScene("Ingame_set");
                        SceneLoader.Instance.AddListener(() =>
                        {
                            DataController.instance.GameStart(save.mapCode);
                            SceneLoader.Instance.RemoveAllListener();
                        });
                    }
                }
                else if (focusedButton == ButtonType.Delete)
                {
                    if (SaveManager.Has(t))
                    {
                        SaveManager.Delete(t);
                        UpdateDiary();
                    }
                }
            });
        }
    }

    private void FocusButton(ButtonType buttonType)
    {
        focusedButton = buttonType;
        if (buttonType == ButtonType.Save)
        {
            saveMask.rectTransform.anchoredPosition = new Vector2(120, saveMask.rectTransform.anchoredPosition.y);
            loadMask.rectTransform.anchoredPosition = new Vector2(167, loadMask.rectTransform.anchoredPosition.y);
            deleteMask.rectTransform.anchoredPosition = new Vector2(179, deleteMask.rectTransform.anchoredPosition.y);

            saveMask.enabled = false;
            loadMask.enabled = true;
            deleteMask.enabled = true;
            
            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                if (!SaveManager.Has(idx))
                {
                    var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
                    button.enabled = true;
                }
            }
        }
        else if (buttonType == ButtonType.Load)
        {
            saveMask.rectTransform.anchoredPosition = new Vector2(167, saveMask.rectTransform.anchoredPosition.y);
            loadMask.rectTransform.anchoredPosition = new Vector2(120, loadMask.rectTransform.anchoredPosition.y);
            deleteMask.rectTransform.anchoredPosition = new Vector2(179, deleteMask.rectTransform.anchoredPosition.y);

            saveMask.enabled = true;
            loadMask.enabled = false;
            deleteMask.enabled = true;
            
            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                if (!SaveManager.Has(idx))
                {
                    var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
                    button.enabled = false;
                }
            }
        }
        else if (buttonType == ButtonType.Delete)
        {
            saveMask.rectTransform.anchoredPosition = new Vector2(179, saveMask.rectTransform.anchoredPosition.y);
            loadMask.rectTransform.anchoredPosition = new Vector2(167, loadMask.rectTransform.anchoredPosition.y);
            deleteMask.rectTransform.anchoredPosition = new Vector2(120, deleteMask.rectTransform.anchoredPosition.y);

            saveMask.enabled = true;
            loadMask.enabled = true;
            deleteMask.enabled = false;
            
            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                if (!SaveManager.Has(idx))
                {
                    var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
                    button.enabled = false;
                }
            }
        }
    }

    private void OnEnable()
    {
        FocusButton(defaultButtonType);
        UpdateDiary();
    }

    private void UpdateDiary()
    {
        for (var idx = 0; idx < diarySaveParent.childCount; idx++)
        {
            if (SaveManager.Load(idx, out SaveData saveData))
            {
                var text = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                text.text = idx + "번입니다" + "\n" + saveData.mapCode;
                Debug.Log(text.text);
            }
            else
            {
                var text = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                text.text = "";
            }
        }
    }
}
