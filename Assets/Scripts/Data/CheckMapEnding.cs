using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckMapEnding : MonoBehaviour
{
    
    private enum CharacterMask
    {
        speat,
        oun,
        rau
    }
    
    public  MapData recieving;              // 트리거 신호를 받을 맵 데이터
    [SerializeField] private CharacterMask character;          // 캐릭터 마스크
    [ContextMenuItem("Speat", "TriggerOnSpeat")]
    [ContextMenuItem("Oun", "TriggerOnOun")]
    [ContextMenuItem("Rau", "TriggerOnRau")]
    [SerializeField] private CharacterManager transmitting;  // 트리거 신호를 줄 캐릭터

    private void Update()
    {
        // 트리거 체크할 캐릭터 확인
        if (character == CharacterMask.speat) transmitting = DataController.instance_DataController.speat;
        if (character == CharacterMask.oun) transmitting = DataController.instance_DataController.oun;
        if (character == CharacterMask.rau) transmitting = DataController.instance_DataController.rau;
    }

    // 트리거 활성화
    void TriggerOnSpeat() { transmitting = DataController.instance_DataController.speat; }
    void TriggerOnOun() { transmitting = DataController.instance_DataController.oun; }
    void TriggerOnRau() { transmitting = DataController.instance_DataController.rau; }

    // 캐릭터 확인 후 트리거 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (transmitting)
            if (other.GetComponent<CharacterManager>() == transmitting) recieving.mapEnding = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (transmitting)
            if (other.GetComponent<CharacterManager>() == transmitting) recieving.mapEnding = true;
    }
}
