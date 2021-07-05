using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckMapClear : MonoBehaviour
{
    public bool isClear; // 클리어 여부
    public string nextSelectMapcode = "000000";//어디 스토리로 갈 건지.
    public CharacterManager who;  // 트리거 신호를 줄 캐릭터

    private void Update()
    {
        if (this.gameObject.GetComponent<InteractionObj_stroke>() != null)
        {
            if (this.gameObject.GetComponent<InteractionObj_stroke>().isTouched == true&& nextSelectMapcode != "000000") //이 오브젝트의 터치를 인식했으면 클리어 여부 체크
            {
                DataController.instance_DataController.currentMap.nextMapcode = nextSelectMapcode;
                isClear = true;
            }
        }
    }

    public void clearForDebug()
    {
        isClear = true;
    }

    // 캐릭터 확인 후 트리거 활성화
    private void OnTriggerEnter(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who)
            {
                if (!nextSelectMapcode.Equals("000000")) DataController.instance_DataController.currentMap.nextMapcode = nextSelectMapcode;
                isClear = true;
            }
    }

    private void OnTriggerStay(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who)
            {
                isClear = true;
                if (!nextSelectMapcode.Equals("000000")) DataController.instance_DataController.currentMap.nextMapcode = nextSelectMapcode;
            }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (who)
            if (other.GetComponent<CharacterManager>() == who)
            {
                if (!nextSelectMapcode.Equals("000000")) DataController.instance_DataController.currentMap.nextMapcode = nextSelectMapcode;
                isClear = false;
            }
    }
}
