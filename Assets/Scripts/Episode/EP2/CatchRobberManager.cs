using System.Collections;
using System.Collections.Generic;
using Play;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using static Data.CustomEnum;

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

public class CatchRobberManager : MonoBehaviour, IGamePlayable
{
    public bool IsPlay { get; set; }

    private readonly int SpeedHash = Animator.StringToHash("Speed");

    // 라우
    private CharacterManager rau;
    [Header("라우")]
    public float rauSpeed;
    [SerializeField] private float acceleration;
    bool isStopping = false;
    private Transform[] moveTransforms;
    public Transform moveLeftAndRight;
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

    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Start()
    {
        this.StartCoroutine(InitialSetting());
    }

    public void Play()
    {
        IsPlay = true;
    }

    public void EndPlay()
    {
        IsPlay = false;
    }

    public enum Dir { 
        Middle, Left, Right
    }
    
    //public Dir GoDir;
    public Dir CurrentDir;
    public bool isDragged;
    public bool isMoving;
    float facedir = 25;
    void Update()
    {
        if (IsPlay)
        {
            if (!isMoving && !isStopping)
            {
                if (JoystickController.instance.joystick.Horizontal < -0.5 && !isDragged && CurrentDir != Dir.Left)
                {
                    // 왼 드래그 공통
                    isDragged = true;
                    

                    //이동 실행
                    if (CurrentDir == Dir.Middle)
                    {
                        CurrentDir = Dir.Left;
                        rau.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(moveTransforms[0], moveTransforms[1]));
                    }
                    else if (CurrentDir == Dir.Right)
                    {
                        CurrentDir = Dir.Middle;
                        rau.transform.rotation = Quaternion.Euler(facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(moveTransforms[2], moveTransforms[0]));
                    }
                }
                else if (JoystickController.instance.joystick.Horizontal > 0.5 && !isDragged && CurrentDir != Dir.Right)
                {
                    //GoDir = Dir.Right;
                    isDragged = true;
                    if (CurrentDir == Dir.Middle)
                    {
                        CurrentDir = Dir.Right;
                        rau.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(moveTransforms[0], moveTransforms[2]));
                    }
                    else if (CurrentDir == Dir.Left)
                    {
                        CurrentDir = Dir.Middle;
                        rau.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(moveTransforms[1], moveTransforms[0]));
                    }
                }
                else if (JoystickController.instance.joystick.Horizontal >= -0.5 && JoystickController.instance.joystick.Horizontal <= 0.5)
                {
                    isDragged = false;
                    //Debug.Log("none");
                }
            }
                RauMove();

                DoveMove(); 
            
                // 소매치기 이동
                characterController_robber.Move(new Vector3(0, -1, robberSpeed * Time.deltaTime));
            
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
    IEnumerator InitialSetting()
    {
        yield return new WaitUntil(() => DataController.Instance.GetCharacter(Character.Main) != null);

        // 현재 캐릭터
        rau = DataController.Instance.GetCharacter(Character.Main);
        rau.IsMove = false;
        rau.anim.applyRootMotion = false;
        rau.anim.SetFloat(Speed, 0.7f);
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
        //조이스틱 이미지 끄기
        foreach (Image image in JoystickController.instance.joystick.GetComponentsInChildren(typeof(Image), true))
        {
            image.color = Color.clear;
        }
        //조이스틱이 수평으로만 움직이도록
        JoystickController.instance.joystick.AxisOptions = AxisOptions.Horizontal;
        //
        rau.PickUpCharacter();
        //
        
        moveTransforms = new Transform[3];
        moveTransforms[0] = moveLeftAndRight.GetChild(0);
        moveTransforms[1] = moveLeftAndRight.GetChild(1);
        moveTransforms[2] = moveLeftAndRight.GetChild(2);
        Debug.Log("길이 - " + moveTransforms.Length);

        moveLeftAndRight.position = rau.transform.position;
    }
    #endregion

    #region 라우 이동 관련
    void RauMove() {
        
        if (!isStopping)
        {
            //rau.PickUpCharacter();
            //rau.ctrl.enabled = false;
            Vector3 followZofRau = moveLeftAndRight.position;
            followZofRau.z = rau.transform.position.z;
            followZofRau.y = rau.transform.position.y;

            moveLeftAndRight.position = followZofRau;
            // rau.ctrl.Move(rau.transform.position-threeWay[0].position);
            rau.ctrl.Move(new Vector3(0, -1, Time.deltaTime) * rauSpeed);
            rau.anim.SetFloat(SpeedHash, rauSpeed * Time.deltaTime);
            rauSpeed += acceleration * Time.deltaTime;
        }
        // 캐릭터매니저랑 라우랑 위치 일치시키기 => 충돌 감지때문
        gameObject.transform.position = rau.transform.position;
    }
    #endregion

    IEnumerator GoHorizontal(Transform start, Transform target) {
        //Debug.Log(start);
        Debug.Log(target.name + "이동 시작");
        
        var t = 0.0f;
        isMoving = true;
        while (t <= 1) {
            Vector3 moveXofRau = rau.transform.position;
            moveXofRau.x = Vector3.Lerp(start.position, target.position, t).x;
            rau.transform.position = moveXofRau;

            t += Time.deltaTime / 0.5f; //원하는 시간
      
        //    //Debug.Log(target.name + "이동 중" + t);
            yield return null;
        }
        Debug.Log(target.name + "이동 완료");
        isMoving = false;
        rau.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

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
    void CheckCompletion()
    {

        float dis = robber.transform.position.z - rau.transform.position.z;

        if (Mathf.Round(timer) == 0)
        {
            if (dis <= clearDis)
            {
                Debug.Log("성공.");
                StopAllCoroutines();
                DataController.Instance.CurrentMap.MapClear(Time.deltaTime);

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
                StopAllCoroutines();
                DataController.Instance.CurrentMap.MapClear(Time.deltaTime);
            }

        }

    }
    #endregion

    #region Stop코루틴
    IEnumerator Stop(float time) {

        isStopping = true;

        yield return new WaitForSeconds(time);

        isStopping = false;
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
    private void OnDestroy()
    {
        GameObject.Destroy(moveLeftAndRight);
    }
}
