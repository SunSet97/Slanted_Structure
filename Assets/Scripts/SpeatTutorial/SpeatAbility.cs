using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeatAbility : MonoBehaviour
{
    public float test;

    [Header("-UI")]
    public Image abilityImage;  // 능력 시간 UI 이미지
    public Text abilityText;    // 통과한 벽 UI 텍스트
    public Image buttonImage;   // 능력 사용 버튼 UI 이미지

    [Header("-Variable")]
    public float setDuration = 10; // 능력 지속 시간 설정값
    private float duration = 10;   // 능력 지속 시간
    public float setCooltime = 3;  // 능력 사용 대기 시간 설정값
    private float cooltime = 0;    // 능력 사용 대기 시간
    public int maxWallNum = 5;     // 최대 벽 통과 갯수
    private int wallNum = 0;       // 벽 통과 횟수
    public bool isAbility = false; // 현재 능력 사용 여부
    public bool isPassingHor = false; // 벽을 통과중인지 여부
    public bool isPassingVer = false; // 벽을 통과중인지 여부

    private Vector3 fwdDir, bwdDir; // 전후방 방향 벡터
    private Vector3 upDir, downDir; // 위아래 방향 벡터
    private Vector3 cenPos;         // 스핏 중앙 위치
    private Vector3 fwdPos, bwdPos; // 스핏 전후방 위치
    private Vector3 upPos, downPos; // 스핏 위아래 위치
    private RaycastHit wallFwd, wallBack, wallBwd, wallFwdTmp, wallBwdTmp;
    private RaycastHit ceilingFace, ceilingInverse, ceilingTmp;
    private RaycastHit floorFace, floorInverse, floorTmp;
    private int i = 1, j = 1, k =1; // 탐색 인자
    private float force;            // 벽을 통과하는 힘
    private bool isUp = false, isDown = false;

    GameObject tempDoor; // 클릭 한 문 임시로 저장
    bool isFrontDoor = false; // 스핏이 문 앞에 있는지 여부
    bool isInRoom = false; // 스핏이 방 안이면 true
    public bool isHiding = false; // true면 숨고 있는 중
    float touchRange = 0.5f; // 터치(클릭) 허용 범위
    Ray touchDown;
    Ray touchUp;

    [Header("-Speat")]
    public CharacterManager speat;

    void Update()
    {
        // 스핏 확인
        if (speat != null)
        {
            fwdDir = speat.transform.forward; bwdDir = -speat.transform.forward;    // 전후방 방향 벡터
            upDir = speat.transform.up; downDir = -speat.transform.up;              // 위아래 방향 벡터

            cenPos = speat.transform.position + speat.transform.up * 0.2f;          // 스핏 중앙 위치
            fwdPos = cenPos + fwdDir * 0.3f; bwdPos = cenPos + bwdDir * 0.3f;       // 스핏 전후방 위치
            upPos = cenPos + upDir * 0.4f; downPos = cenPos + downDir * 0.1f;       // 스핏 위아래 위치

            ChangeIsAbility();  // 능력 사용 여부 판단
            Dash();             // 조건 만족시 대쉬

            // *숨기 능력 *     여기서 문 or 출구 클릭 입력 받음.
            if (Input.GetMouseButtonDown(0)) touchDown = Camera.main.ScreenPointToRay(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
            {
                touchUp = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (touchDown.origin.x - touchRange <= touchUp.origin.x && touchUp.origin.x <= touchDown.origin.x + touchRange
                    && touchDown.origin.y - touchRange <= touchUp.origin.y && touchUp.origin.y <= touchDown.origin.y + touchRange)
                {
                    RaycastHit hit;
                    Physics.Raycast(touchUp, out hit);
                    GameObject touchedObj = hit.transform.gameObject;

                    if (hit.collider != null && touchedObj.layer.Equals(11)) // 클릭한 오브젝트가 문 일때
                    {
                        CheckDoor(touchUp, touchedObj);

                    }
                    else if (hit.collider != null && touchedObj.name.Equals("goal"))
                    {
                        CheckGoal(touchedObj);
                    }

                }
            }
        }
        else
        {
            speat = DataController.instance_DataController.speat;
        }
    }

    // 능력 사용 여부 판단
    private void ChangeIsAbility()
    {
        // 능력 사용중이 아닐때
        if (!isAbility)
        {
            isPassingVer = false;
            isPassingHor = false;
            speat.gameObject.layer = 0;
            // 사용 대기 시간 계산
            if (cooltime > 0) cooltime -= Time.deltaTime;
            else if (cooltime <= 0) cooltime = 0;
            abilityImage.fillAmount = cooltime / setCooltime; // 쿨타임 시간에 맞춰 UI 변화

            abilityText.text = null;// 벽 통과 횟수 숨김

            buttonImage.color = Color.black; // 터치 이미지 불투명
        }
        // 능력 사용중일 때
        if (isAbility)
        {
            // 지속 시간 계산
            if (duration > 0) duration -= Time.deltaTime;
            else if (duration <= 0) { isAbility = false; duration = 0; } // 지속 시간 초과시 능력 자동 종료 및 0 고정
            abilityImage.fillAmount = (setDuration - duration) / setDuration; // 지속 시간에 맞춰 UI 변화

            if (wallNum < maxWallNum) abilityText.text = wallNum.ToString(); // 벽 통과 횟수 변화
            else { isAbility = false; abilityText.text = null; } // 벽 통과 횟수 초과시 능력 자동 종료 및 숨김

            buttonImage.color = Color.clear; // 터치 이미지 투명
        }

        // 캐릭터의 능력 사용 여부 변화
        speat.isAbility = isAbility;
    }

    // 능력 사용 버튼
    public void UseAbility()
    {
        if (speat != null)
        {
            // 능력 사용중이 아닐때
            if (!isAbility && cooltime <= 0)
            {
                isAbility = true;
                wallNum = 0; abilityText.text = "0";            // 벽 통과 횟수 초기화
                duration = setDuration; cooltime = setCooltime; // 시간 초기화
                abilityImage.fillAmount = 0;                    // UI 초기화
            }
            // 능력 사용중일 때
            else if (isAbility)
            {
                isAbility = false;
                wallNum = 0; abilityText.text = null;           // 벽 통과 횟수 초기화 및 숨김
                duration = setDuration; cooltime = setCooltime; // 시간 초기화
                abilityImage.fillAmount = 1;                    // UI 초기화
            }
                        
        }
    }

    // 디버깅용 기즈모 및 레이캐스트
    private void OnDrawGizmos()
    {
        if (speat != null)
        {
            // 전방 벽 전면 탐지
            int wallLayerMask = 1 << 11;
            if (Physics.Raycast(fwdPos, fwdDir, out wallFwd, float.MaxValue, wallLayerMask))
            {
                // 전방 첫번째 벽 전면 임시 저장
                if (wallFwdTmp.transform == null) { wallFwdTmp = wallFwd; }
                // 벽이 바뀌면 변수 초기화 후 재탐색
                else if (wallFwdTmp.transform != wallFwd.transform) { i = 1; wallFwdTmp = wallFwd; }

                Gizmos.color = Color.red; Gizmos.DrawLine(fwdPos, wallFwd.point); Gizmos.DrawWireSphere(wallFwd.point, 0.1f); // 디버깅

                // 전방 벽 후면 탐지
                if (Physics.Raycast(wallFwd.point + fwdDir * 5 + bwdDir * i, bwdDir, out wallBack, float.MaxValue, wallLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(wallBack.point, 0.1f); // 디버깅

                    force = 5 + Vector3.Distance(wallFwd.point, wallBack.point) * 4f; // 벽 통과할 힘은 벽 두께에 따라 결정

                    if (wallFwd.transform != wallBack.transform) i++; // 같은 벽을 가리킬때까지 탐색
                }
            }
            // 후방 벽 탐지
            if (Physics.Raycast(bwdPos, bwdDir, out wallBwd, float.MaxValue, wallLayerMask))
            {
                // 후방 첫번째 벽 임시 저장
                if (wallBwdTmp.transform == null) { wallBwdTmp = wallBwd; }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (wallBwdTmp.transform != wallBwd.transform)
                {
                    i = 1;
                    wallBwdTmp = wallBwd;
                    if (!isPassingVer)
                    {
                        speat.CannotWallPass(9);
                        speat.moveHorDir = Vector3.zero;
                        isPassingHor = false;
                        wallNum++;
                    }
                }
                Gizmos.color = Color.red; Gizmos.DrawLine(bwdPos, wallBwd.point); Gizmos.DrawWireSphere(wallBwd.point, 0.1f); // 디버깅
            }

            // 천장 벽 전면 탐지
            int floorLayerMask = 1 << 12;
            if (Physics.Raycast(upPos, upDir, out ceilingFace, float.MaxValue, floorLayerMask))
            {
                // 천장 첫번째 벽 전면 임시 저장
                if (ceilingTmp.transform == null) { ceilingTmp = ceilingFace; }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (ceilingTmp.transform != ceilingFace.transform) {
                    j = 1;
                    ceilingTmp = ceilingFace;
                    if (isDown)
                    {
                        isDown = false;
                        speat.CannotWallPass(10);
                        isPassingVer = false;
                        wallNum++;
                    }
                }

                Gizmos.color = Color.red; Gizmos.DrawLine(upPos, ceilingFace.point); Gizmos.DrawWireSphere(ceilingFace.point, 0.1f); // 디버깅

                // 천장 벽 후면 탐지
                if (Physics.Raycast(ceilingFace.point + upDir * 5 + downDir * j, downDir, out ceilingInverse, float.MaxValue, floorLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(ceilingInverse.point, 0.1f); // 디버깅

                    force = 8 + Vector3.Distance(ceilingFace.point, ceilingInverse.point) * 4f; // 벽 통과할 힘은 벽 두께에 따라 결정

                    if (ceilingFace.transform != ceilingInverse.transform) j++; // 같은 벽을 가리킬때까지 탐색
                }
            }
            // 바닥 벽 전면 탐지
            if (Physics.Raycast(downPos, downDir, out floorFace, float.MaxValue, floorLayerMask))
            {
                // 바닥 첫번째 벽 전면 임시 저장
                if (floorTmp.transform == null) { floorTmp = floorFace; }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (floorTmp.transform != floorFace.transform)
                {
                    k = 1;
                    floorTmp = floorFace;
                    if (isUp)
                    {
                        isUp = false;
                        speat.CannotWallPass(10);
                        speat.moveVerDir = Vector3.zero;
                        isPassingVer = false;
                        wallNum++;
                    }
                }

                Gizmos.color = Color.red; Gizmos.DrawLine(upPos, floorFace.point); Gizmos.DrawWireSphere(floorFace.point, 0.1f); // 디버깅

                // 바닥 벽 후면 탐지
                if (Physics.Raycast(floorFace.point + downDir * 5 + upDir * k, upDir, out floorInverse, float.MaxValue, floorLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(floorInverse.point, 0.1f); // 디버깅

                    force = 2 + Vector3.Distance(floorFace.point, floorInverse.point) * 4f; // 벽 통과할 힘은 벽 두께에 따라 결정

                    if (floorFace.transform != floorInverse.transform) k++; // 같은 벽을 가리킬때까지 탐색
                }
            }
        }
    }

    private void Dash()
    {
        if (!isPassingHor && !isPassingVer && isAbility)
        {
            if (speat.joyStick.Horizontal < -0.7f && Vector3.Distance(cenPos, wallFwd.point)< 0.5f)
            {
                speat.moveHorDir = -Vector3.right * 5;
                speat.CanWallPass(9); //벽을 통과
                isPassingHor = true;
            }
            if (speat.joyStick.Horizontal > 0.7f && Vector3.Distance(cenPos, wallFwd.point) < 0.5f)
            {
                speat.moveHorDir = Vector3.right * 5;
                speat.CanWallPass(9); //벽을 통과
                isPassingHor = true;
            }
            if (speat.joyStick.Vertical < -0.7f)
            {
                speat.moveVerDir = -Vector3.up * 5;
                speat.CanWallPass(10); //벽을 통과
                isPassingVer = true;
                isDown = true;
            }
            if (speat.joyStick.Vertical > 0.7f)
            {
                speat.moveVerDir = Vector3.up * 10;
                speat.CanWallPass(10); //벽을 통과
                isPassingVer = true;
                isUp = true;
            }
        }
    }

    private void CheckGoal(GameObject goal)
    {
        Vector3 goalPos = goal.transform.position;
        Vector3 speatPos = speat.transform.position;
        float clearRange = 2.0f; // 출구로부터 어느 범위 내에 있어야지 clear

        if (goalPos.y - clearRange <= speatPos.y && speatPos.y <= goalPos.y + clearRange
            && goalPos.x - clearRange <= speatPos.x && speatPos.x <= goalPos.x + clearRange)
        {
            Debug.Log("!!스핏 튜토리얼 - 클리어!!");
            // 다음 맵으로 넘어가기 ~~
        }

    }

    private void CheckDoor(Ray ray, GameObject door)
    {
        Vector3 target;
        Vector3 doorPos = door.transform.position;
        Vector3 speatPos = speat.transform.position;
        float distacne = 2.0f;
        float frontDoorRange = 0.5f;

        Debug.Log("Touch the Door");

        if (!isInRoom && doorPos.x - frontDoorRange <= speatPos.x && speatPos.x <= doorPos.x + frontDoorRange) isFrontDoor = true;
        else isFrontDoor = false;

        if (!tempDoor)
        {
            tempDoor = door;
            // 문으로 들어가는 방향으로 target설정
            if (isFrontDoor && !isHiding && !isInRoom)
            {
                target = new Vector3(speatPos.x, speatPos.y, speatPos.z + distacne);
                Debug.Log("들어간다..");
                StartCoroutine(Hiding(target));
            }

        }
        else if (tempDoor && tempDoor == door)
        {
            tempDoor = null;
            // 문에서 나오는 방향으로 target설정
            if (!isFrontDoor && !isHiding && isInRoom)
            {
                target = new Vector3(speatPos.x, speatPos.y, speatPos.z - distacne);
                Debug.Log("나온다..");
                StartCoroutine(Hiding(target));
            }

        }

    }

    IEnumerator Hiding(Vector3 target)
    {

        isHiding = true;

        if (isFrontDoor) isFrontDoor = false; // 능력써서 방 안으로 들어올 때
        else isFrontDoor = true; // 능력써서 방 밖으로 나올 때

        if (isInRoom) isInRoom = false; // 능력써서 방 밖으로 나올 때
        else isInRoom = true; // 능력써서 방안으로 들어올 때

        speat.Change_Position(false);
        while (!speat.transform.position.Equals(target))
        {
            speat.transform.position = Vector3.MoveTowards(speat.transform.position, target, 0.03f); // 마지막 파라미터는 숨을 때 속도!
            yield return new WaitForSeconds(0f);
        }
        speat.Change_Position(true);
        isHiding = false;

    }
}
