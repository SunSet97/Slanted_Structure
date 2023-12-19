﻿using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Utility.Character;
using Utility.Core;
using Utility.Game;
using Utility.Utils;
using Random = UnityEngine.Random;

namespace Episode.EP2.CatchPickpocket
{
    public class CatchPickpocketManager : MiniGame
    {
        private enum Path
        {
            Middle,
            Left,
            Right
        }
#pragma warning disable 0649
        [Header("Response UI")] [SerializeField]
        private Animator responseUi;

        [SerializeField] private Image renderImage;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private Vector3 cameraPositionOffset;
        [SerializeField] private Vector3 cameraAnglesOffset;

        [Header("장애물 Npc")] [SerializeField] private GameObject[] patternPrefabs;
        [SerializeField] private List<ObstacleManager> spawnedList;
        [SerializeField] private float spawnInterval;
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private float obstacleSpeed = 1f;
        [SerializeField] private Transform obstacleRoot;

        [Header("라우")] [SerializeField] private float rauSpeed = 1f;
        [SerializeField] private float rauRunAnimationSpeed = 15f;
        [SerializeField] private float acceleration;
        [SerializeField] private Transform movePointParent;
        [SerializeField] private Transform[] movePoints;
        [SerializeField] private float horizontalMoveSpeed;

        [Header("소매치기")] public float robberSpeed = 1f;
        [SerializeField] private Animator pickpocket;

        [Header("타이머 설정")] [SerializeField] private float timer;

        [Header("라우~소매치기 성공 거리")] [SerializeField]
        private float clearDis;
#pragma warning restore 0649


        private Path currentPath;
        private Path targetPath;
        private bool isHorizontalMoveEnable;
        private CharacterManager mainCharacter;
        private float time;
        private Vector3 runDirection;
        private float originCharacterYPos;
        private float spawnTime;
        private CameraMoving cameraMoving;

        private const float JumpSpeed = 1.5f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int ResetJumpHash = Animator.StringToHash("ResetJump");
        private static readonly int JumpSpeedHash = Animator.StringToHash("JumpSpeed");
        private static readonly int PlayHash = Animator.StringToHash("Play");

        public override void EndPlay(bool isSuccess)
        {
            mainCharacter.CharacterAnimator.SetFloat(JumpSpeedHash, 1);
            renderCamera.gameObject.SetActive(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            pickpocket.gameObject.SetActive(false);
            StopAllCoroutines();
            Destroy(mainCharacter.gameObject.GetComponent<CatchPickpocketPlayer>());
            JoystickController.Instance.SetJoystickArea(CustomEnum.JoystickAreaType.Default);
            if (!isSuccess)
            {
                DataController.Instance.CurrentMap.ResetMap();
            }

            base.EndPlay(isSuccess);
        }

        public override void Play()
        {
            Initialize();
            base.Play();
        }

        private void Initialize()
        {
            foreach (var obstacleManager in spawnedList)
            {
                obstacleManager.Initialize(obstacleSpeed);
            }

            cameraMoving = DataController.Instance.Cam.GetComponent<CameraMoving>();
            originCharacterYPos = -float.MaxValue;
            JoystickController.Instance.SetJoystickArea(CustomEnum.JoystickAreaType.Full);
            runDirection = Vector3.forward;

            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);

            mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
            mainCharacter.CharacterAnimator.applyRootMotion = false;
            mainCharacter.gameObject.AddComponent<CatchPickpocketPlayer>().Init(OnTrigger);

            renderCamera.gameObject.SetActive(true);
            renderCamera.transform.SetParent(mainCharacter.transform);
            renderCamera.transform.localPosition = cameraPositionOffset;
            renderCamera.transform.localEulerAngles = cameraAnglesOffset;

            pickpocket.gameObject.SetActive(true);
            pickpocket.transform.position = mainCharacter.transform.position + new Vector3(0, 0, 10);

            JoystickController.Instance.SetVisible(false);
            JoystickController.Instance.Joystick.AxisOptions = AxisOptions.Horizontal;

            mainCharacter.CharacterAnimator.SetFloat(JumpSpeedHash, JumpSpeed);
        }

        private void Update()
        {
            if (!IsPlay)
            {
                return;
            }

            SetRenderImage();
        }

        private void FixedUpdate()
        {
            if (!IsPlay)
            {
                return;
            }

            Spawn();

            if (CheckDrag())
            {
                MoveHorizontal();
                JoystickController.Instance.ResetJoyStickState();
            }

            Run();
            MoveObstacle(Time.fixedDeltaTime);

            CheckCompletion();
            time += Time.fixedDeltaTime;
        }

        private void Spawn()
        {
            if (time - spawnTime < spawnInterval)
            {
                return;
            }

            spawnTime = time;

            var randomRange = Random.Range(0, patternPrefabs.Length);

            var obstacleManager = ObjectPoolHelper.Get(patternPrefabs[randomRange]).GetComponent<ObstacleManager>();
            obstacleManager.transform.parent = obstacleRoot;
            obstacleManager.transform.SetPositionAndRotation(spawnTransform.transform.position, Quaternion.identity);
            // obstacleManager.transform.position = spawnTransform.transform.position;
            // obstacleManager.transform.localEulerAngles = Vector3.zero;
            // pattern.transform.LookAt(pattern.transform.position - Vector3.forward);

            spawnedList.Add(obstacleManager);
            obstacleManager.Initialize(obstacleSpeed);
        }

        private void MoveObstacle(float deltaTime)
        {
            foreach (var obstacleManager in spawnedList)
            {
                obstacleManager.Move(deltaTime);

                // 나중에, 옆으로 움직이는 친구 있음
                // obstacleManager.transform. = .prodeltaTime
                // // if 카메라 뒤에 있으면 Release
                // if (mainCharacter.transform.position.z - 10 < obstacleManager.transform.position.z)
                // {
                //     obstacleManager.gameObject.SetActive(false);
                //     ObjectPoolHelper.Release(patternPrefabs[obstacleManager.index], obstacleManager.gameObject);
                // }
            }
        }

        private void Run()
        {
            var followZofRau = mainCharacter.transform.position;
            followZofRau.x = movePointParent.position.x;
            movePointParent.position = followZofRau;

            if (Mathf.Approximately(originCharacterYPos, -float.MaxValue))
            {
                originCharacterYPos = mainCharacter.transform.position.y;
                spawnTransform.position = new Vector3(spawnTransform.position.x, mainCharacter.transform.position.y,
                    spawnTransform.position.z);
            }

            if (!JoystickController.Instance.InputJump && mainCharacter.IsJumpEnable())
            {
                cameraMoving.UnFreeze();

                mainCharacter.CharacterController.Move(runDirection * (Time.fixedDeltaTime * rauSpeed));

                mainCharacter.CharacterAnimator.SetBool(ResetJumpHash, true);

                if (!mainCharacter.CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                {
                    var animationSpeed = Mathf.Clamp(rauSpeed * rauRunAnimationSpeed * Time.fixedDeltaTime, 0f, 1f);
                    animationSpeed = Mathf.Lerp(mainCharacter.CharacterAnimator.GetFloat(SpeedHash), animationSpeed,
                        .1f);
                    mainCharacter.CharacterAnimator.SetFloat(SpeedHash, animationSpeed);
                }

                rauSpeed += acceleration * Time.fixedDeltaTime;
                // Debug.Log($"Set Speed {rauSpeed}, Animate Speed - {rauSpeed * rauRunAnimationSpeed * Time.fixedDeltaTime}");
            }

            pickpocket.transform.Translate(0, 0, robberSpeed * Time.fixedDeltaTime);

        }

        private void OnTrigger(bool isEnter, Animator obstacle)
        {
            // Debug.Log($"{obstacle.gameObject}   Trigger!!!");
            responseUi.SetTrigger(PlayHash);

            cameraMoving.Freeze(FreezeType.Y);

            mainCharacter.CharacterAnimator.SetBool(ResetJumpHash, false);
            mainCharacter.CharacterAnimator.SetFloat(SpeedHash, 0);
            Debug.Log("Set Speed 0");
            mainCharacter.TryJump();
        }

        private bool CheckDrag()
        {
            if (!isHorizontalMoveEnable)
            {
                return false;
            }

            if (Mathf.Abs(JoystickController.Instance.Joystick.Horizontal) <= .5f)
            {
                return false;
            }

            if (JoystickController.Instance.Joystick.Horizontal < -.5f)
            {
                if (currentPath == Path.Middle)
                {
                    targetPath = Path.Left;
                }
                else if (currentPath == Path.Right)
                {
                    targetPath = Path.Middle;
                }
                else
                {
                    return false;
                }

                return true;
            }

            if (JoystickController.Instance.Joystick.Horizontal > .5f)
            {
                if (currentPath == Path.Middle)
                {
                    targetPath = Path.Right;
                }
                else if (currentPath == Path.Left)
                {
                    targetPath = Path.Middle;
                }
                else
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void MoveHorizontal()
        {
            // const float faceRot = 25f;

            switch (targetPath)
            {
                case Path.Left:
                    currentPath = Path.Left;
                    StartCoroutine(GoHorizontal(movePoints[1], movePoints[0]));
                    break;
                case Path.Middle:
                {
                    if (currentPath == Path.Right)
                    {
                        StartCoroutine(GoHorizontal(movePoints[2], movePoints[1]));
                    }
                    else if (currentPath == Path.Left)
                    {
                        StartCoroutine(GoHorizontal(movePoints[0], movePoints[1]));
                    }

                    currentPath = Path.Middle;
                    break;
                }
                case Path.Right:
                    currentPath = Path.Right;
                    StartCoroutine(GoHorizontal(movePoints[1], movePoints[2]));
                    break;
            }
        }

        private IEnumerator GoHorizontal(Transform start, Transform target)
        {
            isHorizontalMoveEnable = false;
            var targetDirection = (target.position - start.position).normalized;
            runDirection.x = targetDirection.x * horizontalMoveSpeed;
            var waitForFixedUpdate = new WaitForFixedUpdate();

            if (targetDirection.x > 0)
            {
                // 오른쪽
                while (mainCharacter.transform.position.x < target.position.x)
                {
                    yield return waitForFixedUpdate;
                }
            }
            else
            {
                // 왼쪽
                while (mainCharacter.transform.position.x > target.position.x)
                {
                    yield return waitForFixedUpdate;
                }
            }

            runDirection.x = 0;

            isHorizontalMoveEnable = true;
        }

        private void SetRenderImage()
        {
            renderCamera.transform.localPosition = cameraPositionOffset;
            renderCamera.transform.localEulerAngles = cameraAnglesOffset;

            var renderTexture = new RenderTexture((int)renderImage.rectTransform.rect.width,
                (int)renderImage.rectTransform.rect.height, 0)
            {
                name = "Default_Texture"
            };

            renderCamera.targetTexture = renderTexture;

            renderCamera.Render();

            var currentActiveRT = RenderTexture.active;
            RenderTexture.active = renderCamera.targetTexture;
            var texture = new Texture2D(renderCamera.targetTexture.width, renderCamera.targetTexture.height);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = currentActiveRT;

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            sprite.name = texture.name;

            renderImage.sprite = sprite;
        }

        private void CheckCompletion()
        {
            var dis = pickpocket.transform.position.z - mainCharacter.transform.position.z;

            if (dis <= clearDis)
            {
                Debug.Log("성공.");
                EndPlay(true);
            }
            else if (time >= timer)
            {
                Debug.Log("실패.");
                EndPlay(false);
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
                return;
            foreach (var patternPrefab in patternPrefabs)
            {
                ObjectPoolHelper.Dispose(patternPrefab);
            }
        }
    }
}