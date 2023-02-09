using System.Collections.Generic;
using Data;
using UnityEngine;
using Utility.Core;
using Utility.Interaction;

namespace Utility.Save
{
    public class SaveHelper : MonoBehaviour
    {
        private static SaveHelper _instance;

        public static SaveHelper Instance
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
            SaveData saveData = new SaveData
            {
                mapCode = DataController.instance.mapCode,
                charData = DataController.instance.charData,
                interactionDatas = new List<InteractionSaveData>()
            };

            foreach (var interaction in DataController.instance.InteractionObjects)
            {
                var interactionData = interaction.GetInteractionSaveData();
                saveData.interactionDatas.Add(interactionData);
            }

            return saveData;
        }
    }
}