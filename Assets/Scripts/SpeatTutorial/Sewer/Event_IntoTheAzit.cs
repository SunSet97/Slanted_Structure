using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_IntoTheAzit : MonoBehaviour
{
    public MapData mapdata;
    public Transform startPoint; // 시작 위치
    public Transform endPoint; // 도착 위치
    [Range(5, 20)]
    public int moveSpeed = 10; // 이동 속도
    private bool isCoroot = false; // 코루틴 실행 여부

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == DataController.instance_DataController.currentChar.name)
        {
            other.GetComponent<CharacterManager>().PickUpCharacter();
            if (!isCoroot)
            {
                isCoroot = true;
                StartCoroutine(MoveToAzit(other));
            }
        }
    }

    IEnumerator MoveToAzit(Collider other)
    {
        other.transform.position = startPoint.position; // 시작 위치로 이동
        other.transform.LookAt(endPoint);
        // 도착 위치까지 반복
        Vector3 tick = (endPoint.position - startPoint.position) / 100f;
        for (int i = 0; i < 100; i++)
        {
            other.GetComponent<Animator>().SetFloat("Speed", 1f);
            other.transform.position += tick;
            yield return new WaitForSeconds(0.05f / moveSpeed);
        }
        mapdata.method = MapData.JoystickInputMethod.AllDirection; // 이동 방식 변경
        other.GetComponent<CharacterManager>().UseJoystickCharacter(); // 캐릭터 이동활성화
        yield return new WaitForSeconds(0.5f); // 잠시 대기
        isCoroot = false; // 코루틴 초기화
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue * 0.7f;
        Gizmos.DrawSphere(startPoint.position, 0.5f);
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        Gizmos.DrawSphere(endPoint.position, 0.5f);
    }
}
