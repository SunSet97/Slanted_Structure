using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckMapClear : MonoBehaviour
{
    public int mask; // 캐릭터 마스크
    public bool isClear; // 클리어 여부
    [SerializeField] private CharacterManager who;  // 트리거 신호를 줄 캐릭터

    private void Update()
    {
        // 트리거 체크할 캐릭터 확인
        if (DataController.instance_DataController)
        {
            if (mask == 0) who = DataController.instance_DataController.speat;
            if (mask == 1) who = DataController.instance_DataController.oun;
            if (mask == 2) who = DataController.instance_DataController.rau;
        }
    }

    // 캐릭터 확인 후 트리거 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who) isClear = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who) isClear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who) isClear = false;
    }
}
