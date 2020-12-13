using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchManager : MonoBehaviour
{
    // 라우
    [Header("라우")]
    CharacterManager character;
    public float rauSpeed;
    public float acceleration_setValue;
    float acceleration;
    Vector3 forwardDir;

    // 소매치기
    [Header("소매치기")]
    GameObject robber;
    CharacterController characterController_robber;
    public float robberSpeed;

    // 판정
    public float clearDistance; // 소매치기~라우 clear 거리

    // Start is called before the first frame update
    void Start()
    {
        character = DataController.instance_DataController.currentChar;
        DataController.instance_DataController.isMapChanged = true;
        acceleration = acceleration_setValue;
        forwardDir = character.transform.position - DataController.instance_DataController.cam.transform.position;
        forwardDir.y = 0;
        robber.transform.position = character.transform.position + new Vector3(0, 0, 10); // 라우와 10만큼 떨어진 위치에 등장!
        if (robber) characterController_robber = robber.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!character)
        {
            character = DataController.instance_DataController.currentChar;
            DataController.instance_DataController.isMapChanged = true;
            acceleration = acceleration_setValue;
            forwardDir = character.transform.position - DataController.instance_DataController.cam.transform.position;
            robber.transform.position = character.transform.position + new Vector3(0, 0, 10); // 라우와 10만큼 떨어진 위치에 등장!
            if (robber) characterController_robber = robber.GetComponent<CharacterController>();
        }
        else
        {
            // 캐릭터매니저랑 라우랑 위치 일치시키기 => 충돌 감지때문
            gameObject.transform.position = character.transform.position;

            // 소매치기 이동
            characterController_robber.Move(new Vector3(0, 0, robberSpeed * Time.deltaTime));

            // 라우 이동
            character.ctrl.Move(new Vector3(0, 0, rauSpeed * Time.deltaTime));
            rauSpeed += acceleration;





            // 조이스틱 input없을때 forwadDir방향 바라보게하기
            if (DataController.instance_DataController.inputDirection == new Vector2(0, 0)) // 조이스틱 인풋 없을 때 forwadDir방향 바라보게하기
            {
                Quaternion targetRot = Quaternion.LookRotation(forwardDir);
                character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRot, Time.deltaTime * 2);

            }


        }


    }


    void OnEnterTrigger(Collider other)
    {

    }


}
