using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlleyInMarketManager : MonoBehaviour
{
    [Header("회전 각 크기")]
    public float rotationValueScale = 45;
    [Header("카메라 회전 속도")]
    public float camRotationSpeed;
    [Header("웨이포인트")]
    public Waypoint waypoint;

    float acceptedRotationValue;

    private CharacterManager character;

    bool triggerRoateView = false;
    bool triggerAutoMove = false;

    float angle = 0;
    Vector2 lastInput; // 플레이어의 마지막 조이스틱 인풋 => 이 방향으로 캐릭터 자동 이동시킴
    

    // Start is called before the first frame update
    void Start()
    {
        DataController.instance_DataController.isMapChanged = true;
        character = DataController.instance_DataController.currentChar;
    }

    // Update is called once per frame
    void Update()
    {

        if (!character)
        {
            character = DataController.instance_DataController.currentChar;
        }
        else if (character)
        {
            // 매니저 스크립트랑  캐릭터랑 일치시키기 => 충돌 감지때문에!
            gameObject.transform.position = character.transform.position;

            // 카메라 뷰 변경
            RotateViewPointFunction();

            // 마지막 waypoint를 지나면, 조이스틱 인풋없이 움직임
            AutoMoving();

        }

    }

    private void OnTriggerEnter(Collider other)
    {

        // 카메라 시점 바꾸기
        if (other.name == "TransformView_TriggerCollider")
        {
            StartCoroutine(WaitTime(0.01f));
        }
        if (other.name == "automaticMove_TriggerCollider")
        {
            lastInput = DataController.instance_DataController.inputDirection;
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);
            StartCoroutine(AutoMoveCoroutine());
        }
    }

    #region 카메라 회전
    private void RotateViewPointFunction()
    {
        // acceptedRotationValue < 0 이면, 반시계방향으로 회전하기(마지막 조이스틱 입력이 오른쪽) acceptedRotationValue > 0 이면, 시계방향으로 회전하기!(마지막 조이스틱 입력이 왼쪽)

        if (triggerRoateView)
        {
            // 카메라 회전하는 동안 조이스틱 입력 (0,0)으로 해서 멈추게 한 후 조이스틱 끄기
            DataController.instance_DataController.joyStick.input = new Vector2(0, 0);
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);

            // 카메라 무빙 끄기 => 안끄면 회전 안됨.
            DataController.instance_DataController.cam.gameObject.GetComponent<Camera_Moving>().enabled = false;

            // 카메라 회전! => 휙휙 말고 부드럽게 회전
            if(acceptedRotationValue > 0) DataController.instance_DataController.cam.gameObject.transform.RotateAround(character.transform.position, Vector3.up, camRotationSpeed * Time.deltaTime);
            else if(acceptedRotationValue < 0) DataController.instance_DataController.cam.gameObject.transform.RotateAround(character.transform.position, Vector3.up, -camRotationSpeed * Time.deltaTime);

            angle += Time.deltaTime * camRotationSpeed;
            // angle값이 rotationValueScale과 같아지면 회전 끝!
            if (Mathf.Round(angle) >= rotationValueScale)
            {
                triggerRoateView = false;
                angle = 0; // 다시 0으로 초기화
                DataController.instance_DataController.camDis = DataController.instance_DataController.cam.transform.position - character.transform.position;
                DataController.instance_DataController.rot = new Vector3(DataController.instance_DataController.rot.x, DataController.instance_DataController.rot.y + acceptedRotationValue, DataController.instance_DataController.rot.z);
                DataController.instance_DataController.cam.gameObject.GetComponent<Camera_Moving>().enabled = true;
                DataController.instance_DataController.joyStick.gameObject.SetActive(true); // 조이스틱 보이게 하기

            }

        }
        else if (!triggerRoateView)
        {
            if (DataController.instance_DataController.inputDirection.x > 0) acceptedRotationValue = -rotationValueScale; // 조이스틱 입력이 오른쪽
            else if (DataController.instance_DataController.inputDirection.x < 0) acceptedRotationValue = rotationValueScale; // 조이스틱 입력이 왼쪽
        }

    }
    #endregion

    #region 마지막 조이스틱 input으로 이동
    private void AutoMoving()
    {
        if (triggerAutoMove)
        {
            DataController.instance_DataController.inputDirection = lastInput;

        }
    }
    #endregion

    #region waitTime 코로틴
    IEnumerator WaitTime(float time)
    {
        yield return new WaitForSeconds(time);
        triggerRoateView = true;
    }
    #endregion

    #region 자동 이동 코루틴
    IEnumerator AutoMoveCoroutine()
    {
        triggerAutoMove = true;
        yield return new WaitUntil(() => waypoint.checkedWaypoint == waypoint.waypoints[waypoint.waypoints.Count - 1]);
        triggerAutoMove = false;
        DataController.instance_DataController.joyStick.gameObject.SetActive(true);
        DataController.instance_DataController.inputDirection = new Vector2(0, 0);
    }
    #endregion
}
