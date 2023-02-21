using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Save;

namespace Utility.SceneLoader
{
    public class SceneLoader : MonoBehaviour
    {

        private static SceneLoader _instance;

        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<SceneLoader>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        [SerializeField] private CanvasGroup sceneLoaderCanvasGroup;
        [SerializeField] private Image progressBar;

        private UnityAction<Scene, LoadSceneMode> onSceneLoaded;

        private string loadSceneName;

        private static SceneLoader Create()
        {
            var sceneLoaderPrefab = Resources.Load<SceneLoader>("SceneLoader");
            return Instantiate(sceneLoaderPrefab);
        }

        public void LoadScene(string sceneName, int idx = -1)
        {
            gameObject.SetActive(true);
            SceneManager.sceneLoaded += LoadSceneEnd;
            loadSceneName = sceneName;
            StartCoroutine(Load(sceneName, idx));
        }

        private IEnumerator Load(string sceneName, int idx)
        {
            progressBar.fillAmount = 0f;
            yield return StartCoroutine(Fade(true));

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float timer = 0.0f;
            while (!op.isDone)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;
                if (op.progress < 0.9f)
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                    if (progressBar.fillAmount >= op.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                    if (idx == -1)
                    {
                        if (Mathf.Approximately(progressBar.fillAmount, 1.0f))
                        {
                            op.allowSceneActivation = true;
                            yield break;
                        }
                    }
                    else
                    {
                        if (Mathf.Approximately(progressBar.fillAmount, 1.0f) && SaveManager.IsLoaded(idx))
                        {
                            op.allowSceneActivation = true;
                            yield break;
                        }   
                    }
                }
            }
        }

        private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == loadSceneName)
            {
                SceneManager.sceneLoaded -= LoadSceneEnd;
                gameObject.SetActive(false);
            }
        }

        public void AddListener(UnityAction t)
        {
            onSceneLoaded += (scene, loadSceneMode) => { t(); };
            SceneManager.sceneLoaded += onSceneLoaded;
        }

        public void RemoveAllListener()
        {
            SceneManager.sceneLoaded -= onSceneLoaded;
            onSceneLoaded = (scene, loadSceneMode) => { };
        }

        private IEnumerator Fade(bool isFadeIn)
        {
            float timer = 0f;

            while (timer <= 1f)
            {
                yield return null;
                timer += Time.unscaledDeltaTime * 2f;
                sceneLoaderCanvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
            }

            if (!isFadeIn)
            {
                gameObject.SetActive(false);
            }
        }
    }
}