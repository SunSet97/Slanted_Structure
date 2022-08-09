using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainTitleManager : MonoBehaviour
{
    public Button startButton;

    
    void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene("Ingame_set");
        });
        
    }
}
