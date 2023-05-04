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
                    mapCode = DataController.Instance.CurrentMap.mapCode,
                    location = DataController.Instance.CurrentMap.location,
                    date = DataController.Instance.CurrentMap.date,
                    time = DataController.Instance.CurrentMap.time
                },
                charDatas = new List<CharacterData>(),
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
                saveData.charDatas.Add(new CharacterData
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