using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Core;
using Utility.Save;

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

        [SerializeField] private ButtonType defaultButtonType;

        [SerializeField] private GameObject diaryPanel;
        [SerializeField] private Transform diarySaveParent;

        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        [Header("덮어쓰기")] [Space(15)] [SerializeField]
        private GameObject coverPanel;

        [SerializeField] private Button coverYesButton;
        [SerializeField] private Button coverNoButton;

        private int _coverIdx;

        private RectMask2D _saveMask;
        private RectMask2D _loadMask;
        private RectMask2D _deleteMask;

        private ButtonType _focusedButton;

        private void Awake()
        {
            _saveMask = saveButton.transform.parent.GetComponent<RectMask2D>();
            _loadMask = loadButton.transform.parent.GetComponent<RectMask2D>();
            _deleteMask = deleteButton.transform.parent.GetComponent<RectMask2D>();

            saveButton.onClick.AddListener(() => { FocusButton(ButtonType.Save); });
            loadButton.onClick.AddListener(() => { FocusButton(ButtonType.Load); });
            deleteButton.onClick.AddListener(() => { FocusButton(ButtonType.Delete); });

            coverYesButton.onClick.AddListener(() =>
            {
                SaveData saveData = SaveHelper.Instance.GetSaveData();
                SaveManager.Save(_coverIdx, saveData, () =>
                {
                    Debug.Log(_coverIdx + "저장");
                    UpdateDiary();
                    coverPanel.SetActive(false);
                });
            });

            coverNoButton.onClick.AddListener(() => { coverPanel.SetActive(false); });

            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                var button = diarySaveParent.GetChild(idx).GetComponentInChildren<Button>();
                var t = idx;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (_focusedButton == ButtonType.Save)
                    {
                        if (SaveManager.Has(t))
                        {
                            _coverIdx = t;
                            coverPanel.SetActive(true);
                        }
                        else
                        {
                            SaveData saveData = SaveHelper.Instance.GetSaveData();
                            SaveManager.Save(t, saveData, () =>
                            {
                                Debug.Log(t + "저장");
                                UpdateDiary();
                            });
                        }
                    }
                    else if (_focusedButton == ButtonType.Load)
                    {
                        if (SaveManager.Has(t) && SaveManager.IsCoverLoaded(t))
                        {
                            var sceneName = SceneManager.GetActiveScene().name;

                            if (sceneName.Equals("Ingame_set"))
                            {
                                diaryPanel.SetActive(false);
                            }

                            SaveManager.Load(t);
                            SceneLoader.SceneLoader.Instance.LoadScene("Ingame_set", t);

                            SceneLoader.SceneLoader.Instance.AddListener(() =>
                            {
                                var saveData = SaveManager.GetSaveData(t);
                                saveData.Debug();
                                DataController.Instance.GameStart(saveData.saveCoverData.mapCode, saveData);

                                SceneLoader.SceneLoader.Instance.RemoveAllListener();
                            });
                        }
                    }
                    else if (_focusedButton == ButtonType.Delete)
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
            _focusedButton = buttonType;
            if (buttonType == ButtonType.Save)
            {
                _saveMask.rectTransform.anchoredPosition = new Vector2(120, _saveMask.rectTransform.anchoredPosition.y);
                _loadMask.rectTransform.anchoredPosition = new Vector2(167, _loadMask.rectTransform.anchoredPosition.y);
                _deleteMask.rectTransform.anchoredPosition =
                    new Vector2(179, _deleteMask.rectTransform.anchoredPosition.y);

                _saveMask.enabled = false;
                _loadMask.enabled = true;
                _deleteMask.enabled = true;

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
                _saveMask.rectTransform.anchoredPosition = new Vector2(167, _saveMask.rectTransform.anchoredPosition.y);
                _loadMask.rectTransform.anchoredPosition = new Vector2(120, _loadMask.rectTransform.anchoredPosition.y);
                _deleteMask.rectTransform.anchoredPosition =
                    new Vector2(179, _deleteMask.rectTransform.anchoredPosition.y);

                _saveMask.enabled = true;
                _loadMask.enabled = false;
                _deleteMask.enabled = true;

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
                _saveMask.rectTransform.anchoredPosition = new Vector2(179, _saveMask.rectTransform.anchoredPosition.y);
                _loadMask.rectTransform.anchoredPosition = new Vector2(167, _loadMask.rectTransform.anchoredPosition.y);
                _deleteMask.rectTransform.anchoredPosition =
                    new Vector2(120, _deleteMask.rectTransform.anchoredPosition.y);

                _saveMask.enabled = true;
                _loadMask.enabled = true;
                _deleteMask.enabled = false;

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

        private async void UpdateDiary()
        {
            for (var idx = 0; idx < diarySaveParent.childCount; idx++)
            {
                if (SaveManager.Has(idx))
                {
                    if (!SaveManager.IsCoverLoaded(idx))
                    {
                        var task = SaveManager.LoadCoverAsync(idx);
                        // Debug.Log(Time.time + "Task 대기");
                        await task;
                    }

                    var saveCoverData = SaveManager.GetCoverData(idx);
                    var text = diarySaveParent.GetChild(idx).GetComponentInChildren<Text>();
                    text.text = idx + "번입니다" + "\n" + saveCoverData.mapCode;
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
}
