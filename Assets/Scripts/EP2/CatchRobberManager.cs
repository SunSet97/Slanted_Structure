using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class dovesComoponents
{
    public List<Animator> anims;
    public List<CameraFollow> camFollow;
    
    public dovesComoponents(List<Animator> anims, List<CameraFollow> camFollow)
    {
        this.anims = anims;
        this.camFollow = camFollow;
    }

}

public class CatchRobberManager : MonoBehaviour
{
    // 라우
    [Header("라우")]
    CharacterManager character;
    public float rauSpeed;
    public float acceleration_setValue;
    float acceleration;
    bool isStopping = false;
    Vector3 forwardDir;

    // 소매치기
    [Header("소매치기")]
    public GameObject robber;
    public float robberSpeed;
    CharacterController characterController_robber;

    [Header("장애물들")]
    public GameObject kickBoard;
    bool isKickBoard;
    public GameObject CollisionObstacles; // 장애물 이동을 위한.. - 킥보드 제외
    public GameObject doves_1;
    public GameObject doves_2;
    public Transform dovesPosition_1;
    public Transform dovesPosition_2;
    public int obstacleIndex = 0;
    public List<dovesComoponents> dovesComponents = new List<dovesComoponents>();
    bool isFlying_doves = false; // 비둘기 날고 있는지

    [Header("비둘기 속성")]
    public float doveSpeed;

    [Header("파티클시스템")]
    public ParticleSystem particle;

    [Header("타이머 설정")]
    public float timer;

    [Header("라우~소매치기 성공 거리")]
    public float clearDis;


    // Start is called before the first frame update
    void Start()
    {
        InitialSetting();
    }

    // Update is called once per frame
    void Update()
    {
        if (!character)
        {
            InitialSetting();
        }
        else
        {

            RauMove();

            DoveMove(); ;

            // 소매치기 이동
            characterController_robber.Move(new Vector3(0, 0, robberSpeed * Time.deltaTime));

            timer -= Time.deltaTime;

        }


    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {

            // 비둘기랑 만났을 때
            if (other.name == "doves_1" || other.name == "doves_2")
            {
                //print("둘기들이랑 충돌!!");
                StartCoroutine(Fly(5f));
            }
            else // 비둘기 말고 이외의 장애물과 만났을 때
            {
                //isStopping = true;
                StartCoroutine(Stop(3.0f)); // Obstacle이랑 부딪히면, 3초간 멈춤
            }

        }

        // 얘랑 부딪히면 오브젝트 묶음 이동(재활용때문)
        if (other.name == "SetObstacleCollider")
        {
            PositioningObstacle();
            other.transform.position = other.transform.position + new Vector3(0, 0, 20); // 콜라이더도 같이 이동
        }

    }


    #region 초기 세팅
    void InitialSetting()
    {
        // 현재 캐릭터
        character = DataController.instance_DataController.currentChar;
        // 맵 이동
        DataController.instance_DataController.isMapChanged = true;
        // 가속도 설정
        acceleration = acceleration_setValue;
        // 조이스틱 입력없을 때 라우가 바라보는 방향 설정
        forwardDir = character.transform.position - DataController.instance_DataController.cam.transform.position;
        forwardDir.y = 0;
        forwardDir.x = 0;
        // 라우 위치시키기
        robber.transform.position = character.transform.position + new Vector3(0, 0, 10);
        if (robber) characterController_robber = robber.GetComponent<CharacterController>();
        // 비둘기 애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 1
        List<Animator> tmpAnimsList1 = new List<Animator>();
        List<CameraFollow> tmpCamFollowList1 = new List<CameraFollow>();
        dovesComoponents tmp1;
        for (int i = 0; i < doves_1.transform.childCount; i++)
        {
            tmpAnimsList1.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<Animator>());
            tmpCamFollowList1.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //tmpCamFollowList1[i].SmoothSpeed = a;
        }
        tmp1 = new dovesComoponents(tmpAnimsList1, tmpCamFollowList1);
        dovesComponents.Add(tmp1);
        // 비둘기애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 2
        List<Animator> tmpAnimsList2 = new List<Animator>();
        List<CameraFollow> tmpCamFollowList2 = new List<CameraFollow>();
        dovesComoponents tmp2;
        for (int i = 0; i < doves_2.transform.childCount; i++)
        {
            tmpAnimsList2.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<Animator>());
            tmpCamFollowList2.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //tmpCamFollowList2[i].SmoothSpeed = a;
        }
        tmp2 = new dovesComoponents(tmpAnimsList2, tmpCamFollowList2);
        dovesComponents.Add(tmp2);

    }
    #endregion

    #region 라우 이동 관련
    void RauMove() {

        // 라우 이동
        if (!isStopping)
        {
            character.ctrl.Move(new Vector3(0, 0, rauSpeed * Time.deltaTime));
            rauSpeed += acceleration;
        }

        // 조이스틱 input없을때 forwadDir방향 바라보게하기
        if (DataController.instance_DataController.inputDirection == new Vector2(0, 0)) // 조이스틱 인풋 없을 때 forwadDir방향 바라보게하기
        {
            Quaternion targetRot = Quaternion.LookRotation(forwardDir);
            character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRot, Time.deltaTime * 2);
        }

        // 캐릭터매니저랑 라우랑 위치 일치시키기 => 충돌 감지때문
        gameObject.transform.position = character.transform.position;
    }
    #endregion

    #region 장애물 위치시키기
    void PositioningObstacle() {

        CollisionObstacles.transform.GetChild(obstacleIndex).gameObject.transform.position = CollisionObstacles.transform.GetChild(obstacleIndex).gameObject.transform.position + new Vector3(0,0,40); // 40만큼 이동

        // 다음에 위치될 obstacle 그룹 설정!
        if (obstacleIndex == 0) obstacleIndex = 1;
        else obstacleIndex = 0;
    }
    #endregion

    #region 비둘기 이동
    void DoveMove() {

        if (isFlying_doves)
        {

            if (obstacleIndex == 0)
            {
                //Vector3 DesiredPosition = Camera.main.transform.position + cameraOffset_dove;
                //Vector3 SmoothPosition = Vector3.Lerp(doves_1.transform.position, DesiredPosition, doveSpeed * Time.deltaTime);
                //doves_1.transform.position = SmoothPosition;
                doves_1.transform.position = Vector3.MoveTowards(doves_1.transform.position, character.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
            }
            else if(obstacleIndex == 1)
            {
                //Vector3 DesiredPosition = Camera.main.transform.position + cameraOffset_dove;
                //Vector3 SmoothPosition = Vector3.Lerp(doves_2.transform.position, DesiredPosition, doveSpeed * Time.deltaTime);
                //doves_2.transform.position = SmoothPosition;
                doves_2.transform.position = Vector3.MoveTowards(doves_2.transform.position, character.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
            }



            
        }

    }
    #endregion

    void SetKickBoardPosition()
    {

    }

    #region 판정
    void CheckCompletion() {

        float dis = Vector3.Distance(character.transform.position, robber.transform.position);

        if (Mathf.Round(timer) == 0)
        {
            if (dis <= clearDis)
            {
                Debug.Log("성공.");
            }
            else
            {
                Debug.Log("실패.");
            }

        }
        else
        {
            if (dis <= clearDis)
            {
                Debug.Log("성공");
            }

        }

    }
    #endregion

    #region Stop코루틴
    IEnumerator Stop(float time) {

        DataController.instance_DataController.joyStick.input = new Vector2(0,0);

        DataController.instance_DataController.joyStick.gameObject.SetActive(false); // 조이스틱 없애기
        isStopping = true;

        yield return new WaitForSeconds(time);

        isStopping = false;
        DataController.instance_DataController.joyStick.gameObject.SetActive(true); // 조이스틱 없애기
    }
    #endregion

    #region Fly코루틴
    // 카메라 방향으로 날라가는거
    IEnumerator Fly(float time) {
        print("Fly 코루틴~");
        isFlying_doves = true;

        // 날기
        for (int i = 0; i < dovesComponents[obstacleIndex].anims.Count; i++)
        {
            dovesComponents[obstacleIndex].anims[i].SetBool("takeoff", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("idle", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("falling", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("landing", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("fly", false);
            //dovesComponents[obstacleIndex].camFollow[i].target = DataController.instance_DataController.cam.transform;
        }
        yield return new WaitForSeconds(0); // -> 이거 안하면 애니메이션 플레이 안됨.. 이유는 모름..ㅠ
        for (int i = 0; i < dovesComponents[obstacleIndex].anims.Count; i++)
        {
            dovesComponents[obstacleIndex].anims[i].SetBool("takeoff", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("idle", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("falling", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("landing", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("fly", false);
            //dovesComponents[obstacleIndex].camFollow[i].target = null;
        }

        yield return new WaitForSeconds(time);

        // 앉기
        for (int i = 0; i < dovesComponents[obstacleIndex].anims.Count; i++)
        {
            dovesComponents[obstacleIndex].anims[i].SetBool("takeoff", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("idle", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("falling", false);
            dovesComponents[obstacleIndex].anims[i].SetBool("landing", true);
            dovesComponents[obstacleIndex].anims[i].SetBool("fly", false);
            //dovesComponents[obstacleIndex].camFollow[i].target = DataController.instance_DataController.cam.transform;
        }

        // 원위치시키기
        if (obstacleIndex == 0) doves_1.transform.position = dovesPosition_1.position;
        else if (obstacleIndex == 1) doves_2.transform.position = dovesPosition_2.position;

        isFlying_doves = false;

    }
    #endregion

    IEnumerator MoveKickboard(float time)
    {
        yield return new WaitForSeconds(time);

    }

}
