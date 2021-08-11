using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PimpGuestMoving : MonoBehaviour
{
    // 스핏
    [Header("스핏")]
    public CharacterManager speat; // 스핏
    public SpeatAbility speatAbility; // 스핏ability
    public float changeDirectionCycle;
    public int speedUPTime;

    // 적들
    [Header("적")]
    public float speed;
    public float rotationSpeed;
    public float accVal; // 가속 값
    public int nextDirection;
    int rotVal = 0;
    float sameFloorRange = 0.5f;// y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
    CharacterController npc;
    Camera cam;
    Animator animator;

    private readonly int speedHash = Animator.StringToHash("Speed");

    [Header("속도 증가 여부")]
    public bool speedUp = false;

    // 대화
    private CanvasControl canvasCtrl;
    public bool talking = false;

    void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;

        npc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        //npc.GetComponent<Collider>().isTrigger = true;

        speat = DataController.instance_DataController.speat;

        cam = DataController.instance_DataController.cam;
        
        Invoke("Think", 0);

    }

    void Update()
    {

        if (!npc.isGrounded) npc.Move(transform.up * -1); // 중력


        npc.Move((Vector3.right * nextDirection * speed) * Time.deltaTime); // 적들 이동.

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, rotVal, 0), rotationSpeed * Time.deltaTime); // 적들 이동 방향에 따른 회전

        if (talking) nextDirection = 0; // 스핏하고 대화중일때 멈추게


        if (!speedUp) // SpeedUpFunc() 한번 수행될 수 있게.
        {
            if (CheckSameFloorWithSpeat() && CheckCamera() && (speatAbility.isAbility || speatAbility.isHiding))
            {
                speedUp = true;
                StartCoroutine(SpeedUpFunc());

            }
        }
        if (gameObject.name.Equals("Pimp_1")) print("rotVal: " + rotVal);


    }


    void Think() // 방향 설정.
    {

        if (!talking) nextDirection = (int)Random.Range(-1, 2);
        else nextDirection = 0;

        if (nextDirection == 1) // 오른쪽으로 움직임
        {
            animator.SetFloat(speedHash, 1.0f);
            rotVal = 90;
        }
        else if (nextDirection == -1) // 왼쪽으로 움직임
        {
            animator.SetFloat(speedHash, 1.0f);
            rotVal = -90;
        }
        else if (nextDirection == 0) // 정면 바라보고 정지
        {
            animator.SetFloat(speedHash, 0.0f);
            rotVal = 180;
        }

        Invoke("Think", changeDirectionCycle);
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 대화시작
        if (transform.parent.name.Equals("Guest"))
        {
            if (hit.gameObject.name.Equals("Speat") && canvasCtrl.isPossibleCnvs)
            {
                print("스핏과 만남");
                talking = true;
                canvasCtrl.selectedGuest = gameObject;

                //모든 캐릭터 이동 중지
                //모든 캐릭터 talking 켜주면 될 거 같은데

                DataController.instance_DataController.LoadData(transform.parent.name, gameObject.name + ".json");
                //수정
                CanvasControl.instance_CanvasControl.StartConversation();

            }

        }
        else if (transform.name.Equals("Pimp"))
        {
            if (hit.gameObject.name.Equals("Speat"))
            {
                print("포주와 마주침. 게임 종료");

                DataController.instance_DataController.ChangeMap(DataController.instance_DataController.mapCode);

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

    // pimp나 guest가 카메라에 걸리는지 확인하는 함수
    private bool CheckCamera()
    {
        Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }

    // pimp나 guest가 같은 층에 있는 지 bool값 반환
    private bool CheckSameFloorWithSpeat()
    {
        if (speat.transform.position.y - sameFloorRange <= gameObject.transform.position.y
            && gameObject.transform.position.y <= speat.transform.position.y + sameFloorRange) return true;
        else return false;
    }

    IEnumerator SpeedUpFunc()
    {
        //Debug.Log("능력 쓰는거 걸림");
        int i = speedUPTime;
        float tempSpeed = speed;
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
