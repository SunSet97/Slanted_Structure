using System.Collections;
using Data;
using UnityEngine;

public class PimpGuestMoving : MonoBehaviour
{
    public PimpGameManager pimpGameManager;
    // 스핏
    [Header("스핏")]
    public SpeatAbility speatAbility; // 스핏ability
    public float changeDirectionCycle;
    public int speedUpTime;

    // 적들
    [Header("적")]
    public float speed;
    public float rotationSpeed;
    public float accVal; // 가속 값
    public int nextDirection;
    int rotVal = 0;
    float sameFloorRange = 0.5f;// y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
    private CharacterController npc;
    
    private Animator animator;

    [Header("속도 증가 여부")]
    public bool speedUp = false;


    public TextAsset jsonFile;
    
    
    public readonly int speedHash = Animator.StringToHash("Speed");

    void Start()
    {
        npc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        // npc.GetComponent<Collider>().isTrigger = true;
        // cam = DataController.instance_DataController.cam;
        animator.SetFloat(speedHash, 0.0f);
        rotVal = 180;
    }

    public void Move(bool isMove)
    {
        if (!npc.isGrounded) npc.Move(transform.up * -1); // 중력

        if (isMove)
        {
            if (!pimpGameManager.isTalking)
                npc.Move((Vector3.right * nextDirection * speed) * Time.deltaTime); // 적들 이동.

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, rotVal, 0),
                rotationSpeed * Time.deltaTime); // 적들 이동 방향에 따른 회전

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
    }


    public void Think() // 방향 설정.
    {
        if (pimpGameManager.isTalking)
        {
            Invoke("Think", changeDirectionCycle);
            return;
        }

        nextDirection = (int) Random.Range(-1, 2);
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

        // else
        // {
        //
        //     animator.SetFloat(speedHash, 0.0f);
        //     rotVal = 180;
        // }

        Invoke("Think", changeDirectionCycle);
    }

    private void SetTalking(bool isTalking)
    {
        var speat = DataController.instance.GetCharacter(CustomEnum.Character.Speat);
        speat.gameObject.layer = 0;
        for (int i = 0; i < transform.parent.parent.childCount; i++)
        {
            for(int j = 0; j < transform.parent.parent.GetChild(i).childCount; j++)
            {
                pimpGameManager.isTalking = isTalking;
                DataController.instance.currentMap.ui.gameObject.SetActive(!isTalking);
                speat.IsMove = !isTalking;
                if (isTalking)
                {
                    animator.SetFloat(speedHash, 0.0f);
                }
                else
                {
                    animator.SetFloat(speedHash, nextDirection);
                }
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 대화시작
        if (transform.parent.name.Equals("Guest"))
        {
            if (hit.gameObject.name.Equals("Speat_Adult") && !CanvasControl.instance.isInConverstation)
            {
                print("스핏과 만남");

                SetTalking(true);
                var speat = DataController.instance.GetCharacter(CustomEnum.Character.Speat);
                speat.IsMove = false;
                DialogueController.instance.SetDialougueEndAction(() =>
                {
                    SetTalking(false);
                    speat.IsMove = true;
                    
                    DataController.instance.ChangeMap(DataController.instance.mapCode);
                });
                if (jsonFile) DialogueController.instance.StartConversation(jsonFile.text);
            }

        }
        else if (transform.name.Equals("Pimp"))
        {
            if (hit.gameObject.name.Equals("Speat"))
            {
                print("포주와 마주침. 게임 종료");
                DataController.instance.ChangeMap(DataController.instance.mapCode);
                return;
            }
        }


        if (hit.gameObject.CompareTag("NPC"))
        {
            nextDirection *= -1;

            if (nextDirection == 1)
            {
                rotVal = 90;
            }
            else if (nextDirection == -1)
            {
                rotVal = -90;
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
        var cam = DataController.instance.cam;
        Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }

    // pimp나 guest가 같은 층에 있는 지 bool값 반환
    private bool CheckSameFloorWithSpeat()
    {
        var speat = DataController.instance.GetCharacter(CustomEnum.Character.Speat);
        if (speat.transform.position.y - sameFloorRange <= gameObject.transform.position.y
            && gameObject.transform.position.y <= speat.transform.position.y + sameFloorRange) return true;
        else return false;
    }

    private IEnumerator SpeedUpFunc()
    {
        //Debug.Log("능력 쓰는거 걸림");
        int i = speedUpTime;
        float tempSpeed = speed;
        speed += accVal;
        while (i >= 0)
        {
            yield return new WaitForSeconds(speedUpTime);
            i--;
        }
        speed = tempSpeed;
        speedUp = false;
    }
}
