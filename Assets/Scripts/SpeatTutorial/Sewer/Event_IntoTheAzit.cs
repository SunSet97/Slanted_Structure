using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_IntoTheAzit : MonoBehaviour
{
    public Transform startPoint; // 시작 위치
    public Transform endPoint; // 도착 위치
    [Range(5, 20)]
    public int moveSpeed = 10; // 이동 속도

    private static readonly int Speed = Animator.StringToHash("Speed");

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out CharacterManager character)) return;

        character.PickUpCharacter();
        GetComponent<Collider>().enabled = false;
        DataController.instance_DataController.InitializeJoystic(false);
        StartCoroutine(MoveToAzit(other));
    }

    IEnumerator MoveToAzit(Collider other)
    {
        other.transform.position = startPoint.position; // 시작 위치로 이동
        other.transform.LookAt(endPoint);
        // 도착 위치까지 반복
        Vector3 tick = (endPoint.position - startPoint.position) * 0.01f;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.05f / moveSpeed);
        other.GetComponent<Animator>().SetFloat(Speed, 1f);
        for (int i = 0; i < 100; i++)
        {
            other.transform.position += tick;
            yield return waitForSeconds;
        }
        //DataController.instance_DataController.currentMap.method = MapData.JoystickInputMethod.AllDirection; // 이동 방식 변경
        other.GetComponent<CharacterManager>().UseJoystickCharacter(); // 캐릭터 이동활성화
        other.GetComponent<Animator>().SetFloat(Speed, 0f);
        yield return new WaitForSeconds(0.5f); // 잠시 대기
        DataController.instance_DataController.InitializeJoystic(true);
        GetComponent<Collider>().enabled = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue * 0.7f;
        Gizmos.DrawSphere(startPoint.position, 0.5f);
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        Gizmos.DrawSphere(endPoint.position, 0.5f);
    }
}
