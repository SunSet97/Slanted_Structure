using System;
using System.Collections;
using CommonScript;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using static Data.CustomEnum;

public class SpeatAbility : MonoBehaviour
{
    [Header("-UI")]
    public Image abilityImage; // 능력 시간 UI 이미지
    public Text abilityText; // 통과한 벽 UI 텍스트
    public Image buttonImage; // 능력 사용 버튼 UI 이미지

    [Header("-Variable")] 
    public float setDuration = 10; // 능력 지속 시간 설정값
    private float duration = 10; // 능력 지속 시간
    public float setCooltime = 3; // 능력 사용 대기 시간 설정값
    private float cooltime = 0; // 능력 사용 대기 시간
    public int maxWallNum = 5; // 최대 벽 통과 갯수
    private int wallNum = 0; // 벽 통과 횟수
    public bool isAbility = false; // 현재 능력 사용 여부
    public bool isPassingHor = false; // 벽을 통과중인지 여부
    public bool isPassingVerUp = false; // 벽을 통과중인지 여부
    public bool isPassingVerDown = false; // 벽을 통과중인지 여부

    private Vector3 fwdDir, bwdDir; // 전후방 방향 벡터
    private Vector3 upDir, downDir; // 위아래 방향 벡터
    private Vector3 cenPos; // 스핏 중앙 위치
    private Vector3 fwdPos, bwdPos; // 스핏 전후방 위치
    private Vector3 upPos, downPos; // 스핏 위아래 위치
    private RaycastHit wallFwdFace, wallFwdInverse, wallFwdTmp;
    private RaycastHit wallBwdFace, wallBwdInverse, wallBwdTmp;
    private RaycastHit ceilingFace, ceilingInverse, ceilingTmp;
    private RaycastHit floorFace, floorInverse, floorTmp;
    private int fwdCnt = 1;
    private int bwdCnt = 1;
    private int ceilingCnt = 1;
    private int floorCnt = 1; // 탐색 인자
    private float force; // 벽을 통과하는 힘
    private bool isUp = false;
    private bool isDown = false;

    private bool isInRoom;
    [NonSerialized]
    public bool isHiding;
    float touchRange = 0.5f; // 터치(클릭) 허용 범위


    private GameObject hidedDoor;
    private Vector3 originPos;
    private CharacterManager speat;

    void Start()
    {
        ObjectClicker.instance.isCustomUse = true;
        speat = DataController.Instance.GetCharacter(Character.Speat_Adult);
    }

    void Update()
    {
        if (isAbility)
        {
            // 전방 벽 전면 탐지
            int wallLayerMask = 1 << 11;
            if (Physics.Raycast(fwdPos, fwdDir, out wallFwdFace, float.MaxValue, wallLayerMask))
            {
                // 전방 첫번째 벽 전면 임시 저장
                if (wallFwdTmp.transform == null)
                {
                    wallFwdTmp = wallFwdFace;
                }
                // 벽이 바뀌면 변수 초기화 후 재탐색
                else if (wallFwdTmp.transform != wallFwdFace.transform)
                {
                    fwdCnt = 1;
                    wallFwdTmp = wallFwdFace;
                }

                // 전방 벽 후면 탐지
                if (Physics.Raycast(wallFwdFace.point + fwdDir * 5 + bwdDir * fwdCnt, bwdDir, out wallFwdInverse,
                    float.MaxValue, wallLayerMask))
                {
                    if (wallFwdFace.transform != wallFwdInverse.transform)
                    {
                        fwdCnt++; // 같은 벽을 가리킬때까지 탐색
                    }
                }
            }

            // 후방 벽 탐지
            if (Physics.Raycast(bwdPos, bwdDir, out wallBwdFace, float.MaxValue, wallLayerMask))
            {
                // 후방 첫번째 벽 임시 저장
                if (wallBwdTmp.transform == null)
                {
                    wallBwdTmp = wallBwdFace;
                }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (wallBwdTmp.transform != wallBwdFace.transform)
                {
                    bwdCnt = 1;
                    wallBwdTmp = wallBwdFace;
                    if (isPassingHor)
                    {
                        speat.gameObject.layer = LayerMask.NameToLayer("Player");
                        speat.moveHorDir = Vector3.zero;
                        isPassingHor = false;
                        wallNum++;
                    }
                }

                // 후방 벽 후면 탐지
                if (Physics.Raycast(wallBwdFace.point + fwdDir * 5 + bwdDir * bwdCnt, fwdDir, out wallBwdInverse,
                    float.MaxValue, wallLayerMask))
                {
                    if (wallBwdFace.transform != wallBwdInverse.transform)
                    {
                        bwdCnt++; // 같은 벽을 가리킬때까지 탐색
                    }
                }
            }

            // 천장 벽 전면 탐지
            int floorLayerMask = 1 << 12;
            if (Physics.Raycast(upPos, upDir, out ceilingFace, float.MaxValue, floorLayerMask))
            {
                // 천장 첫번째 벽 전면 임시 저장
                if (ceilingTmp.transform == null)
                {
                    ceilingTmp = ceilingFace;
                }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (ceilingTmp.transform != ceilingFace.transform)
                {
                    ceilingCnt = 1;
                    ceilingTmp = ceilingFace;
                    if (isPassingVerDown)
                    {
                        speat.gameObject.layer = LayerMask.NameToLayer("Player");
                        speat.moveVerDir = Vector3.zero;
                        isPassingVerDown = false;
                        wallNum++;
                    }
                }

                // 천장 벽 후면 탐지
                if (Physics.Raycast(ceilingFace.point + upDir * 5 + downDir * ceilingCnt, downDir, out ceilingInverse,
                    float.MaxValue, floorLayerMask))
                {
                    if (ceilingFace.transform != ceilingInverse.transform)
                    {
                        ceilingCnt++; // 같은 벽을 가리킬때까지 탐색
                    }
                }
            }

            // 바닥 벽 전면 탐지
            if (Physics.Raycast(downPos, downDir, out floorFace, float.MaxValue, floorLayerMask))
            {
                // 바닥 첫번째 벽 전면 임시 저장
                if (floorTmp.transform == null)
                {
                    floorTmp = floorFace;
                }
                // 벽이 바뀌면 벽을 통과한 걸로 간주
                else if (floorTmp.transform != floorFace.transform)
                {
                    floorCnt = 1;
                    floorTmp = floorFace;
                    if (isPassingVerUp)
                    {
                        speat.gameObject.layer = LayerMask.NameToLayer("Player");
                        speat.moveVerDir = Vector3.zero;
                        isPassingVerUp = false;
                        wallNum++;
                    }
                }

                // 바닥 벽 후면 탐지
                if (Physics.Raycast(floorFace.point + downDir * 5 + upDir * floorCnt, upDir, out floorInverse,
                    float.MaxValue, floorLayerMask))
                {
                    if (floorFace.transform != floorInverse.transform)
                    {
                        floorCnt++; // 같은 벽을 가리킬때까지 탐색
                    }
                }
            }
        }

        HideBehindDoor(); // 문 뒤로 숨기
        fwdDir = speat.transform.forward;
        bwdDir = -speat.transform.forward; // 전후방 방향 벡터
        upDir = speat.transform.up;
        downDir = -speat.transform.up; // 위아래 방향 벡터

        cenPos = speat.transform.position + speat.transform.up * 0.35f; // 스핏 중앙 위치
        fwdPos = cenPos + fwdDir * 0.3f;
        bwdPos = cenPos + bwdDir * 0.3f; // 스핏 전후방 위치
        upPos = cenPos + upDir * 0.5f;
        downPos = cenPos + downDir * 0.2f; // 스핏 위아래 위치

        ChangeIsAbility(); // 능력 사용 여부 판단
        Dash(); // 조건 만족시 대쉬
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
            speat.gameObject.layer = LayerMask.NameToLayer("Player");
            // 사용 대기 시간 계산
            if (cooltime > 0)
            {
                cooltime -= Time.deltaTime;
            }
            else if (cooltime <= 0)
            {
                cooltime = 0;
            }

            abilityImage.fillAmount = cooltime / setCooltime; // 쿨타임 시간에 맞춰 UI 변화
            buttonImage.fillAmount = 0;

            abilityText.text = null; // 벽 통과 횟수 숨김
        }
        else
        {
            // 능력 사용중일 때

            // 지속 시간 계산
            if (duration > 0)
            {
                duration -= Time.deltaTime;
            }
            else if (duration <= 0)
            {
                isAbility = false;
                duration = 0;
            } // 지속 시간 초과시 능력 자동 종료 및 0 고정

            buttonImage.fillAmount = duration / setDuration; // 지속 시간에 맞춰 UI 변화
            abilityImage.fillAmount = 0;

            // 벽 통과 횟수 변화
            if (wallNum < maxWallNum)
            {
                abilityText.text = wallNum.ToString();
            }
            else
            {
                // 벽 통과 횟수 초과시 능력 자동 종료 및 숨김
                isAbility = false;
                abilityText.text = null;
            }
        }
    }

    // 능력 사용 버튼
    public void UseAbility()
    {
        // 능력 사용중이 아닐때
        if (!isAbility && cooltime <= 0)
        {
            isAbility = true;
            wallNum = 0;
            abilityText.text = "0"; // 벽 통과 횟수 초기화
            duration = setDuration;
            cooltime = setCooltime; // 시간 초기화
            abilityImage.fillAmount = 1; // UI 초기화
            buttonImage.fillAmount = 0; // UI 초기화
        }
        // 능력 사용중일 때
        else if (isAbility)
        {
            isAbility = false;
            wallNum = 0;
            abilityText.text = ""; // 벽 통과 횟수 초기화 및 숨김
            duration = setDuration;
            cooltime = setCooltime; // 시간 초기화
            abilityImage.fillAmount = 0; // UI 초기화
            buttonImage.fillAmount = 1; // UI 초기화
        }
    }

    private void Dash()
    {
        if (!isPassingHor && !isPassingVerUp && !isPassingVerDown && isAbility)
        {
            var joyStick = JoystickController.instance.joystick;
            if (joyStick.Horizontal < -0.7f && Vector3.Distance(cenPos, wallFwdFace.point) < 0.5f)
            {
                speat.gameObject.layer = 9; //벽을 통과
                speat.moveHorDir = Vector3.left * 6;
                isPassingHor = true;
            }

            if (joyStick.Horizontal > 0.7f && Vector3.Distance(cenPos, wallFwdFace.point) < 0.5f)
            {
                speat.gameObject.layer = 9; //벽을 통과
                speat.moveHorDir = Vector3.right * 6;
                isPassingHor = true;
            }

            if (joyStick.Vertical < -0.7f)
            {
                speat.gameObject.layer = 10; //벽을 통과
                speat.moveVerDir = Vector3.down * 6;
                isPassingVerDown = true;
            }

            if (joyStick.Vertical > 0.7f)
            {
                speat.gameObject.layer = 10; //벽을 통과
                speat.moveVerDir = Vector3.up * 15;
                isPassingVerUp = true;
            }
        }
    }


    private void HideBehindDoor()
    {
        if (isHiding)
        {
            return;
        }

        if (ObjectClicker.instance.TouchDisplay(out RaycastHit[] hits))
        {
            foreach (var hit in hits)
            {
                if (!hit.collider.GetComponent<Outline>().enabled)
                {
                    continue;
                }

                var target = hit.collider.transform.position;
                if (!isInRoom)
                {
                    JoystickController.instance.StopSaveLoadJoyStick(true);
                    hidedDoor = hit.collider.gameObject;
                    StartCoroutine(Hide(target, hidedDoor));
                    break;
                }
                else
                {
                    StartCoroutine(Hide(target, hidedDoor));
                    JoystickController.instance.StopSaveLoadJoyStick(false);
                    break;
                }
            }
        }
    }


    private IEnumerator Hide(Vector3 targetPos, GameObject door)
    {
        if (!isInRoom)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0.6f;
            canvasGroup.interactable = false;
        }

        var waitForFixedUpdate = new WaitForFixedUpdate();
        isHiding = true;
        door.GetComponent<OutlineClickObj>().IsClickEnable = false;
        speat.PickUpCharacter();

        Quaternion rotation;
        if (isInRoom)
        {
            rotation = Quaternion.Euler(0, 180, 0);
            targetPos.z = originPos.z;
            Debug.Log("뒤에서 앞으로");
            hidedDoor = null;
        }
        else
        {
            originPos = speat.transform.position;
            rotation = Quaternion.Euler(0, 0, 0);
            targetPos.z += 2f;
            Debug.Log("앞에서 뒤로");
        }

        speat.transform.rotation = rotation;


        speat.anim.SetFloat("Speed", 1);

        var t = 0f;
        var speed = 0.5f;
        var charPos = speat.transform.position;
        while (t <= 1)
        {
            t += Time.fixedDeltaTime * speed;
            // speat.transform.position = Vector3.MoveTowards(speat.transform.position, target, 0.1f); // 마지막 파라미터는 숨을 때 속도!
            speat.transform.position = Vector3.Lerp(charPos, targetPos, t); // 마지막 파라미터는 숨을 때 속도!
            yield return waitForFixedUpdate;
        }

        speat.anim.SetFloat("Speed", 0f);

        if(isInRoom)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
        
        isInRoom = !isInRoom;

        door.GetComponent<OutlineClickObj>().IsClickEnable = true;
        isHiding = false;
        speat.PutDownCharacter();
    }
}