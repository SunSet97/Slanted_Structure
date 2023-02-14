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
                saveCoverData = new SaveCoverData
                {
                    mapCode = DataController.Instance.mapCode
                },
                charDatas = new List<CharData>(),
                charRelationshipData = DataController.Instance.charRelationshipData,
                interactionDatas = new List<InteractionSaveData>()
            };

            foreach (var interaction in DataController.Instance.InteractionObjects)
            {
                var interactionData = interaction.GetInteractionSaveData();
                saveData.interactionDatas.Add(interactionData);
            }
            
            foreach (var positionSet in DataController.Instance.CurrentMap.positionSets)
            {
                var character = DataController.Instance.GetCharacter(positionSet.who);
                saveData.charDatas.Add(new CharData
                {
                    pos = character.transform.position,
                    rot = character.transform.rotation,
                    character = character.who
                });
            }
            saveData.Debug();
            return saveData;
        }
    }
}