using System.Collections.Generic;
using Data;
using UnityEngine;
using Utility.Core;

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

            saveData.InteractionDatas = new List<InteractionData>();
            // foreach (var interactor in DataController.instance.currentMap.interactionObjects)
            // {
            //     saveData.InteractionDatas.Add(new InteractionData
            //     {
            //     
            //     });
            // }
            
            return saveData;
        }
    }
}
