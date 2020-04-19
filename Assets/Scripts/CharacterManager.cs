using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private Joystick joyStick; //조이스틱
    private CharacterController ctrl; //캐릭터컨트롤러
    public Vector3 moveHorDir = Vector3.zero, moveVerDir = Vector3.zero; //수평,수직 이동 방향 벡터
    private IEnumerator dieAction;

    [Header("캐릭터 정보")]
    public string name; //이름
    public string gender; //성별
    public bool isSelected; //선택여부
    public bool isExisted; //Scene 존재여부(데이터 로드시 사용)

    [Header("시점")]
    public Camera cam;

    [Header("디버깅용")]
    public float maxMoveSpeed = 4f; //최대 이동속도
    public float moveAcceleration = 8f; //이동가속도
    public float airResistance = 3f; //공기저항
    public float frictionalForce = 5f; //지상 마찰력
    public bool isJump; // 캐릭터가 점프 여부
    public float jumpForce = 6f; //점프력
    public float gravityScale = 1f; //중력 배수
    public bool isDie = false;

    void Start()
    {
        //if (isSelected)
        //    this.tag
        ctrl = this.GetComponent<CharacterController>();
        dieAction = DieAction();

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

            ctrl.enabled = false;
            transform.position = DataController.instance_DataController.charData.endPosition;
            ctrl.enabled = true;
        }
    }

    void Update()
    {
        //조이스틱 설정
        if (!joyStick) joyStick = SceneInformation.instance_SceneInformation.joyStick;
        //ScreenInformation.instance_SceneInformation.playMethod="Cut";
    }

    private void FixedUpdate()
    {
        //조이스틱 설정이 끝난 이후 이동 가능
        if (joyStick) CharacterMovement(SceneInformation.instance_SceneInformation.playMethod);
    }

    private void CharacterMovement(string playMethod)
    {
        Quaternion camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0); //메인 카메라 기준으로 joystick input 변경(라인트레이서 제외)
        float moveSpeed = Mathf.Sqrt(moveHorDir.x * moveHorDir.x + moveHorDir.z * moveHorDir.z); //현재의 수평 이동 속도 계산
        Vector3 unitVector = Vector3.zero; //이동 방향 기준 단위 벡터
        float normalizing = 0; //조이스틱 입력 강도 정규화

        //2D 플랫포머
        if (playMethod == "Plt")
        {
            if (joyStick.Horizontal == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * (Vector3.right * joyStick.Horizontal).normalized; //현재 입력되는 방향이 정방향(x축)
            normalizing = Mathf.Abs(joyStick.Horizontal); //수평 성분이 입력의 세기
        }
        //라인트레이서
        else if (playMethod == "Line")
        {
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = (Waypoint.FindObjectOfType<Waypoint>().moveTo * joyStick.Horizontal).normalized; //현재 입력되는 방향이 정방향(waypoint연결선)
            normalizing = Mathf.Abs(joyStick.Horizontal); //수평 성분이 입력의 세기(선위에서 움직이는 방향이 2가지이므로)
        }
        //쿼터뷰
        else if (playMethod == "Qrt")
        {
            if (joyStick.Horizontal == 0 && joyStick.Vertical == 0)
                unitVector = moveHorDir.normalized; //현재 움직이는 방향이 마지막으로 입력된 정방향
            else
                unitVector = camRotation * new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).normalized; //현재 입력되는 방향이 정방향(xz평면)
            normalizing = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical).sqrMagnitude; //수평 수직의 벡터 크기가 입력의 세기
        }

        //좌우 이동시 최대 이동속도를 제한하여 일정 속도를 넘지 않도록함
        if (moveSpeed <= maxMoveSpeed)
        {
            if (!isSelected || isDie) normalizing = 0; //선택중이지 않으면 이동 불가
            if (!ctrl.isGrounded) normalizing *= 0.2f; //공중에서는 약하게 움직임 가능
            moveHorDir += unitVector * normalizing * moveAcceleration * Time.deltaTime; //조이스틱의 방향과 세기에 따라 해당 이동 방향으로 가속도 작용
        }

        //점프는 바닥에 닿아 있을 때 위로 스와이프 했을 경우에 가능(쿼터뷰일때 불가능)
        if (joyStick.Vertical > 0.5f && ctrl.isGrounded && playMethod != "Qrt")
            isJump = true;  //점프 가능 상태로 변경

        //캐릭터 선택중일때 점프 가능
        if ((isSelected && !isDie) && isJump)
        {
            moveVerDir.y = jumpForce; //점프력 만큼 힘을 가함
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

            ctrl.Move((moveHorDir + moveVerDir) * Time.deltaTime); //캐릭터를 최종 이동 시킴
    }

    private void OnTriggerEnter(Collider other)
    {
        // 리스폰 포지션을 데이터에 저장
        if (other.gameObject.CompareTag("RespawnPoint"))
        {

            DataController.instance_DataController.charData.respawnLocation = other.gameObject;

        }

        // 획득한 아이템을 데이터에 저장
        if (other.gameObject.CompareTag("Item"))
        {
            Destroy(other.gameObject);
            DataController.instance_DataController.charData.item.Add(other.gameObject.name);
        }

        // 몬스터와 닿으면 사망
        if (other.gameObject.CompareTag("Monster"))
        {

            isDie = true;

            //CharDie();
            StartCoroutine(DieAction());
            //Destroy(other.gameObject);
        }
    }

    //// 플레이어가 죽으면 불리는 함수
    //void CharDie()
    //{
    //    StartCoroutine(dieAction);
    //}

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
        transform.position = DataController.instance_DataController.charData.respawnLocation.transform.position;

        // 투명도를 원상태로 복귀
        mat.color = new Color(color.r, color.g, color.b, 1f);


        // 플레이어가 부활한 후 다시 캐릭터컨트롤러 활성화
        ctrl.enabled = true;
        isDie = false;
        StopCoroutine(DieAction());
    }
}
