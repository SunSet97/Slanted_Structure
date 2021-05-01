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
    public bool isPassingVerUp = false; // 벽을 통과중인지 여부
    public bool isPassingVerDown = false; // 벽을 통과중인지 여부

    private Vector3 fwdDir, bwdDir; // 전후방 방향 벡터
    private Vector3 upDir, downDir; // 위아래 방향 벡터
    private Vector3 cenPos;         // 스핏 중앙 위치
    private Vector3 fwdPos, bwdPos; // 스핏 전후방 위치
    private Vector3 upPos, downPos; // 스핏 위아래 위치
    private RaycastHit wallFwdFace, wallFwdInverse, wallFwdTmp;
    private RaycastHit wallBwdFace, wallBwdInverse, wallBwdTmp;
    private RaycastHit ceilingFace, ceilingInverse, ceilingTmp;
    private RaycastHit floorFace, floorInverse, floorTmp;
    private int fwdCnt = 1, bwdCnt = 1, ceilingCnt =1, floorCnt = 1; // 탐색 인자
    private float force;            // 벽을 통과하는 힘
    private bool isUp = false, isDown = false;

    public Interact_ObjectWithRau[] doors;
    int clickedDoorIndex;
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
            HideBehindDoor(); // 문 뒤로 숨기
            fwdDir = speat.transform.forward; bwdDir = -speat.transform.forward;    // 전후방 방향 벡터
            upDir = speat.transform.up; downDir = -speat.transform.up;              // 위아래 방향 벡터

            cenPos = speat.transform.position + speat.transform.up * 0.35f;          // 스핏 중앙 위치
            fwdPos = cenPos + fwdDir * 0.3f; bwdPos = cenPos + bwdDir * 0.3f;       // 스핏 전후방 위치
            upPos = cenPos + upDir * 0.5f; downPos = cenPos + downDir * 0.2f;       // 스핏 위아래 위치

            ChangeIsAbility();  // 능력 사용 여부 판단
            Dash();             // 조건 만족시 대쉬

            for (int i = 0; i < doors.Length; i++) {
                bool tmp = doors[i].isTouched;
                if (tmp) print("i: " + i + "  /  doors[i].isTouched: " + doors[i].isTouched);
            }


            /*
            if (Input.GetMouseButtonDown(0)) touchDown = Camera.main.ScreenPointToRay(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
            {
                touchUp = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (touchDown.origin.x - touchRange <= touchUp.origin.x && touchUp.origin.x <= touchDown.origin.x + touchRange
                    && touchDown.origin.y - touchRange <= touchUp.origin.y && touchUp.origin.y <= touchDown.origin.y + touchRange)
                {
                    RaycastHit hit;
                    Physics.Raycast(touchUp, out hit, Mathf.Infinity);
                    GameObject touchedObj = hit.collider.gameObject;
                    // 하..
                    print(hit.collider.name);
                    if (hit.collider != null && touchedObj.name.Equals("door")) // 클릭한 오브젝트가 문 일때
                    {
                        print(touchedObj.gameObject.name + "터치!!");
                        CheckDoor(touchUp, touchedObj);

                    }
                    else if (hit.collider != null && touchedObj.name.Equals("goal"))
                    {
                        CheckGoal(touchedObj);
                    }

                }
            }
            */

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
            isPassingHor = false;
            isPassingVerUp = false;
            isPassingVerDown = false;
            speat.gameObject.layer = 0;
            // 사용 대기 시간 계산
            if (cooltime > 0) cooltime -= Time.deltaTime;
            else if (cooltime <= 0) cooltime = 0;
            abilityImage.fillAmount = cooltime / setCooltime; // 쿨타임 시간에 맞춰 UI 변화
            buttonImage.fillAmount = 0;

            abilityText.text = null;// 벽 통과 횟수 숨김
        }
        // 능력 사용중일 때
        if (isAbility)
        {
            // 지속 시간 계산
            if (duration > 0) duration -= Time.deltaTime;
            else if (duration <= 0) { isAbility = false; duration = 0; } // 지속 시간 초과시 능력 자동 종료 및 0 고정
            buttonImage.fillAmount = duration / setDuration; // 지속 시간에 맞춰 UI 변화
            abilityImage.fillAmount = 0;

            if (wallNum < maxWallNum) abilityText.text = wallNum.ToString(); // 벽 통과 횟수 변화
            else { isAbility = false; abilityText.text = null; } // 벽 통과 횟수 초과시 능력 자동 종료 및 숨김
        }
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
                abilityImage.fillAmount = 1;                    // UI 초기화
                buttonImage.fillAmount = 0;                    // UI 초기화
            }
            // 능력 사용중일 때
            else if (isAbility)
            {
                isAbility = false;
                wallNum = 0; abilityText.text = null;           // 벽 통과 횟수 초기화 및 숨김
                duration = setDuration; cooltime = setCooltime; // 시간 초기화
                abilityImage.fillAmount = 0;                    // UI 초기화
                buttonImage.fillAmount = 1;                    // UI 초기화
            }                        
        }
    }

    // 디버깅용 기즈모 및 레이캐스트
    private void OnDrawGizmos()
    {
        if (speat != null)
        {
            Gizmos.DrawWireSphere(fwdPos, 0.1f);
            Gizmos.DrawWireSphere(bwdPos, 0.1f);
            Gizmos.DrawWireSphere(upPos, 0.1f);
            Gizmos.DrawWireSphere(downPos, 0.1f);
            
            // 전방 벽 전면 탐지
            int wallLayerMask = 1 << 11;
            if (Physics.Raycast(fwdPos, fwdDir, out wallFwdFace, float.MaxValue, wallLayerMask))
            {
                // 전방 첫번째 벽 전면 임시 저장
                if (wallFwdTmp.transform == null) { wallFwdTmp = wallFwdFace; }
                // 벽이 바뀌면 변수 초기화 후 재탐색
                else if (wallFwdTmp.transform != wallFwdFace.transform) { fwdCnt = 1; wallFwdTmp = wallFwdFace; }
                Gizmos.color = Color.red; Gizmos.DrawLine(fwdPos, wallFwdFace.point); Gizmos.DrawWireSphere(wallFwdFace.point, 0.1f); // 디버깅

                // 전방 벽 후면 탐지
                if (Physics.Raycast(wallFwdFace.point + fwdDir * 5 + bwdDir * fwdCnt, bwdDir, out wallFwdInverse, float.MaxValue, wallLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(wallFwdInverse.point, 0.1f); // 디버깅
                    if (wallFwdFace.transform != wallFwdInverse.transform) fwdCnt++; // 같은 벽을 가리킬때까지 탐색
                }
            }
            // 후방 벽 탐지
            if (Physics.Raycast(bwdPos, bwdDir, out wallBwdFace, float.MaxValue, wallLayerMask))
            {
                // 후방 첫번째 벽 임시 저장
                if (wallBwdTmp.transform == null) { wallBwdTmp = wallBwdFace; }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (wallBwdTmp.transform != wallBwdFace.transform)
                {
                    bwdCnt = 1;
                    wallBwdTmp = wallBwdFace;
                    if (isPassingHor)
                    {
                        speat.gameObject.layer = 0;
                        speat.moveHorDir = Vector3.zero;
                        isPassingHor = false;
                        wallNum++;
                    }
                }
                Gizmos.color = Color.red; Gizmos.DrawLine(bwdPos, wallBwdFace.point); Gizmos.DrawWireSphere(wallBwdFace.point, 0.1f); // 디버깅

                // 후방 벽 후면 탐지
                if (Physics.Raycast(wallBwdFace.point + fwdDir * 5 + bwdDir * bwdCnt, fwdDir, out wallBwdInverse, float.MaxValue, wallLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(wallBwdInverse.point, 0.1f); // 디버깅
                    if (wallBwdFace.transform != wallBwdInverse.transform) bwdCnt++; // 같은 벽을 가리킬때까지 탐색
                }
            }

            // 천장 벽 전면 탐지
            int floorLayerMask = 1 << 12;
            if (Physics.Raycast(upPos, upDir, out ceilingFace, float.MaxValue, floorLayerMask))
            {
                // 천장 첫번째 벽 전면 임시 저장
                if (ceilingTmp.transform == null) { ceilingTmp = ceilingFace; }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (ceilingTmp.transform != ceilingFace.transform)
                {
                    ceilingCnt = 1;
                    ceilingTmp = ceilingFace;
                    if (isPassingVerDown)
                    {
                        speat.gameObject.layer = 0;
                        speat.moveVerDir = Vector3.zero;
                        isPassingVerDown = false;
                        wallNum++;
                    }
                }
                Gizmos.color = Color.red; Gizmos.DrawLine(upPos, ceilingFace.point); Gizmos.DrawWireSphere(ceilingFace.point, 0.1f); // 디버깅

                // 천장 벽 후면 탐지
                if (Physics.Raycast(ceilingFace.point + upDir * 5 + downDir * ceilingCnt, downDir, out ceilingInverse, float.MaxValue, floorLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(ceilingInverse.point, 0.1f); // 디버깅
                    if (ceilingFace.transform != ceilingInverse.transform) ceilingCnt++; // 같은 벽을 가리킬때까지 탐색
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
                    floorCnt = 1;
                    floorTmp = floorFace;
                    if (isPassingVerUp)
                    {
                        speat.gameObject.layer = 0;
                        speat.moveVerDir = Vector3.zero;
                        isPassingVerUp = false;
                        wallNum++;
                    }
                }
                Gizmos.color = Color.red; Gizmos.DrawLine(upPos, floorFace.point); Gizmos.DrawWireSphere(floorFace.point, 0.1f); // 디버깅

                // 바닥 벽 후면 탐지
                if (Physics.Raycast(floorFace.point + downDir * 5 + upDir * floorCnt, upDir, out floorInverse, float.MaxValue, floorLayerMask))
                {
                    Gizmos.color = Color.blue; Gizmos.DrawWireSphere(floorInverse.point, 0.1f); // 디버깅
                    if (floorFace.transform != floorInverse.transform) floorCnt++; // 같은 벽을 가리킬때까지 탐색
                }
            }

            // 스핏 주위에 있는 obj_interaction감지 범위 보여줌
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(speat.transform.position, 5);
        }
    }

    private void Dash()
    {
        if (!isPassingHor && !isPassingVerUp && !isPassingVerDown && isAbility)
        {
            if (speat.joyStick.Horizontal < -0.7f && Vector3.Distance(cenPos, wallFwdFace.point)< 0.5f)
            {
                speat.gameObject.layer = 9; //벽을 통과
                speat.moveHorDir = Vector3.left * 6;
                isPassingHor = true;
            }
            if (speat.joyStick.Horizontal > 0.7f && Vector3.Distance(cenPos, wallFwdFace.point) < 0.5f)
            {
                speat.gameObject.layer = 9; //벽을 통과
                speat.moveHorDir = Vector3.right * 6;
                isPassingHor = true;
            }
            if (speat.joyStick.Vertical < -0.7f)
            {
                speat.gameObject.layer = 10; //벽을 통과
                speat.moveVerDir = Vector3.down * 6;
                isPassingVerDown = true;
            }
            if (speat.joyStick.Vertical > 0.7f)
            {
                speat.gameObject.layer = 10; //벽을 통과
                speat.moveVerDir = Vector3.up * 15;
                isPassingVerUp = true;
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

    private void HideBehindDoor() {

        if (Input.GetMouseButtonDown(0)) {

            // 스핏이 방으로 완전히 들어간 상태일때 클릭
            if (!isHiding && isInRoom) foreach (Interact_ObjectWithRau door in doors) door.GetObjectTouch();

            Vector3 target; // 들어가는 방향
            float distance = 0; // 얼만큼 들어가는지
            int idx = -1;

            foreach (Interact_ObjectWithRau door in doors)
            {
                //print("door.isTouched: " + door.isTouched);
                idx++; // door 인덱스
                //print("idx증가해서 " + idx + "이다!!");
                if (door.isTouched && (!isHiding && !isInRoom))
                {
                    print("들어간다~");
                    speat.transform.rotation = Quaternion.Euler(0,0,0);
                    clickedDoorIndex = idx;
                    print("idx = " + idx + "  /  clickedDoorIndex: " + clickedDoorIndex);
                    distance = 2.0f;
                    target = new Vector3(speat.transform.position.x, speat.transform.position.y, speat.transform.position.z + distance);
                    StartCoroutine(Hiding(target));
                    break;
                }
                else if (door.isTouched && (!isHiding && isInRoom) && idx == clickedDoorIndex)
                {
                    print("나온다~");
                    print("idx = " + idx + "  /  clickedDoorIndex: " + clickedDoorIndex);
                    clickedDoorIndex = -2;
                    speat.transform.rotation = Quaternion.Euler(0, 180, 0);
                    distance = -2.0f;
                    target = new Vector3(speat.transform.position.x, speat.transform.position.y, speat.transform.position.z + distance);
                    StartCoroutine(Hiding(target));
                    break;
                } 
            }
        }

    }


    IEnumerator Hiding(Vector3 target)
    {

        isHiding = true;

        if (isInRoom) isInRoom = false; // 능력써서 방 밖으로 나올 때
        else isInRoom = true; // 능력써서 방안으로 들어올 때

        speat.ctrl.enabled = false;

        while (speat.transform.position != target)
        {
            speat.transform.position = Vector3.MoveTowards(speat.transform.position, target, 0.1f); // 마지막 파라미터는 숨을 때 속도!
            yield return new WaitForSeconds(0f);
        }

        speat.ctrl.enabled = true;
        isHiding = false;

    }

    // 주위에 obj_interaction 태그를 단 오브젝트가 있는지 확인
    void CheckAroundInterActionObj()
    {
        // Physics.SphereCastAll(origin, 구 반지름, 레이방향, maxDistance);
        RaycastHit[] hits = Physics.SphereCastAll(speat.transform.position, 5, Vector3.up, 0f);
        foreach (RaycastHit hit in hits)
        {

            if (hit.collider.gameObject.CompareTag("obj_interaction"))
            {
                //print("주위에 obj_interaction있음! 걔의 parent!! " + hit.collider.gameObject.transform.parent.parent.name);
                CheckClickInterActionObj(hit.collider.gameObject); // obj_interaction 클릭 확인
            }

        }
    }

    // 캐릭터 주위에 obj_interaction 클릭 확인.
    void CheckClickInterActionObj(GameObject obj_interaction) // 캐릭터 주위에 있는(==원 안에 들어오는) obj_interaction을 파라메터로 받음. 얘를 클릭한 얘를 같은지 비교할거임.
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = null;

            if (Camera.main.CompareTag("MainCamera"))
            {
                cam = Camera.main.GetComponent<Camera>();
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(speat.transform.position, Input.mousePosition, Mathf.Infinity);

            foreach (RaycastHit hit in hits){
                //print("!!!!!!!hits: " + hit.collider.name + "parent: " + hit.collider.gameObject.transform.parent.name);
                if (obj_interaction.Equals(hit.collider.gameObject) || hit.collider.gameObject.name == "door") {
                    PlayInteraction(hit.collider.name);
                }

            }


           
        }
    }

    void PlayInteraction(string name)
    {
        print(name + "이 클릭되었으니, 인터랙션 하여라");
    }

    void OnTriggerEnter(Collider other) {

    }
   
}
