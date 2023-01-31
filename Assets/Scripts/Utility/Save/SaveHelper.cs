using Data;
using UnityEngine;
using Utility.System;

namespace Utility.Save
{
    public class SaveHelper : MonoBehaviour
    {
        private static SaveHelper _instance;
        public static SaveHelper instance
        {
            get
            {
                if (!_instance)
                {
                    var obj = FindObjectOfType<SaveHelper>();
                    if (obj)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Resources.Load<SaveHelper>("SaveHelper");   
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
        public SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();
            saveData.mapCode = DataController.instance.mapCode;
            saveData.charData = DataController.instance.charData;
            
            return saveData;
        }
    }
}
