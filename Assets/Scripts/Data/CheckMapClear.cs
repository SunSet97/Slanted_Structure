using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckMapClear : MonoBehaviour
{
    public bool isClear; // 클리어 여부
    [SerializeField] private CharacterManager who;  // 트리거 신호를 줄 캐릭터

    private void Update()
    {
        if (this.gameObject.GetComponent<InteractionObj_stroke>() != null)
        {
            if (this.gameObject.GetComponent<InteractionObj_stroke>().isTouched == true) //이 오브젝트의 터치를 인식했으면 클리어 여부 체크
            {
                isClear = true;
            }
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
