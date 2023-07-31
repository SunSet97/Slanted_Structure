using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Core;

namespace Episode.EP2.CatchPickpocket
{
    [Serializable]
    public class DovesComoponents
    {
        public List<Animator> anims;
        public List<CameraFollow> camFollow;

        public DovesComoponents(List<Animator> anims, List<CameraFollow> camFollow)
        {
            this.anims = anims;
            this.camFollow = camFollow;
        }

    }

    public class CatchPickpocketManager : MiniGame
    {
        private enum Dir
        {
            Middle,
            Left,
            Right
        }
        
        [Header("라우")] public float rauSpeed;
        [SerializeField] private float acceleration;
        private Transform[] movePoints;
        [FormerlySerializedAs("moveLeftAndRight")] public Transform movePointParent;

        // 소매치기
        [Header("소매치기")]
        public float robberSpeed;
        [SerializeField] private CharacterController pickpocket;

        [Header("장애물들")] public GameObject kickBoard;
        bool isKickBoard;
        public GameObject CollisionObstacles; // 장애물 이동을 위한.. - 킥보드 제외
        public GameObject doves_1;
        public GameObject doves_2;
        public Transform dovesPosition_1;
        public Transform dovesPosition_2;
        public int obstacleIndex;
        public List<DovesComoponents> dovesComponents = new List<DovesComoponents>();
        bool isFlying_doves; // 비둘기 날고 있는지

        [Header("비둘기 속성")] public float doveSpeed;

        [Header("파티클시스템")] public ParticleSystem particle;

        [Header("타이머 설정")] public float timer;

        [Header("라우~소매치기 성공 거리")] public float clearDis;

        [SerializeField] private Dir currentDir;
        public bool isDragged;
        public bool isMoving;
        float facedir = 25;
        
        private CharacterManager mainCharacter;
        private bool isStop;
        private float time;

        private static readonly int Speed = Animator.StringToHash("Speed");
        
        public override void Play()
        {
            InitialSetting();
            IsPlay = true;
        }

        public override void EndPlay(bool isSuccess)
        {
            OnEndPlay?.Invoke(isSuccess);
            IsPlay = false;
        }

        private void FixedUpdate()
        {
            if (!IsPlay)
            {
                return;
            }
            
            if (!isMoving && !isStop)
            {
                if (JoystickController.Instance.Joystick.Horizontal < -.5f && !isDragged && currentDir != Dir.Left)
                {
                    // 왼 드래그 공통
                    isDragged = true;


                    //이동 실행
                    if (currentDir == Dir.Middle)
                    {
                        currentDir = Dir.Left;
                        mainCharacter.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(movePoints[0], movePoints[1]));
                    }
                    else if (currentDir == Dir.Right)
                    {
                        currentDir = Dir.Middle;
                        mainCharacter.transform.rotation = Quaternion.Euler(facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(movePoints[2], movePoints[0]));
                    }
                }
                else if (JoystickController.Instance.Joystick.Horizontal > .5f && !isDragged &&
                         currentDir != Dir.Right)
                {
                    //GoDir = Dir.Right;
                    isDragged = true;
                    if (currentDir == Dir.Middle)
                    {
                        currentDir = Dir.Right;
                        mainCharacter.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(movePoints[0], movePoints[2]));
                    }
                    else if (currentDir == Dir.Left)
                    {
                        currentDir = Dir.Middle;
                        mainCharacter.transform.rotation = Quaternion.Euler(-facedir, 0, facedir);
                        StartCoroutine(GoHorizontal(movePoints[1], movePoints[0]));
                    }
                }
                else if (Mathf.Abs(JoystickController.Instance.Joystick.Horizontal) >= .5f)
                {
                    isDragged = false;
                }
            }

            RauMove();

            DoveMove();

            // 소매치기 이동
            pickpocket.Move(new Vector3(0, -1, robberSpeed * Time.fixedDeltaTime));

            CheckCompletion();
            time -= Time.fixedDeltaTime;
        }

        private void OnTriggerEnter(Collider other)
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
        
        private void InitialSetting()
        {
            // 현재 캐릭터
            mainCharacter = DataController.Instance.GetCharacter(CustomEnum.Character.Main);
            mainCharacter.IsMove = false;
            mainCharacter.CharacterAnimator.applyRootMotion = false;
            mainCharacter.CharacterAnimator.SetFloat(Speed, 0.7f);
            mainCharacter.PickUpCharacter();
            
            time = timer;
            
            pickpocket.transform.position = mainCharacter.transform.position + new Vector3(0, 0, 10);
            
            // 비둘기 애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 1
            // var tmpAnimsList = new List<Animator>();
            // var tmpCamFollowList = new List<CameraFollow>();
            //
            // for (var i = 0; i < doves_1.transform.childCount; i++)
            // {
            //     tmpAnimsList.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<Animator>());
            //     tmpCamFollowList.Add(doves_1.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //     //tmpCamFollowList1[i].SmoothSpeed = a;
            // }
            //
            // dovesComponents.Add(new DovesComoponents(tmpAnimsList, tmpCamFollowList));
            // 비둘기애니메이션, camerafollow 컴포넌트들 리스트에 넣기 - 2
            // tmpAnimsList.Clear();
            // tmpCamFollowList.Clear();
            // for (int i = 0; i < doves_2.transform.childCount; i++)
            // {
            //     tmpAnimsList.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<Animator>());
            //     tmpCamFollowList.Add(doves_2.transform.GetChild(i).gameObject.GetComponent<CameraFollow>());
            //     //tmpCamFollowList2[i].SmoothSpeed = a;
            // }
            // dovesComponents.Add(new DovesComoponents(tmpAnimsList, tmpCamFollowList));
            
            // joystick 안보이지만 실행되도록
            foreach (var component in JoystickController.Instance.Joystick.GetComponentsInChildren(typeof(Image), true))
            {
                var image = (Image)component;
                image.color = Color.clear;
            }

            //조이스틱이 수평으로만 움직이도록
            JoystickController.Instance.Joystick.AxisOptions = AxisOptions.Horizontal;

            movePoints = new Transform[movePointParent.childCount];
            for (var index = 0; index < movePoints.Length; index++)
            {
                movePoints[index] = movePointParent.GetChild(index);
            }

            movePointParent.position = mainCharacter.transform.position;
        }


        private void RauMove()
        {
            if (!isStop)
            {
                var followZofRau = mainCharacter.transform.position;
                followZofRau.x = movePointParent.position.x;
                movePointParent.position = followZofRau;
                
                // 실제 캐릭터가 움직이는거야? 맵이 움직이는거 아니었나
                mainCharacter.CharacterController.Move(new Vector3(0, -1, Time.fixedDeltaTime) * rauSpeed);
                mainCharacter.CharacterAnimator.SetFloat(Speed, rauSpeed * Time.fixedDeltaTime);
                rauSpeed += acceleration * Time.deltaTime;
            }

            // 캐릭터매니저랑 라우랑 위치 일치시키기 => 충돌 감지때문
            gameObject.transform.position = mainCharacter.transform.position;
        }

        private IEnumerator GoHorizontal(Transform start, Transform target)
        {
            //Debug.Log(start);
            Debug.Log(target.name + "이동 시작");

            var t = 0.0f;
            isMoving = true;
            while (t <= 1)
            {
                Vector3 moveXofRau = mainCharacter.transform.position;
                moveXofRau.x = Vector3.Lerp(start.position, target.position, t).x;
                mainCharacter.transform.position = moveXofRau;

                t += Time.deltaTime / 0.5f; //원하는 시간

                //    //Debug.Log(target.name + "이동 중" + t);
                yield return null;
            }

            Debug.Log(target.name + "이동 완료");
            isMoving = false;
            mainCharacter.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        private void PositioningObstacle()
        {

            CollisionObstacles.transform.GetChild(obstacleIndex).gameObject.transform.position +=
                new Vector3(0, 0, 40); // 40만큼 이동

            // 다음에 위치될 obstacle 그룹 설정!
            if (obstacleIndex == 0) obstacleIndex = 1;
            else obstacleIndex = 0;
        }

        private void DoveMove()
        {
            if (isFlying_doves)
            {

                if (obstacleIndex == 0)
                {
                    //Vector3 DesiredPosition = Camera.main.transform.position + cameraOffset_dove;
                    //Vector3 SmoothPosition = Vector3.Lerp(doves_1.transform.position, DesiredPosition, doveSpeed * Time.deltaTime);
                    //doves_1.transform.position = SmoothPosition;
                    doves_1.transform.position = Vector3.MoveTowards(doves_1.transform.position,
                        mainCharacter.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
                }
                else if (obstacleIndex == 1)
                {
                    //Vector3 DesiredPosition = Camera.main.transform.position + cameraOffset_dove;
                    //Vector3 SmoothPosition = Vector3.Lerp(doves_2.transform.position, DesiredPosition, doveSpeed * Time.deltaTime);
                    //doves_2.transform.position = SmoothPosition;
                    doves_2.transform.position = Vector3.MoveTowards(doves_2.transform.position,
                        mainCharacter.transform.position + new Vector3(0, 6, -5), doveSpeed * Time.deltaTime);
                }




            }

        }

        void SetKickBoardPosition()
        {

        }

        private void CheckCompletion()
        {
            var dis = pickpocket.transform.position.z - mainCharacter.transform.position.z;

            if (Mathf.Approximately(time, 0f))
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

        private IEnumerator Stop(float time)
        {

            isStop = true;

            yield return new WaitForSeconds(time);

            isStop = false;
        }

        // 카메라 방향으로 날라가는거
        private IEnumerator Fly(float time)
        {
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

        IEnumerator MoveKickboard(float time)
        {
            yield return new WaitForSeconds(time);

        }
    }
}