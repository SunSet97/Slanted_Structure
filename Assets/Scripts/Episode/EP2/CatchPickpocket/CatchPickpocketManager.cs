using System.Collections;
using System.Collections.Generic;
using Data;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
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

        [Header("Response UI")] [SerializeField]
        private Animator responseUi;

        [SerializeField] private Image renderImage;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private Vector3 cameraPositionOffset;
        [SerializeField] private Vector3 cameraAnglesOffset;

        [Header("장애물 Npc")] [SerializeField] private GameObject[] patternPrefabs;
        [SerializeField] private List<GameObject> spawnedList;
        [SerializeField] private float spawnInterval;
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private float obstacleSpeed = 1f;
        [SerializeField] private Transform obstacleRoot;

        [Header("라우")] [SerializeField] private float rauRunAnimationSpeed = 1f;
        [SerializeField] private float rauSpeed = 1f;
        [SerializeField] private float acceleration;
        [SerializeField] private Transform movePointParent;
        [SerializeField] private Transform[] movePoints;
        [SerializeField] private float horizontalMoveSpeed;

        [Header("소매치기")] public float robberSpeed = 1f;
        [SerializeField] private Animator pickpocket;

        [Header("타이머 설정")] [SerializeField] private float timer;

        [Header("라우~소매치기 성공 거리")] [SerializeField]
        private float clearDis;

        private Path currentPath;
        private Path targetPath;
        private bool isMoving;
        private bool isStop;
        private CharacterManager mainCharacter;
        private float time;
        private Vector3 runDirection;
        private float originCharacterYPos;
        private float spawnTime;

        private static readonly int Speed = Animator.StringToHash("Speed");

        public override void EndPlay(bool isSuccess)
        {
            StopAllCoroutines();
            Destroy(mainCharacter.gameObject.GetComponent<CatchPickpocketPlayer>());
            JoystickController.Instance.SetJoystickArea(CustomEnum.JoystickAreaType.Default);
            if (!isSuccess)
            {
                DataController.Instance.CurrentMap.ResetMap();
            }

            OnEndPlay?.Invoke(isSuccess);
            IsPlay = false;
        }

        public override void Play()
        {
            Initialize();
            base.Play();
        }

        private void Initialize()
        {
            foreach (var t in spawnedList)
            {
                var obstacleManager = t.GetComponent<ObstacleManager>();
                obstacleManager.Initialize(obstacleSpeed);
            }
            
            originCharacterYPos = -float.MaxValue;
            JoystickController.Instance.SetJoystickArea(CustomEnum.JoystickAreaType.Full);
            runDirection = Vector3.forward;

            mainCharacter = DataController.Instance.GetCharacter(CustomEnum.Character.Main);
            mainCharacter.CharacterAnimator.applyRootMotion = false;
            mainCharacter.CharacterAnimator.SetFloat(Speed, 0.7f);
            mainCharacter.gameObject.AddComponent<CatchPickpocketPlayer>().Init(OnTrigger);

            renderCamera.transform.SetParent(mainCharacter.transform);
            renderCamera.transform.localPosition = cameraPositionOffset;
            renderCamera.transform.localEulerAngles = cameraAnglesOffset;

            pickpocket.transform.position = mainCharacter.transform.position + new Vector3(0, 0, 10);

            JoystickController.Instance.SetVisible(false);
            JoystickController.Instance.Joystick.AxisOptions = AxisOptions.Horizontal;
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

            var pattern = Instantiate(patternPrefabs[randomRange], obstacleRoot);

            pattern.transform.position = spawnTransform.transform.position;
            
            var t = pattern.transform.position;
            t.y = originCharacterYPos;
            pattern.transform.position = t;
            
            // pattern.transform.LookAt(pattern.transform.position - Vector3.forward);
            pattern.transform.localEulerAngles = Vector3.zero;
            var obstacleManager = pattern.GetComponent<ObstacleManager>();
            obstacleManager.Initialize(obstacleSpeed);
        }

        private void Run()
        {
            var followZofRau = mainCharacter.transform.position;
            followZofRau.x = movePointParent.position.x;
            movePointParent.position = followZofRau;

            if (Mathf.Approximately(originCharacterYPos, -float.MaxValue))
            {
                originCharacterYPos = mainCharacter.transform.position.y;
            }

            if (mainCharacter.IsJumpEnable())
            {
                // Debug.Log("움직이는 중");
                mainCharacter.CharacterController.Move(runDirection * (Time.fixedDeltaTime * rauSpeed));
                mainCharacter.CharacterAnimator.SetFloat(Speed, rauSpeed * rauRunAnimationSpeed * Time.fixedDeltaTime);
                rauSpeed += acceleration * Time.fixedDeltaTime;
            }
            else
            {
                // Debug.Log("멈춤");
                mainCharacter.CharacterAnimator.SetFloat(Speed, 0);
            }

            pickpocket.transform.Translate(0, 0, robberSpeed * Time.fixedDeltaTime);

        }

        private void OnTrigger(bool isEnter, Animator obstacle)
        {
            // Debug.Log($"{obstacle.gameObject}   Trigger!!!");
            // Instantiate(responseUiPrefab, obstacle.transform.position, obstacle.transform.rotation);
            responseUi.SetTrigger("Play");

            DataController.Instance.Cam.GetComponent<CameraMoving>().Freeze(FreezeType.Y);

            mainCharacter.TryJump();

            // isStop = true;
            if (isStop)
            {
                // obstacle.Stop
            }
            else
            {
                // obstacle.Move
            }
        }

        private bool CheckDrag()
        {
            if (isMoving)
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
            isMoving = true;
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

            isMoving = false;
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
    }
}