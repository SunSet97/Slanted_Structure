using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        public List<CharData> charDatas;
        
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
                                      $"Pos: {(Vector3)saveDataInteractionData.pos}\n" +
                                      $"Rot: {(Quaternion)saveDataInteractionData.rot}\n" +
                                      $"Interaction Index: {saveDataInteractionData.interactIndex}");
            }
            foreach (var charData in charDatas)
            {
                UnityEngine.Debug.Log($"캐릭터: {charData.character}\n" +
                                      $"Pos: {(Vector3)charData.pos}\n" +
                                      $"Rot: {(Quaternion)charData.rot}\n");
            }
        }
    }
    
    [Serializable]
    public class SaveCoverData
    {
        public string mapCode;
    }
}