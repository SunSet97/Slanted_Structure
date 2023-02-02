using System.Collections;
using Data;
using UnityEngine;
using Utility.Core;

public class PimpGuestMoving : MonoBehaviour
{
    public PimpGameManager pimpGameManager;

    // 스핏
    [Header("스핏")]
    public SpeatAbility speatAbility;
    public float changeDirectionCycle;
    public int speedUpTime;

    // 적들
    [Header("적")] 
    public float speed;
    public float rotationSpeed;
    public float accVal; // 가속 값
    public int nextDirection;
    
    private int rotVal = 180;
    private float sameFloorRange = 0.5f; // y값 얼마차이까지 스핏하고 같은 층에 있다고 판단할 것인지 범위.
    
    [Header("속도 증가 여부")]
    public bool speedUp;

    public TextAsset jsonFile;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private CharacterController npc;

    void Start()
    {
        npc = GetComponent<CharacterController>();
        var animator = GetComponent<Animator>();
        animator.SetFloat(speedHash, 0.0f);
    }

    public void Move(bool isPlay)
    {
        if (!npc.isGrounded)
        {
            npc.Move(transform.up * -1); // 중력
        }

        if (!isPlay)
        {
            return;
        }

        npc.Move((Vector3.right * nextDirection * speed) * Time.deltaTime);

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
    }


    public void Think() // 방향 설정.
    {
        if (!pimpGameManager.IsPlay)
        {
            Invoke("Think", changeDirectionCycle);
            return;
        }

        var animator = GetComponent<Animator>();
        nextDirection = Random.Range(-1, 2);
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
        if (!pimpGameManager.IsPlay)
        {
            return;
        }
        if (hit.collider.TryGetComponent(out CharacterManager characterManager) && characterManager.who.Equals(CustomEnum.Character.Speat_Adult))
        {
            var speat = DataController.instance.GetCharacter(CustomEnum.Character.Speat_Adult);
            speat.gameObject.layer = LayerMask.NameToLayer("Player");
            
            if (jsonFile)
            {
                speat.IsMove = false;
                
                pimpGameManager.EndPlay();
                
                DialogueController.instance.SetDialougueEndAction(() =>
                {
                    speat.IsMove = true;
                    DataController.instance.ChangeMap(DataController.instance.mapCode);
                });
                DialogueController.instance.StartConversation(jsonFile.text);
            }
            else
            {
                DataController.instance.ChangeMap(DataController.instance.mapCode);
            }
        }
        else if (hit.gameObject.CompareTag("NPC"))
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
    }

    // pimp나 guest가 카메라에 걸리는지 확인하는 함수
    private bool CheckCamera()
    {
        var cam = DataController.instance.cam;
        Vector3 screenPoint = cam.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 &&
                        screenPoint.y < 1;

        return onScreen;
    }

    // pimp나 guest가 같은 층에 있는 지 bool값 반환
    private bool CheckSameFloorWithSpeat()
    {
        var speat = DataController.instance.GetCharacter(CustomEnum.Character.Speat_Adult);
        if (speat.transform.position.y - sameFloorRange <= gameObject.transform.position.y
            && gameObject.transform.position.y <= speat.transform.position.y + sameFloorRange)
        {
            return true;
        }
        
        return false;
    }

    private IEnumerator SpeedUpFunc()
    {
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
