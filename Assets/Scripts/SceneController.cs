using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 씬 부르는 함수
    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    } 

    //처음 게임 시작할 때 (새로운 데이터로)
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        // 새 게임 시작할 경우 새 데이터 파일 생성하도록
        // 씬 이름이 정해지면 주석 풀 예정
    
        //if (sceneName == "newGame")
        //{
        //    DataController.Instance.LoadCharData("NewData.json");
        //}

        DataController.Instance.LoadData("Save", "NewData.json");
        
        // 씬 이동 시 현재 씬 이름 데이터 파일에 저장
        DataController.Instance.charData.currentScene = sceneName;

    }

    //세이브 된 게임 데이터 파일을 여는 함수
    public void SaveFileOpen(int fileNum)
    {
        if (DataManager.Instance.isExistdata[fileNum])
        {
            DataController.Instance.LoadData("Save", "SaveData" + fileNum);
            
            SceneManager.LoadScene(DataController.Instance.charData.currentScene);
            print("SaveData" + fileNum + ".json");
            
        } 

    }
}
