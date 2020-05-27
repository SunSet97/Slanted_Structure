using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability_Speat : MonoBehaviour
{
    Image mask;
    public Image[] hand = new Image[2]; //손 이미지. 0번째는 비활성화, 1번째는 활성화
    public Text[] num = new Text[2]; //사용 횟수표시 텍스트. 0번째는 비활성화, 1번째는 활성화
    Move_Speat ms; //move speat 스크립트를 쓰지 않게 되면 수정해야 함.
    Transform sp_t; //speat의 트랜스폼
    public bool isPowered, isAvailable; //능력을 사용중인지 판별하는 변수, 현재 사용 가능한지 판별하는 변수(쿨타임이 지났는지)
    IEnumerator cool_eff;
    public int cool_time = 3, use_time = 10, possible_num = 5; //쿨타임, 사용 가능 시간, 사용 가능 횟수

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ms.GetComponent<Rigidbody>().AddForce(new Vector3(15, 0, 0), ForceMode.Impulse);
         //StartCoroutine(Dash(Vector3.right));
        if (Input.GetKeyDown(KeyCode.S))
            ms.GetComponent<Rigidbody>().AddForce(new Vector3(0, -15, 0), ForceMode.Impulse);
        // StartCoroutine(Dash(Vector3.down));
    }

    private void Awake()
    {
        isPowered = false; //능력 사용중이 아님
        isAvailable = true; //능력 사용 가능
        ms = GameObject.Find("Speat").GetComponent<Move_Speat>(); //Speat이라는 오브젝트를 찾아 Move_Speat 스크립트를 가져옴.
        sp_t = ms.transform; //Speat의 transform
        mask = transform.parent.gameObject.GetComponent<Image>();
    }

    public void Touched() //버튼 터치 시 실행
    {
        if(!isPowered && isAvailable) //현재 사용중이 아니고, 사용 가능할 때 (능력 사용)
        {
            isPowered = true;
            isAvailable = false;
            cool_eff = CoolEff(use_time, 1, 0);
            StartCoroutine(cool_eff);
            ms.joy.gameObject.SetActive(false); //조이스틱을 잠시 안보이게 만듦(필요 없으므로)
            HandSet(false); //손 이미지 비활성화
            NumSet(true); //능력 횟수 텍스트 활성화
        }
        else if(isPowered) //현재 사용중일 때 (능력 사용 중지)
        {
            isPowered = false;
            StopCoroutine(cool_eff);
            StartCoroutine(CoolEff(cool_time, 0, 1));
            ms.joy.gameObject.SetActive(true); //조이스틱 활성화
            HandSet(true); //손 이미지 활성화
            NumSet(false); //능력 횟수 텍스트 비활성화
        }
        
    }

    IEnumerator CoolEff(int get_time, int start, int end) //쿨타임 표시 함수
    {
        float time = 0;
        Debug.Log(get_time);
        Debug.Log(time);
        while (time < get_time)
        {
            time += Time.deltaTime;
            mask.fillAmount = Mathf.Lerp(start, end, time / get_time);
            yield return null;
        }
        isPowered = false;
        if (end == 1) //끝날 때 꽉 차게 됐다면(쿨타임 지난 후)
        {
            isAvailable = true; //사용 가능
        }
        else //사용 시간이 지난 후 
        {
            StartCoroutine(CoolEff(cool_time, 0, 1));
            ms.joy.gameObject.SetActive(true); //조이스틱 활성화
            HandSet(true); //손 이미지 활성화
            NumSet(false); //능력 횟수 텍스트 비활성화
        }
    }

    void HandSet(bool active) //손 이미지 활성화 or 비활성화 함수
    {
        hand[0].gameObject.SetActive(active);
        hand[1].gameObject.SetActive(active);
    }
    void NumSet(bool active) //횟수 텍스트 활성화 or 비활성화 함수
    {
        if (active == true)
        {
            num[0].text = possible_num.ToString();
            num[1].text = possible_num.ToString();
        }
        num[0].gameObject.SetActive(active);
        num[1].gameObject.SetActive(active);
    }

    IEnumerator Dash(Vector3 direction) //능력 사용 후 슬라이드로 돌진 시 함수.
    {
        Vector3 target_pos = sp_t.position + direction * 2.5f;
        float time = 0;

        if(direction == Vector3.right)
        {
            while(Mathf.Cos(time) > 0)
            {
                time += Time.deltaTime * (Mathf.PI/2);
                sp_t.position = new Vector3(Mathf.Lerp(target_pos.x, sp_t.position.x, Mathf.Cos(time)), sp_t.position.y, sp_t.position.z);
                yield return null;
            }
        }
        else if (direction == Vector3.down)
        {
            while (Mathf.Cos(time) > 0)
            {
                time += Time.deltaTime * (Mathf.PI / 2);
                sp_t.position = new Vector3(sp_t.position.x, Mathf.Lerp(target_pos.y, sp_t.position.y, Mathf.Cos(time)), sp_t.position.z);
                yield return null;
            }
        }
    }
}
