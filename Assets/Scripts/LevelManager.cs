using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    // [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private GameObject _loadingText;
    [SerializeField] private Image _progressBar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadScenee(sceneName));
        // var scene = SceneManager.LoadSceneAsync(sceneName);
        // scene.allowSceneActivation = false;
        // // _loaderCanvas.SetActive(true);
        // _loadingText.SetActive(true);
        // _progressBar.gameObject.SetActive(true);
        // do
        // {
        //     await Task.Delay(100);
        //     _progressBar.fillAmount = scene.progress;
        // } while (scene.progress < 0.9f);
        //
        // //영상에서는 do while 씀
        //
        // scene.allowSceneActivation = true;
        // // _loaderCanvas.SetActive(false);
        // _loadingText.SetActive(false);

    }

    public IEnumerator LoadScenee(string sceneName)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;
        // _loaderCanvas.SetActive(true);
        _loadingText.SetActive(true);
        _progressBar.gameObject.SetActive(true);
        do
        {
            yield return new WaitForSeconds(0.2f);
            _progressBar.fillAmount = scene.progress;
        } while (scene.progress < 0.9f);
        
        //영상에서는 do while 씀

        scene.allowSceneActivation = true;
        // _loaderCanvas.SetActive(false);
        _loadingText.SetActive(false);
        yield return scene;
        Destroy(gameObject);
    }
}
