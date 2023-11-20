using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Character;
using Utility.Dialogue;
using Utility.Interaction;

namespace Utility.Save
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        public List<CharacterData> charDatas;

        public CharRelationshipData charRelationshipData;

        public List<InteractionSaveData> interactionDatas;

        public void Debug()
        {
            UnityEngine.Debug.Log($"자존감: {charRelationshipData.selfEstm}\n" +
                                  $"Oun, Rau: {charRelationshipData.intimacyOunRau}\n" +
                                  $"Speat, Rau: {charRelationshipData.intimacySpRau}\n");
            UnityEngine.Debug.Log($"MapCode: {saveCoverData.mapCode}");
            foreach (var saveDataInteractionData in interactionDatas)
            {
                UnityEngine.Debug.Log($"인터랙션 이름: {saveDataInteractionData.id}\n" +
                                      $"Pos: {(Vector3) saveDataInteractionData.pos}\n" +
                                      $"Rot: {(Quaternion) saveDataInteractionData.rot}\n" +
                                      $"Interaction Index: {saveDataInteractionData.interactIndex}");
            }

            foreach (var charData in charDatas)
            {
                UnityEngine.Debug.Log($"캐릭터: {charData.characterType}\n" +
                                      $"Pos: {(Vector3) charData.pos}\n" +
                                      $"Rot: {(Quaternion) charData.rot}\n");
            }
        }
    }

    [Serializable]
    public class SaveCoverData
    {
        public string mapCode;
        public int step;
        public string location;
        public string date;
        public string time;
    }
}