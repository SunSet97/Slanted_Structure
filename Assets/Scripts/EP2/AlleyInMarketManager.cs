using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlleyInMarketManager : MonoBehaviour
{
    bool triggerRoateView = false;
    bool triggerAutoMove = false;
    float angleReduction = 0;
    float rotationValue = -45;
    public GameObject pivot1;
    public GameObject pivot2;
    public CharacterManager character;
    public int collisionNum;
    public Waypoint waypoint;
    Vector2 lastInput; // 플레이어의 마지막 조이스틱 인풋 => 이 방향으로 캐릭터 자동 이동시킴
    bool isPassed = false; // 이미 지나간 콜라이더 표시.

    // Start is called before the first frame update
    void Start()
    {
        DataController.instance_DataController.isMapChanged = true;

    }

    // Update is called once per frame
    void Update()
    {

        if (!character && DataController.instance_DataController.currentChar)
        {
            character = DataController.instance_DataController.currentChar;
        }


        if (triggerRoateView)
        {
            DataController.instance_DataController.rot.y = -45;
            //DataController.instance_DataController.camDis.x = 10;
//            DataController.instance_DataController.camDis.z = 10;
            //DataController.instance_DataController.cam.gameObject.transform.RotateAround(character.transform.position, Vector3.up, -45);
        }

        if (triggerAutoMove)
        {
            DataController.instance_DataController.inputDirection = lastInput;

        }

    }


    private void OnTriggerEnter(Collider other)
    {

        // 카메라 시점 바꾸기
        if (gameObject.name == "TransformView_TriggerCollider" && other.gameObject.name == DataController.instance_DataController.currentChar.name)
        {
            triggerRoateView = true;
        }
        if (gameObject.name == "automaticMove_TriggerCollider" && other.gameObject.name == DataController.instance_DataController.currentChar.name)
        {
            lastInput = DataController.instance_DataController.inputDirection;
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);
            StartCoroutine(AutoMoveCoroutine());
        }
    }

    // 카메라 시점 바꾸기
    // !! offset 해야함.
    IEnumerator RotateViewCoroutine()
    {

        triggerRoateView = true;
        yield return new WaitUntil(() => DataController.instance_DataController.rot.y == Mathf.Round(rotationValue));
        triggerRoateView = false;

    }

    IEnumerator AutoMoveCoroutine()
    {


        triggerAutoMove = true;
        yield return new WaitUntil(() => waypoint.checkedWaypoint == waypoint.waypoints[waypoint.waypoints.Count - 1]);
        triggerAutoMove = false;
        DataController.instance_DataController.joyStick.gameObject.SetActive(true);
        DataController.instance_DataController.inputDirection = new Vector2(0, 0);


    }


}
