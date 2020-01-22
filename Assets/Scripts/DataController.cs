using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class DataController : MonoBehaviour
{
   
    private static GameObject container;
    static GameObject Container
    {
        get
        {
            return container;
        }
    }

    static DataController instance;
    public static DataController Instance
    {
        get
        {
            if (!instance)
            {
                //print("1");
                container = new GameObject();
                container.name = "DataController";
                instance = container.AddComponent(typeof(DataController)) as DataController;
                DontDestroyOnLoad(container);
            }

            return instance;
        }
    }


    public CharData _charData;
    public CharData charData
    {
        get
        {
            //if(_charData == null)
            //{
            //    print("2");
            //    LoadCharData();
            //    SaveCharData();
            //}
            return _charData;
        }
    }

    //저장할 데이터의 파일이름 
    public string CharDataFileName;
    // 세이브 데이터를 불러오는 함수 
    // 저장된 파일이 있을 경우 기존의 파일을 가져오고 없다면 새로 생성  
    public void LoadCharData(string fileName)
    {
        CharDataFileName = fileName;
        string filePath = Application.persistentDataPath + "/" + CharDataFileName;

        if(File.Exists(filePath))
        {
            
            Debug.Log("로드 성공");
            string FromJsonData = File.ReadAllText(filePath);
            _charData = JsonUtility.FromJson<CharData>(FromJsonData);
        }
        else
        {
            print(Application.persistentDataPath);
            Debug.Log("새 파일 생성");
            _charData = new CharData();
        }
    }

    public void SaveCharData(string fileName)
    {
        string ToJsonData = JsonUtility.ToJson(charData);
        string filePath = Application.persistentDataPath + "/" + fileName;
        File.WriteAllText(filePath, ToJsonData);
        Debug.Log("저장");
    }

    //private void OnApplicationQuit()
    //{
    //    SaveCharData("SaveData.json");
    //}
}
