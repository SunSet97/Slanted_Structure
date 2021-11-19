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

public class CatchRobberManager : MonoBehaviour, IMovable
{
    private short dir;

    public bool IsMove { get; set; } = true;
    // 라우
    [Header("라우")]
    public CharacterManager rau;
    public float rauSpeed;
    [SerializeField] private float acceleration;
    bool isStopping = false;
    Vector3 forwardDir;
    [SerializeField] private Transform[] threeWay;
    private bool isMoveSide;
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
    private void Start()
    {
        Invoke("InitialSetting", 0.5f);
    }

    void Update()
    {
        if (rau != null)
        {
            if (IsMove)
            {
                RauMove();

                DoveMove(); ;

                // 소매치기 이동
                characterController_robber.Move(new Vector3(0, 0, robberSpeed * Time.deltaTime));
            }
            CheckCompletion();
            timer -= Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {

            // 비둘기랑 만났을 때
            if (other.name.Equals("doves_1") || other.name.Equals("doves_2"))
            {
                StartCoroutine(Fly(5f));
            }
            else // 비둘기 말고 이외의 장애물과 만났을 때
            {
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
        rau = DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main);
        rau.IsMove = false;
        rau.anim.applyRootMotion = false;
        rau.anim.SetFloat("Speed", 0.7f);
        // 조이스틱 입력없을 때 라우가 바라보는 방향 설정
        forwardDir = rau.transform.position - DataController.instance_DataController.cam.transform.position;
        forwardDir.y = 0;
        forwardDir.x = 0;
        // 라우 위치시키기
        robber.transform.position = rau.transform.position + new Vector3(0, 0, 10);
        if (robber) characterController_robber = robber.GetComponent<CharacterController>();
        // 비둘기 애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 1
        List<Animator> tmpAnimsList = new List<Animator>();
        List<CameraFollow> tmpCamFollowList = new List<CameraFollow>();
        for (int i = 0; i < doves_1.transform.childCount; i++)
        {
            tmpAnimsList.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<Animator>());
            tmpCamFollowList.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //tmpCamFollowList1[i].SmoothSpeed = a;
        }
        dovesComponents.Add(new dovesComoponents(tmpAnimsList, tmpCamFollowList));
        // 비둘기애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 2
        tmpAnimsList.Clear();
        tmpCamFollowList.Clear();
        for (int i = 0; i < doves_2.transform.childCount; i++)
        {
            tmpAnimsList.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<Animator>());
            tmpCamFollowList.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //tmpCamFollowList2[i].SmoothSpeed = a;
        }
        dovesComponents.Add(new dovesComoponents(tmpAnimsList, tmpCamFollowList));

    }
    #endregion

    #region 라우 이동 관련
    void RauMove() {
        Vector3 rot = default;
        // 조이스틱 input없을때 forwadDir방향 바라보게하기
        if (DataController.instance_DataController.inputDirection == Vector2.zero) // 조이스틱 인풋 없을 때 forwadDir방향 바라보게하기
        {
            Quaternion targetRot = Quaternion.LookRotation(forwardDir);
            rau.transform.rotation = Quaternion.Lerp(rau.transform.rotation, targetRot, Time.deltaTime * 2);
        }
        else
        {
            rot = new Vector3(DataController.instance_DataController.inputDirection.x, 0, DataController.instance_DataController.inputDirection.y);
            if(rot.z < 0)
            {
                rot.z = 0;
            }
            if (Vector3.SignedAngle(Vector3.forward, rot, Vector3.up) > 30)
            {
                if (transform.position.x > threeWay[2].position.x)
                {
                    dir = 0;
                }
                else
                {
                    dir = 1;
                }
                rot.Set(Mathf.Cos(60 * Mathf.Deg2Rad) * DataController.instance_DataController.inputDegree, 0, Mathf.Sin(60 * Mathf.Deg2Rad) * DataController.instance_DataController.inputDegree);
            }
            else if (Vector3.SignedAngle(Vector3.forward, rot, Vector3.up) < -30)
            {
                if (transform.position.x < threeWay[0].position.x)
                {
                    dir = 0;
                }
                else
                {
                    dir = -1;
                }
                rot.Set(Mathf.Cos(120 * Mathf.Deg2Rad) * DataController.instance_DataController.inputDegree, 0, Mathf.Sin(120 * Mathf.Deg2Rad) * DataController.instance_DataController.inputDegree);
            }
            rau.transform.rotation = Quaternion.Lerp(rau.transform.rotation, Quaternion.LookRotation(rot), Time.deltaTime * 3);
        }
        // 몇프레임동안 이동하도록
        //
        //if (isMoveSide)
        //{
        //    DataController.instance_DataController.InitializeJoystic();
        //    DataController.instance_DataController.joyStick.gameObject.SetActive(false);
        //    if (Mathf.Abs(rau.transform.position.x - threeWay[0].position.x) < 1)
        //    {
        //        isMoveSide = false;
        //        dir = 0;
        //    }
        //    else if (Mathf.Abs(rau.transform.position.x - threeWay[1].position.x) < 1)
        //    {
        //        isMoveSide = false;
        //        dir = 0;
        //    }else if(Mathf.Abs(rau.transform.position.x - threeWay[2].position.x) < 1)
        //    {
        //        isMoveSide = false;
        //        dir = 0;
        //    }
        //}
        // 라우 이동
        if (!isStopping)
        {
            isStopping = true;
            rau.ctrl.Move(rau.transform.position-threeWay[0].position);
            //rau.ctrl.Move(new Vector3(dir * 10 * Time.deltaTime, 0, Time.deltaTime) * rauSpeed);
            //character.anim.SetFloat("Speed", rauSpeed * Time.deltaTime);
            rauSpeed += acceleration * Time.deltaTime;
        }
        // 캐릭터매니저랑 라우랑 위치 일치시키기 => 충돌 감지때문
        gameObject.transform.position = rau.transform.position;
        threeWay[0].parent.position.Set(threeWay[0].parent.position.x, threeWay[0].parent.position.y, rau.transform.position.z);
    }
    #endregion

    #region 장애물 위치시키기
    void PositioningObstacle() {

        CollisionObstacles.transform.GetChild(obstacleIndex).gameObject.transform.position += new Vector3(0, 0, 40); // 40만큼 이동

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
                doves_1.transform.position = Vector3.MoveTowards(doves_1.transform.position, rau.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
            }
            else if(obstacleIndex == 1)
            {
                //Vector3 DesiredPosition = Camera.main.transform.position + cameraOffset_dove;
                //Vector3 SmoothPosition = Vector3.Lerp(doves_2.transform.position, DesiredPosition, doveSpeed * Time.deltaTime);
                //doves_2.transform.position = SmoothPosition;
                doves_2.transform.position = Vector3.MoveTowards(doves_2.transform.position, rau.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
            }



            
        }

    }
    #endregion

    void SetKickBoardPosition()
    {

    }

    #region 판정
    void CheckCompletion() {

        float dis = Vector3.Distance(rau.transform.position, robber.transform.position);

        if (Mathf.Round(timer) == 0)
        {
            if (dis <= clearDis)
            {
                Debug.Log("성공.");
                DataController.instance_DataController.currentMap.positionSets[0].clearBox.GetComponent<CheckMapClear>().Clear();
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
                DataController.instance_DataController.currentMap.positionSets[0].clearBox.GetComponent<CheckMapClear>().Clear();
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
