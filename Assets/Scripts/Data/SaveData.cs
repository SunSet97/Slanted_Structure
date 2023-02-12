using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Interaction;

namespace Data
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        public CharData charData;

        public List<InteractionSaveData> interactionDatas;

        public void Debug()
        {
            UnityEngine.Debug.Log($"자존감: {charData.selfEstm}\n" +
                                  $"Oun, Rau: {charData.intimacyOunRau}\n" +
                                  $"Speat, Rau: {charData.intimacySpRau}\n");
            UnityEngine.Debug.Log($"MapCode: {saveCoverData.mapCode}");
            foreach (var saveDataInteractionData in interactionDatas)
            {
                UnityEngine.Debug.Log($"인터랙션 이름: {saveDataInteractionData.name}\n" +
                                      $"Pos: {(Vector3)saveDataInteractionData.pos}\n" +
                                      $"Rot: {(Quaternion)saveDataInteractionData.rot}\n" +
                                      $"Interaction Index: {saveDataInteractionData.interactIndex}");
            }
        }
    }
    
    [Serializable]
    public class SaveCoverData
    {
        public string mapCode;
    }
}