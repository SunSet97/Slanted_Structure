using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlleyInMarketManager : MonoBehaviour
{
    public float rotationSpeed = 10;
    bool triggerRoateView = false;
    bool triggerAutoMove = false;
    float angleReduction = 0;
    public GameObject pivot1;
    public GameObject pivot2;
    public CharacterManager character;
    public int collisionNum;
    Vector3 offset;
    public Waypoint waypoint;
    Vector2 lastInput;

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.name == "AlleyInMarketManager"){

            DataController.instance_DataController.isMapChanged = true;

            // 카메라 조정
            DataController.instance_DataController.camDis_x = 5;
            DataController.instance_DataController.camDis_z = 0;
            DataController.instance_DataController.rot.y = -90;

            // 캐릭터 맵쪽으로 ㄱㄱ
            DataController.instance_DataController.isMapChanged = true;
            

        }



    }

    // Update is called once per frame
    void Update()
    {
        if (triggerRoateView) {

            DataController.instance_DataController.cam.gameObject.transform.RotateAround(character.transform.position, Vector3.up, 45);
            
        }
        
        if (triggerAutoMove) {
            DataController.instance_DataController.inputDirection = lastInput;
            //character.joystickDir = new Vector2(1, 0);
            //character.ctrl.Move((character.moveHorDir + character.moveVerDir) * Time.deltaTime);

        }

    }


    private void OnTriggerEnter(Collider other) {

        // 카메라 시점 바꾸기
        if (gameObject.name == "TransformView_TriggerCollider" && other.gameObject.name == DataController.instance_DataController.currentChar.name)
        {
            StartCoroutine(RotateViewCoroutine());
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
    IEnumerator RotateViewCoroutine() {
        triggerRoateView = true;
        int rotationScale = 45;
        //yield return new WaitUntil(() => Mathf.Round(DataController.instance_DataController.rot.y) == Mathf.Round(tempRotationY));
        yield return new WaitUntil(() => rotationScale == Mathf.Round(angleReduction));
        triggerRoateView = false;
    }

    IEnumerator AutoMoveCoroutine() {
        triggerAutoMove = true;
        yield return new WaitUntil(() => waypoint.checkedWaypoint == waypoint.waypoints[waypoint.waypoints.Count - 1]);
        triggerAutoMove = false;
        DataController.instance_DataController.joyStick.gameObject.SetActive(true);
        DataController.instance_DataController.inputDirection = new Vector2(0,0);

    }
    
}
