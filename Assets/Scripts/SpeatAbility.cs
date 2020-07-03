using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeatAbility : MonoBehaviour
{
    [Header("-UI")]
    public Image abilityImage;  // 능력 시간 UI 이미지
    public Text abilityText;    // 통과한 벽 UI 텍스트
    public Image buttonImage;   // 능력 사용 버튼 UI 이미지

    [Header("-Variable")]
    public float setDuration = 10;  // 능력 지속 시간 설정값
    private float duration = 10;     // 능력 지속 시간
    public float setCooltime = 3;   // 능력 사용 대기 시간 설정값
    private float cooltime = 0;     // 능력 사용 대기 시간
    public int maxWallNum = 5;      // 최대 벽 통과 갯수
    private int wallNum = 0;        // 벽 통과 횟수
    private bool isAbility = false; // 현재 능력 사용 여부

    public float fw;
    public float bw;
    public float sid;
    public float u;
    public float d;
    void Update()
    {
        // 능력 사용중이 아닐때
        if (!isAbility)
        {
            // 사용 대기 시간 계산
            if (cooltime > 0) cooltime -= Time.deltaTime;
            else if (cooltime <= 0) cooltime = 0;
            abilityImage.fillAmount = cooltime / setCooltime; // 쿨타임 시간에 맞춰 UI 변화

            abilityText.text = null;// 벽 통과 횟수 숨김

            buttonImage.color = Color.black; // 터치 이미지 불투명
        }
        // 능력 사용중일 때
        if (isAbility)
        {
            // 지속 시간 계산
            if (duration > 0) duration -= Time.deltaTime;
            else if (duration <= 0) { isAbility = false; duration = 0; } // 지속 시간 초과시 능력 자동 종료 및 0 고정
            abilityImage.fillAmount = (setDuration - duration) / setDuration; // 지속 시간에 맞춰 UI 변화

            if (wallNum < maxWallNum) abilityText.text = wallNum.ToString(); // 벽 통과 횟수 변화
            else { isAbility = false; abilityText.text = null; } // 벽 통과 횟수 초과시 능력 자동 종료 및 숨김

            buttonImage.color = Color.clear; // 터치 이미지 투명
        }

        Transform sp = DataController.instance_DataController.speat.transform;
        Vector3 spc = sp.position + Vector3.up * 0.55f;
        Debug.DrawLine(spc, spc + sp.forward * fw);
        Debug.DrawLine(spc, spc - sp.forward * bw);
        Debug.DrawLine(spc, spc + sp.right *sid);
        Debug.DrawLine(spc, spc - sp.right * sid);
        Debug.DrawLine(spc, spc + sp.up *u);
        Debug.DrawLine(spc, spc - sp.up *d);
    }

    // 능력 사용 버튼
    public void UseAbility()
    {
        // 능력 사용중이 아닐때
        if (!isAbility && cooltime<=0)
        {
            isAbility = true;
            wallNum = 0; abilityText.text = "0";            // 벽 통과 횟수 초기화
            duration = setDuration; cooltime = setCooltime; // 시간 초기화
            abilityImage.fillAmount = 0;                    // UI 초기화
        }
        // 능력 사용중일 때
        else if (isAbility)
        {
            isAbility = false;
            wallNum = 0; abilityText.text = null;           // 벽 통과 횟수 초기화 및 숨김
            duration = setDuration; cooltime = setCooltime; // 시간 초기화
            abilityImage.fillAmount = 1;                    // UI 초기화
        }
    }
}
