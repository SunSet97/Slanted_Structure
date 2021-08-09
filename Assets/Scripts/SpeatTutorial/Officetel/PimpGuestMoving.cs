using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PimpGuestMoving : MonoBehaviour
{
    // 스핏
    public CharacterManager speat; // 스핏
    public SpeatAbility speatAbility; // 스핏ability
    public float changeDirectionCycle;
    public int speedUPTime;
    bool isGround = false; // 스핏이 능력 쓸 때 적들도 같이 아래로 내려오는 것을 막기 위함.
    bool isTriggerRotation = false;

    // 적들
    CharacterController npc;
    public float speed;
    public float rotationSpeed = 4.5f;
    public float accVal; // 가속 값
    public int nextDirection;
    float rotation;
    int rotVal = 0;
    bool isRotating = false;
    public string who; // 적 유형

    // 대화
    private CanvasControl canvasCtrl;
    bool talking = false;

    // 스핏하고 같은 층에 있는지 여부
    bool onSameFloor = false;
    public bool onSameFloorAndScreen = false;
    float sameFloorRange = 0.5f;// y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
    public Camera cam;
    float tempSpeed;
    public bool speedUp = false;
    float cntAngle = 0f;

    // 애니메이터
    Animator animator;

    void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;

        npc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        //npc.GetComponent<Collider>().isTrigger = true;

        speat = DataController.instance_DataController.currentChar;

        cam = DataController.instance_DataController.cam;

        rotation = gameObject.transform.rotation.y;
        Invoke("Think", 0);

    }

    void Update()
    {

        if (!npc.isGrounded) npc.Move(transform.up * -1); // 중력
        if (!speat) speat = DataController.instance_DataController.currentChar;

        npc.Move((Vector3.right * nextDirection * speed) * Time.deltaTime); // 적들 이동.

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, rotVal, 0)), rotationSpeed * Time.deltaTime); // 적들 이동 방향에 따른 회전

        if (talking) nextDirection = 0; // 스핏하고 대화중일때 멈추게

        if (talking)
        {
            //if (Input.GetMouseButtonDown(0))
            //{
            //    if (!canvasCtrl.isPossibleCnvs)
            //    {
            //        CanvasControl.instance_CanvasControl.UpdateWord();
            //    }
            //}
        }

        CheckSameFloorWithSpeat(); // 스핏과 같은 층에 있는지 확인.

        if (onSameFloor && CheckCamera()) onSameFloorAndScreen = true; // 스핏과 같은 층에 있으면서 카메라에 걸리는지 확인.
        else onSameFloorAndScreen = false;

        if (onSameFloorAndScreen && (speatAbility.isAbility || speatAbility.isHiding))
        {
            if (!speedUp) // SpeedUpFunc() 한번 수행될 수 있게.
            {
                speedUp = true;
                StartCoroutine(SpeedUpFunc());
            }

        }
        if (gameObject.name == "Pimp_1") print("rotVal: " + rotVal);


    }


    void Think() // 방향 설정.
    {

        if (!talking) nextDirection = (int)Random.Range(-1, 2);
        else nextDirection = 0;

        if (nextDirection == 1) // 오른쪽으로 움직임
        {
            animator.SetFloat("Speed", 1.0f);
            rotVal = 90;
        }
        else if (nextDirection == -1) // 왼쪽으로 움직임
        {
            animator.SetFloat("Speed", 1.0f);
            rotVal = -90;
        }
        else if (nextDirection == 0) // 정면 바라보고 정지
        {
            animator.SetFloat("Speed", 0.0f);
            rotVal = 180;
        }

        Invoke("Think", changeDirectionCycle);
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 대화시작
        if (transform.parent.name == "Guest")
        {
            if (hit.gameObject.name.Equals("Speat") && canvasCtrl.isPossibleCnvs)
            {
                print("스핏과 만남");
                talking = true;
                canvasCtrl.selectedGuest = gameObject;


                DataController.instance_DataController.LoadData(transform.parent.name, gameObject.name + ".json");
                //수정
                CanvasControl.instance_CanvasControl.StartConversation();

            }

        }


        if (transform.name == "Pimp")
        {
            if (hit.gameObject.name.Equals("Speat")) {
                print("포주와 마주침. 게임 종료");
                //DataController.instance_DataController.isMapChanged = true;

            }
        }


        if (hit.gameObject.CompareTag("NPC"))
        {
            nextDirection *= -1;

            if (nextDirection == 1)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
            else if (nextDirection == -1)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
            }

        }
        else if (hit.gameObject.name == "Speat")
        {
            nextDirection = 0;
        }

    }


    private bool CheckCamera()
    { // pimp나 guest가 카메라에 걸리는지 확인하는 함수.
        Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }

    private void CheckSameFloorWithSpeat()
    {
        if (speat != null)
        {
            if (speat.transform.position.y - sameFloorRange <= gameObject.transform.position.y
                && gameObject.transform.position.y <= speat.transform.position.y + sameFloorRange) onSameFloor = true;
            else onSameFloor = false;
        }
    }

    IEnumerator SpeedUpFunc()
    {
        //Debug.Log("능력 쓰는거 걸림");
        int i = speedUPTime;
        tempSpeed = speed;
        speed += accVal;
        while (i >= 0)
        {
            yield return new WaitForSeconds(speedUPTime);
            i--;
        }
        speed = tempSpeed;
        speedUp = false;
    }
}
