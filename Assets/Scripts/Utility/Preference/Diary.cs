using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Core;
using Utility.Save;
using Utility.Utils;

namespace Utility.Preference
{
    public class Diary : MonoBehaviour
    {
        private enum ButtonType
        {
            Save,
            Load,
            Delete
        }

#pragma warning disable 0649
        [SerializeField] private ButtonType defaultButtonType;

        [SerializeField] private Transform diarySaveParent;

        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        [Header("덮어쓰기")] [Space(15)] [SerializeField]
        private CheckPanel checkPanel;
#pragma warning restore 0649
        
        private int coverIdx;

        private RectMask2D saveMask;
        private RectMask2D loadMask;
        private RectMask2D deleteMask;
        private ButtonType focusedButton;

        private void Awake()
        {
            saveMask = saveButton.transform.parent.GetComponent<RectMask2D>();
            loadMask = loadButton.transform.parent.GetComponent<RectMask2D>();
            deleteMask = deleteButton.transform.parent.GetComponent<RectMask2D>();

            saveButton.onClick.AddListener(() => { FocusButton(ButtonType.Save); });
            loadButton.onClick.AddListener(() => { FocusButton(ButtonType.Load); });
            deleteButton.onClick.AddListener(() => { FocusButton(ButtonType.Delete); });

            checkPanel.SetListener(CheckPanel.ButtonType.No, () => { checkPanel.gameObject.SetActive(false); });

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
                            coverIdx = t;
                            checkPanel.SetText($"저장되지 않은 데이터가 있을 수 있습니다.{System.Environment.NewLine}덮어쓰시겠습니까?");
                            checkPanel.SetListener(CheckPanel.ButtonType.Yes, () =>
                            {
                                var saveData = SaveHelper.Instance.GetSaveData();
                                SaveManager.Save(coverIdx, saveData, () =>
                                {
                                    Debug.Log(coverIdx + "저장");
                                    UpdateDiary();
                                    checkPanel.gameObject.SetActive(false);
                                });
                            });
                            checkPanel.gameObject.SetActive(true);
                        }
                        else
                        {
                            var saveData = SaveHelper.Instance.GetSaveData();
                            SaveManager.Save(t, saveData, () =>
                            {
                                Debug.Log(t + "저장");
                                UpdateDiary();
                            });
                        }
                    }
                    else if (focusedButton == ButtonType.Load)
                    {
                        if (SaveManager.Has(t) && SaveManager.IsCoverLoaded(t))
                        {
                            var sceneName = SceneManager.GetActiveScene().name;

                            if (sceneName.Equals("Ingame_set"))
                            {
                                checkPanel.SetText($"저장되지 않은 데이터가 있을 수 있습니다.{System.Environment.NewLine}로드하시겠습니까?");
                                checkPanel.SetListener(CheckPanel.ButtonType.Yes, () =>
                                {
                                    gameObject.SetActive(false);

                                    SaveManager.Load(t);
                                    SceneLoader.Instance.AddOnLoadListener(() =>
                                    {
                                        var saveData = SaveManager.GetSaveData(t);
                                        saveData.Debug();
                                        DataController.Instance.GameStart(saveData.saveCoverData.mapCode, saveData);

                                        SceneLoader.Instance.RemoveAllListener();
                                    });
                                    SceneLoader.Instance.LoadScene("Ingame_set", t);
                                });
                                checkPanel.gameObject.SetActive(true);
                            }
                            else
                            {
                                SaveManager.Load(t);

                                SceneLoader.Instance.AddOnLoadListener(() =>
                                {
                                    var saveData = SaveManager.GetSaveData(t);
                                    saveData.Debug();
                                    DataController.Instance.GameStart(saveData.saveCoverData.mapCode, saveData);

                                    SceneLoader.Instance.RemoveAllListener();
                                });
                                SceneLoader.Instance.LoadScene("Ingame_set", t);
                            }
                        }
                    }
                    else if (focusedButton == ButtonType.Delete)
                    {
                        checkPanel.SetText($"삭제된 데이터는 복구할 수 없습니다.{System.Environment.NewLine}로드하시겠습니까?");
                        checkPanel.SetListener(CheckPanel.ButtonType.Yes, () =>
                        {
                            if (SaveManager.Has(t))
                            {
                                SaveManager.Delete(t);
                                UpdateDiary();
                            }
                        });
                        checkPanel.gameObject.SetActive(true);
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
                deleteMask.rectTransform.anchoredPosition =
                    new Vector2(179, deleteMask.rectTransform.anchoredPosition.y);

                saveMask.enabled = false;
                loadMask.enabled = true;
                deleteMask.enabled = true;

                for (var idx = 0; idx < diarySaveParent.childCount; idx++)
                {
                    if (!SaveManager.Has(idx))
                    {
                        var button = diarySaveParent.GetChild(idx).GetComponent<Button>();
                        button.interactable = true;
                    }
                }
            }
            else if (buttonType == ButtonType.Load)
            {
                saveMask.rectTransform.anchoredPosition = new Vector2(167, saveMask.rectTransform.anchoredPosition.y);
                loadMask.rectTransform.anchoredPosition = new Vector2(120, loadMask.rectTransform.anchoredPosition.y);
                deleteMask.rectTransform.anchoredPosition =
                    new Vector2(179, deleteMask.rectTransform.anchoredPosition.y);

                saveMask.enabled = true;
                loadMask.enabled = false;
                deleteMask.enabled = true;

                for (var idx = 0; idx < diarySaveParent.childCount; idx++)
                {
                    var button = diarySaveParent.GetChild(idx).GetComponent<Button>();

                    if (!SaveManager.Has(idx))
                    {
                        button.interactable = false;
                    }
                    else
                    {
                        button.interactable = true;
                    }
                }
            }
            else if (buttonType == ButtonType.Delete)
            {
                saveMask.rectTransform.anchoredPosition = new Vector2(179, saveMask.rectTransform.anchoredPosition.y);
                loadMask.rectTransform.anchoredPosition = new Vector2(167, loadMask.rectTransform.anchoredPosition.y);
                deleteMask.rectTransform.anchoredPosition =
                    new Vector2(120, deleteMask.rectTransform.anchoredPosition.y);

                saveMask.enabled = true;
                loadMask.enabled = true;
                deleteMask.enabled = false;

                for (var idx = 0; idx < diarySaveParent.childCount; idx++)
                {
                    var button = diarySaveParent.GetChild(idx).GetComponent<Button>();

                    if (!SaveManager.Has(idx))
                    {
                        button.interactable = false;
                    }
                    else
                    {
                        button.interactable = true;
                    }
                }
            }
        }

        private void OnEnable()
        {
            FocusButton(defaultButtonType);
            UpdateDiary();
        }

        private async void UpdateDiary()
        {
            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                if (SaveManager.Has(idx))
                {
                    var diaryContentText = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                    diaryContentText.text = "";
                    if (!SaveManager.IsCoverLoaded(idx))
                    {
                        var task = SaveManager.LoadCoverAsync(idx);
                        await task;
                    }

                    var saveCoverData = SaveManager.GetCoverData(idx);
                    diaryContentText.text = saveCoverData.location;
                    Debug.Log(diaryContentText.text);
                }
                else
                {
                    var text = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                    text.text = "";
                }
            }
        }
    }
}