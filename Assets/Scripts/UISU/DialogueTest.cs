using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTest : MonoBehaviour
{
    [SerializeField]
    private asdf a;

    [System.Serializable]
    private class asdf {
        public int a = 5;
        private string name = "asd";
    }

    JsontoString.JsonSong[] dialogue;
    Animator animator;
    public Text text;

    public float wordDelay = 0.5f;
    bool IsRead;

    [Header("헤드")]
    [SerializeField]
    private int index = 0;

    IEnumerator sequenceCorutine;
    IEnumerator skipCorutine;


    public void Start()
    {
        //DialogueData로 넣기?
        //dialogue = JsontoString.LoadJsonFromClassName<JsontoString.JsonSong>();
        animator = GetComponent<Animator>();
        //이렇게 하지말고 조금 무리하더라도 Find로 찾은 다음에 하기
        //이 스크립트는 따로 Dialogue System 오브젝트로 만드는 게 좋을듯
        //
    }

    //public IEnumerator StartDialogueSystem(string str)
    //{
    //    WaitUntil waitUntil = new WaitUntil(() =>
    //    {
    //        if (IsRead)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    });

    //    // 해당 대화의 텍스트 개수만큼
    //    for (int i = 0; i < dialogue.Length; i++)
    //    {
    //        sequenceCorutine = SequenceText();
    //        StartCoroutine(sequenceCorutine);

    //        //다 출력되고 누르거나 자동으로 넘어가기 전까지 대기하기
    //        yield return waitUntil;
    //    }

    //    yield return null;
    //}

    //public IEnumerator SequenceText()              //지문 텍스트 한글자식 연속 출력
    //{
    //    skipCorutine = WaitTexting(sequenceCorutine);                     //터치 스킵을 위한 터치 대기 코루틴 할당
    //    StartCoroutine(skipCorutine);                                       //터치 대기 코루틴 시작
    //    text.text = "";                                              //대화 지문 초기화
    //    foreach (char letter in text_.ToCharArray())                    //대화 지문 한글자씩 뽑아내기
    //    {
    //        text.text += letter;                                     //한글자씩 출력
    //        yield return new WaitForSeconds(wordDelay);                     //출력 딜레이
    //    }
    //    //기다리다가 모든 텍스트 출력시 코루틴 해제
    //    StopCoroutine(skipCorutine);                                        //지문 출력이 끝나면 터치 대기 코루틴 해제
    //    //한번 더 누를 경우 다음 대화로
    //}

    //public IEnumerator WaitTexting(IEnumerator sequenceText)
    //{
    //    //yield return new WaitUntil(() => { input이 있을 경우 });
    //    StopCoroutine(sequenceText);
    //    //모든 텍스트 출력
    //    //한번 더 누를 경우 다음 대화로
    //    yield return null;
    //}

    //public IEnumerator WaitNext()
    //{
    //    yield return null;
    //    //yield return new WaitUntil(() => Input이 있을 경우)
    //    //다음 텍스트로 넘기기
    //}

    //public void DisplayNext()
    //{
    //    if(index == dialogue.Length)
    //    {
    //        //끄기
    //    }
    //    StopCoroutine(sequenceCorutine);

    //    //dialog_cycles[index].info[number].check_read = true;            //현재 지문 읽음으로 표시
    //}


    //public IEnumerator seq_sentence(int index, int number)
    //{
    //    skip_seq = touch_wait(seq_, index, number);                     //터치 스킵을 위한 터치 대기 코루틴 할당
    //    StartCoroutine(skip_seq);                                       //터치 대기 코루틴 시작
    //    DialogT.text = "";                                              //대화 지문 초기화
    //    foreach (char letter in text_.ToCharArray())                    //대화 지문 한글자씩 뽑아내기
    //    {
    //        DialogT.text += letter;                                     //한글자씩 출력
    //        yield return new WaitForSeconds(delay);                     //출력 딜레이
    //    }

    //    StopCoroutine(skip_seq);                                        //지문 출력이 끝나면 터치 대기 코루틴 해제
    //    IEnumerator next = next_touch(index, number);                   //버튼 이외에 부분을 터치해도 넘어가는 코루틴 시작
    //    StartCoroutine(next);
    //}

    //public IEnumerator touch_wait(IEnumerator seq, int index, int number)//터치 대기 코루틴
    //{
    //    yield return new WaitForSeconds(0.3f);
    //    yield return new WaitUntil(() => Input.GetMouseButton(0));
    //    StopCoroutine(seq);                                              //대화 지문 코루틴 해제
    //    DialogT.text = text_;                                            //스킵시 모든 지문 한번에 출력
    //    IEnumerator next = next_touch(index, number);                    //대화 지문 코루틴 해제 됬기 때문에 다음 지문으로 가는 코루틴 시작
    //    StartCoroutine(next);
    //}

    public void down()
    {
        if (index <= dialogue.Length)
        {
            animator.SetTrigger(Animator.StringToHash(dialogue[index].expression.ToString()));
            text.text = dialogue[index].contents;
            Debug.Log(dialogue[index].name);
            Debug.Log(dialogue[index].expression);
            Debug.Log(dialogue[index].kind);
            Debug.Log(dialogue[index].contents);
            index++;
        }
    }
}
