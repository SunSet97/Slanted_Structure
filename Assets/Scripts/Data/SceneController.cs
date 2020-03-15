using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    //인스턴스화
    private static SceneController instance = null;
    public static SceneController instance_SceneController
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        // 새 게임 시작할 경우 새 데이터 파일 생성하도록
        // 씬 이름이 정해지면 주석 풀 예정
    
        //if (sceneName == "newGame")
        //{
        //    DataController.Instance.LoadCharData("NewData.json");
        //}

        DataController.instance_DataController.LoadCharData("NewData.json");
        
        // 씬 이동 시 현재 씬 이름 데이터 파일에 저장
        DataController.instance_DataController.charData.currentScene = sceneName;
    }

    //세이브 된 게임 데이터 파일을 여는 함수
    public void SaveFileOpen(int fileNum)
    {
        if (DataManager.instance_DataManager.isExistdata[fileNum])
        {
            DataController.instance_DataController.LoadCharData("SaveData" + fileNum);
            
            SceneManager.LoadScene(DataController.instance_DataController.charData.currentScene);
            print("SaveData" + fileNum + ".json");
            
        } 

    }
}
