using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public  Joystick joyStick;                                              // 조이스틱
    public CharacterController ctrl;                                       // 캐릭터컨트롤러
    private CanvasControl canvasCtrl;
    public Vector3 moveHorDir = Vector3.zero, moveVerDir = Vector3.zero;    // 수평, 수직 이동 방향 벡터
    private IEnumerator dieAction;
    private bool swipeGrass; // 풀 숲 헤쳐나갈 때 사용

    private Animation anim;

    [Header("캐릭터 정보")]
    public string gender;       // 성별
    public bool isControlled;   // 움직일 수 있는 여부
    public bool isSelected;     // 선택여부
    public bool isExisted;      // 맵 존재여부(데이터 로드시 사용)
    public bool isAbility;      // 능력 사용 여부

    [Header("시점")]
    public Camera cam;

    [Header("디버깅용")]
    public float Speed = 0;                 // 이동 속도
    public float maxMoveSpeed = 2f;         // 최대 이동 속도
    public float moveAcceleration = 14f;    // 이동 가속도
    public float airResistance = 1.2f;      // 공기 저항
    public float frictionalForce = 4f;      // 지상 마찰력
    public bool isJump;                     // 캐릭터의 점프 여부
    public float jumpForce = 5f;            // 점프력
    public float gravityScale = 1.1f;       // 중력 배수
    public bool isDie = false;              // 죽음 여부
    public bool splitTest = false;                  // 코드 분리 임시 변수

    public Quaternion camRotation; // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)
    public float moveSpeed;        // 현재의 수평 이동 속도 계산
    public Vector3 unitVector;     //이동 방향 기준 단위 벡터
    public float normalizing;      //조이스틱 입력 강도 정규화

    public bool button_on=true;//현재 캐릭터매니저 컨트롤러를 껏다 키기 위한 bool값.
    
    void Start()
    {
        //if (isSelected)
        //    this.tag
        ctrl = this.GetComponent<CharacterController>();
        canvasCtrl = CanvasControl.instance_CanvasControl;
        dieAction = DieAction();
        swipeGrass = false;
        anim = gameObject.GetComponent< Animation > ();

        // 세이브데이터를 불러왔을 경우 저장된 위치로 캐릭터 위치를 초기화
        if (DataController.instance_DataController != null)
        {
            // 디버깅용
            DataController.instance_DataController.charData.pencilCnt = 4;
            DataController.instance_DataController.charData.selfEstm = 500;
            DataController.instance_DataController.charData.intimacy_spRau = 200;
            DataController.instance_DataController.charData.intimacy_spOun = 150;
            DataController.instance_DataController.charData.intimacy_ounRau = 150;
            DataController.instance_DataController.charData.story = 1;
            DataController.instance_DataController.charData.storyBranch = 2;
            DataController.instance_DataController.charData.storyBranch_scnd = 3;
            DataController.instance_DataController.charData.dialogue_index = 4;
        }
    }

    void Update()
    {
        // 조이스틱 설정
        if (!joyStick && DataController.instance_DataController.joyStick) joyStick = DataController.instance_DataController.joyStick;
        // 카메라 설정
        if (!cam && DataController.instance_DataController.cam) cam = DataController.instance_DataController.cam;

        // 라우 튜토리얼 풀 숲 지나갈 때
        if (swipeGrass && ctrl.enabled == false && canvasCtrl.finishFadeIn) SwipeGrass();
        //Player_Anim.instance_Animation.Dead =isDie;//isJump값을 Player_Anim스크립트로 보냄
    }

    #region ToDo(Delete)
    private void FixedUpdate()
    {
        if (splitTest)
            return;
        // 조이스틱 설정이 끝난 이후 이동 가능, 캐릭터를 조종할 수 있을 때
        if (joyStick && cam && ctrl.enabled && isControlled) CharacterMovement(DataController.instance_DataController.playMethod);
    }

    // 캐릭터 움직임 코드
    private void CharacterMovement(string playMethod)
    {
        camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);         // 메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)
        moveSpeed = Mathf.Sqrt(moveHorDir.x * moveHorDir.x + moveHorDir.z * moveHorDir.z);  // 현재의 수평 이동 속도 계산
        unitVector = Vector3.zero;                                                          //이동 방향 기준 단위 벡터
        normalizing = 0;                                                                    //조이스틱 입력 강도 정규화

        // 2D 플랫포머
        if (playMethod == "Plt")
        {
            if (joyStick.Horizontal == 0)
                unitVector = moveHorDir.normalized; // 현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * (Vector3.right * joyStick.Horizontal).normalized; // 현재 입력되는 방향이 정방향(x축)

            normalizing = Mathf.Abs(joyStick.Horizontal); // 수평 성분이 입력의 세기
        }
        // 라인트레이서
        else if (playMethod == "Line")
        {
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; // 현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = (Waypoint.FindObjectOfType<Waypoint>().moveTo * joyStick.Horizontal).normalized; // 현재 입력되는 방향이 정방향(waypoint연결선)
            normalizing = Mathf.Abs(joyStick.Horizontal); // 수평 성분이 입력의 세기(선위에서 움직이는 방향이 2가지이므로)
        }
        // 쿼터뷰
        else if (playMethod == "Qrt")
        {
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; // 현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).normalized; // 현재 입력되는 방향이 정방향(xz평면)
            normalizing = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).sqrMagnitude; // 수평 수직의 벡터 크기가 입력의 세기
        }
        /*
         /Speed = Mathf.Sqrt(moveHorDir.x * moveHorDir.x + moveHorDir.z * moveHorDir.z);
        // Gameobject.GetComponent<Player_Anim>().Direction = ;//쿼터뷰에서 방향 바꿀때 나오는 벡터값을 어떻게 보낼지 모르겠음..
        //Player_Anim.instance_Animation.Move_Anim(moveSpeed, moveHorDir.x);//이동속도값을 Player_Anim스크립트로 보냄
        //Player_Anim.instance_Animation.Direction = unitVector;//뱡향벡터값을 Player_Anim스크립트로 보냄
        //좌우 이동시 최대 이동속도를 제한하여 일정 속도를 넘지 않도록함
        */

        if (moveSpeed <= maxMoveSpeed)
        {
            if (!isSelected || isDie) normalizing = 0;                                  // 선택중이지 않거나 죽었을 때 이동 불가
            if (!ctrl.isGrounded) normalizing *= 0.2f;                                  // 공중에서는 약하게 움직임 가능
            moveHorDir += unitVector * normalizing * moveAcceleration * Time.deltaTime; // 조이스틱의 방향과 세기에 따라 해당 이동 방향으로 가속도 작용
        }

        //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
        if (isSelected && joyStick.Vertical > 0.5f && ctrl.isGrounded && playMethod != "Qrt")
            isJump = true;  //점프 가능 상태로 변경

        //캐릭터 선택중일때 점프 가능
        if ((isSelected && !isDie) && isJump)
        {
            moveVerDir.y += jumpForce; //점프력 만큼 힘을 가함

            isJump = false; //점프 불가능 상태로 변경하여 연속적인 점프 제한
        }

        //isGrounded에 따라 저항력 결정
        float resistanceValue = 0; //저항력
        if (ctrl.isGrounded)
            resistanceValue = frictionalForce; //캐릭터가 땅에 붙어있을때 마찰력 적용
        else
            resistanceValue = airResistance; //캐릭터가 땅에 벗어나있을때 공기저항 적용

        //이동중 일때 마찰력 적용
        if (moveSpeed >= 0.05f)
        {
            if ((moveHorDir.normalized - unitVector).sqrMagnitude < 0.8f)
                moveHorDir -= unitVector * resistanceValue * Time.deltaTime; //이동 방향과 기준 방향 차의 크기가 1보다 작으면 같은 방향(정방향)으로 이동 중, 따라서 저항력의 방향은 기준 방향의 역방향으로 작용
            else
                moveHorDir += unitVector * resistanceValue * Time.deltaTime; //이동 방향과 기준 방향 차의 크기가 1보다 크면 다른 방향(역방향)으로 이동 중, 따라서 저항력의 방향은 기준 방향의 정방향으로 작용
        }

        //땅에서 떨어져 있을 경우 기본적으로 중력이 적용되고 중력은 가속도이므로 +=를 써서 계속해서 더해줌
        if (!ctrl.isGrounded || isDie)
            moveVerDir.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        //if (DataController.instance_DataController.isMapChanged == false)
            ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 최종 이동 시킴
    }
    #endregion
    
    private void OnTriggerEnter(Collider other)
    {
        if (splitTest)
            return;
        
        // 게임진행에 관련된 콜라이더일 경우
        if (other.gameObject.transform.parent.name == "ProgressCollider")
        {
            canvasCtrl.isGoNextStep = true;

            if (other.gameObject.CompareTag("ScriptCollider")) // 캐릭터 독백
            {
                // 맵이름 정해지면 맵이름 따라 갈 예정
                if (DataController.instance_DataController.currentChar.name == "Rau")
                    DataController.instance_DataController.LoadData(other.gameObject.transform.parent.name + "/RauTutorial/", other.gameObject.name + ".json");
                else
                    DataController.instance_DataController.LoadData(other.gameObject.transform.parent.name + "/SpeatTutorial/", other.gameObject.name + ".json");
                canvasCtrl.progressIndex++;
                canvasCtrl.isPossibleCnvs = false;
                canvasCtrl.StartConversation();
                //canvasCtrl.GoNextStep(true); // 다음 콜라이더를 부를 수 있도록

            }
            else if (other.gameObject.CompareTag("CommandCollider"))// 지시문
            {
                canvasCtrl.progressIndex++;
                canvasCtrl.GoNextStep(); // 일단 다음 콜라이더를 불러줌
                canvasCtrl.TutorialCmdCtrl();
            }
            else if (other.gameObject.CompareTag("Complete"))// 미션 완료 콜라이더
            {
                canvasCtrl.progressIndex++;
                canvasCtrl.GoNextStep();
            }
            else if (other.gameObject.CompareTag("ChangePlayMethod")) //플레이 모드 바꿈
            {
                canvasCtrl.progressIndex++;
                canvasCtrl.GoNextStep();

                DataController.instance_DataController.currentMap.playMethod = other.gameObject.name;

                // 플레이 모드 변경에 맞춘 시점 변경 (바뀔 가능성 있음)
                CameraTransform camInfo;
                camInfo = other.gameObject.GetComponent<CameraTransform>();
                if (camInfo.isChange)
                {
                    // 플랫폼 모드로 바꿀 때 캐릭터의 z축 지정
                    if (other.gameObject.name == "Plt")
                    {
                        ctrl.enabled = false;
                        DataController.instance_DataController.currentChar.transform.position = new Vector3(transform.position.x, transform.position.y, camInfo.charZ);
                        ctrl.enabled = true;
                    }

                    DataController.instance_DataController.camDis_x = camInfo.dis_X;
                    DataController.instance_DataController.camDis_z = camInfo.dis_Z;
                    DataController.instance_DataController.rot = camInfo.rotation;
                }
            }
            else if (other.gameObject.CompareTag("ChangeAngle")) // 카메라 앵글 바꿈
            {
                canvasCtrl.progressIndex++;
                canvasCtrl.GoNextStep();

                // 바뀔 카메라 앵글을 담은 스크립트 받아오기
                CameraTransform camInfo;
                camInfo = other.gameObject.GetComponent<CameraTransform>();

                DataController.instance_DataController.camDis_x = camInfo.dis_X;
                DataController.instance_DataController.camDis_z = camInfo.dis_Z;
                DataController.instance_DataController.rot = camInfo.rotation;
            }
            else if (other.gameObject.CompareTag("ChangeInput")) // 사용하는 입력 설정을 바꿀 때
            {
                canvasCtrl.progressIndex++;

                if (canvasCtrl.progressIndex < DataController.instance_DataController.progressColliders.Length)
                {
                    DataController.instance_DataController.progressColliders[canvasCtrl.progressIndex].gameObject.SetActive(true);
                }

                canvasCtrl.isGoNextStep = false;

                if (other.gameObject.name == "SwipeGrass")
                {
                    ctrl.enabled = false;
                    swipeGrass = true;
                }
            }
            else if (other.gameObject.CompareTag("ActiveInteraction"))
            {
                canvasCtrl.progressIndex++;

                InteractionList list = other.gameObject.GetComponent<InteractionList>();
                int listLen = list.activeList.Length;

                if (other.gameObject.name == "ActiveInteraction")
                {
                    canvasCtrl.isGoNextStep = false;
                    for (int i= 0; i < listLen; i++)
                    {
                        list.activeList[i].GetComponent<InteractObjectControl>().isInteractable = true;
                    }
                }
                else if (other.gameObject.name == "DisableInteraction")// 인터랙션 비활성화
                {
                    for (int i = 0; i < listLen; i++)
                    {
                        list.activeList[i].GetComponent<InteractObjectControl>().isInteractable = false;
                        canvasCtrl.GoNextStep();
                    }
                }
                else if (other.gameObject.name == "MiniGame")
                {
                    list.activeList[0].SetActive(true);
                    list.activeList[0].GetComponent<InteractObjectControl>().playMiniGame = true;
                }

            }
            other.gameObject.SetActive(false);
        }
        else
        {
            // 리스폰 포지션을 데이터에 저장
            if (other.gameObject.CompareTag("RespawnPoint"))
            {

                DataController.instance_DataController.charData.respawnLocation = other.gameObject.transform.position;

            }
            else if (other.gameObject.CompareTag("Item")) // 획득한 아이템을 데이터에 저장
            {
                Destroy(other.gameObject);
                DataController.instance_DataController.charData.item.Add(other.gameObject.name);
            }
            else if (other.gameObject.CompareTag("Monster")) // 몬스터와 닿으면 사망
            {

                isDie = true;

                //CharDie();
                StartCoroutine(DieAction());
                //Destroy(other.gameObject);
            }
        }

    }

    float swipeDis = Screen.width / 3;
    float moveDIs = 3.5f;
    Vector3 startPos, endPos, swipePos;
    int swipeCnt = 0;
    bool leftTurn = true;
    bool curTurn = false;

    // 라우 튜토리얼 풀 숲 헤쳐나갈 때 사용
    void SwipeGrass()
    {
        if (curTurn != leftTurn && Input.GetMouseButtonDown(0))
        {
            curTurn = leftTurn;
            startPos = Input.mousePosition;

            if (swipeCnt % 2 == 0)
            {
                Color color = canvasCtrl.sprRenderers[1].color;
                canvasCtrl.sprRenderers[1].color = new Color(color.r, color.g, color.b, 0.3f);
            }
            else
            {
                Color color = canvasCtrl.sprRenderers[0].color;
                canvasCtrl.sprRenderers[0].color = new Color(color.r, color.g, color.b, 0.3f);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            endPos = Input.mousePosition;
            swipePos = endPos - startPos;

            if ((leftTurn == true && swipePos.x < 0) || (leftTurn == false && swipePos.x > 0))
            {
                if (Mathf.Abs(swipePos.x) >= swipeDis)
                {
                    swipeCnt++;

                    //Vector3 newPos = new Vector3(transform.position.x + moveDIs, transform.position.y, transform.position.z);
                    transform.position = new Vector3(transform.position.x + moveDIs, transform.position.y, transform.position.z); /* Vector3.MoveTowards(transform.position, newPos, maxMoveSpeed * Time.deltaTime);*/

                    Color color = canvasCtrl.sprRenderers[0].color;

                    canvasCtrl.sprRenderers[swipeCnt % 2].color = new Color(color.r, color.g, color.b, 1.0f);

                    leftTurn = curTurn ? false : true;

                    if (swipeCnt == 10)
                    {
                        swipeGrass = false;
                        ctrl.enabled = true;
                    }
                }

            }

        }
    }

    //// 플레이어가 죽으면 불리는 함수
    //void CharDie()
    //{
    //    StartCoroutine(dieAction);
    //}

    public void Change_Position(bool button_on)
    {
        if (!button_on)
        {
            ctrl.enabled = false;
        }
        if (button_on)
        {
            ctrl.enabled = true;
        }

    }

    // 디버깅용. 플레이어가 죽은 후 리스폰 장소에서 부활하기까지의 행동 (플레이어의 투명도 조절 후 이동)
    IEnumerator DieAction()
    {
        Material mat = this.GetComponentInChildren<SkinnedMeshRenderer>().material;
        Color color = mat.color;

        // 플레이어가 죽었음을 보여줌 (반투명)
        mat.color = new Color(color.r, color.g, color.b, 0.5f);
        yield return new WaitForSeconds(2);

        // 캐릭터컨트롤러를 끄고 플레이어를 리스폰 위치로 이동시킴
        ctrl.enabled = false;
        transform.position = DataController.instance_DataController.charData.respawnLocation;

        // 투명도를 원상태로 복귀
        mat.color = new Color(color.r, color.g, color.b, 1f);


        // 플레이어가 부활한 후 다시 캐릭터컨트롤러 활성화
        ctrl.enabled = true;
        isDie = false;
        StopCoroutine(DieAction());
    }

    public void CanWallPass(int layerNum)
    {
        this.gameObject.layer = layerNum;
    }
    public void CannotWallPass(int layerNum)
    {
        this.gameObject.layer = 0;
    }
}
