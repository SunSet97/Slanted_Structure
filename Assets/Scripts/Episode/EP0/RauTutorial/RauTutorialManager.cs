using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Interaction.Click;
using static Data.CustomEnum;

namespace Episode.EP0.RauTutorial
{
    public enum TerrainType
    {
        ForestIntro,
        GrassSwipe,
        River,
        Forest,
        ForestWood
    }

    [Serializable]
    public class CheckPoint
    {
        public GameObject ui;
        public Transform checkPoint;
        public bool isChecked;
        public TerrainType terrainType;
    }

    public class RauTutorialManager : MonoBehaviour
    {
        public CheckPoint[] checkPoints;

        [Space(20)] public Button riverTreeButton;
        public Slider riverTreeSlider;
        public GameObject[] grass;

        public GameObject[] riverTrees;

        public GameObject forestTree;
        public Transform[] movePoint;

        [SerializeField] private CamInfo viewForward = new CamInfo
            {camDis = new Vector3(-1f, 1.5f, 0), camRot = new Vector3(10, 90, 0)};

        [SerializeField] private CamInfo viewRiver = new CamInfo
            {camDis = new Vector3(3f, 3f, -10f), camRot = new Vector3(10, 0, 0)};

        [SerializeField] private CamInfo viewQuarter = new CamInfo
            {camDis = new Vector3(-6f, 5f, 0f), camRot = new Vector3(20, 90, 0)};

        [SerializeField] private float shakeTime = 1f;
        [SerializeField] private float shakeAmount = .7f;
        
        private Swipe swipeDir;
        private int grassSwipeIndex;
        private bool isGrassSwipe;
        private bool isGrassMoveForward;
        private int woodKickIndex;
        private int woodSwipeIndex;
        private bool isWoodMoveUp;

        private void Start()
        {
            riverTreeButton.onClick.AddListener(() => { KickWood(riverTreeSlider); });
        }

        public void Play(int checkedIndex)
        {
            var checkPoint = checkPoints[checkedIndex];
            if (checkPoint.isChecked)
            {
                return;
            }

            checkPoint.isChecked = true;

            switch (checkPoint.terrainType)
            {
                case TerrainType.ForestIntro:
                    ForestIntro();
                    break;
                case TerrainType.GrassSwipe:
                {
                    StartCoroutine(GrassSwipe());
                    break;
                }
                case TerrainType.River:
                    StartCoroutine(River());
                    break;
                case TerrainType.Forest:
                    Forest();
                    break;
                case TerrainType.ForestWood:
                    StartCoroutine(KnockDownTree());
                    break;
            }
        }

        private void TouchSlide()
        {
            // https://www.youtube.com/watch?v=98dQBWUyy9M 멀티 터치 참고
            if (JoystickController.Instance.Joystick.Horizontal > 0) swipeDir = Swipe.Right;
            else if (JoystickController.Instance.Joystick.Horizontal < 0) swipeDir = Swipe.Left;
            else if (JoystickController.Instance.Joystick.Vertical < 0) swipeDir = Swipe.Down;
            else swipeDir = Swipe.None;
        }

        // 조이스틱 이미지 껐다 끄기
        void OnOffJoystick(bool isOn)
        {
            foreach (var component in JoystickController.Instance.Joystick.GetComponentsInChildren(typeof(Image), true))
            {
                var image = (Image) component;
                if (isOn && !image.name.Equals("Transparent Dynamic Joystick"))
                {
                    image.color = Color.white - Color.black * 0.3f;
                }
                else
                {
                    image.color = Color.clear;
                }
            }
        }

        /// <summary>
        /// 이동 방식 변환 함수 
        /// </summary>
        /// <param name="methodNum">0 = one dir, 1 = all dir, 2 = other, Other인 경우 인터랙션, 조이스틱 X</param>
        /// <param name="axisNum">0 = both, 1 = hor, 2 = ver</param>
        private static void ChangeJoystickSetting(JoystickInputMethod methodNum, AxisOptions axisNum)
        {
            DataController.Instance.CurrentMap.method = methodNum;
            JoystickController.Instance.SetVisible(methodNum != JoystickInputMethod.Other);
            JoystickController.Instance.Joystick.AxisOptions = axisNum;
        }

        // 숲 초입길
        private void ForestIntro()
        {
            var forestIntro = Array.Find(checkPoints, item => item.terrainType == TerrainType.ForestIntro);
            forestIntro.ui.SetActive(true);
        }

        // 수풀길
        private IEnumerator GrassSwipe()
        {
            var forestIntro = Array.Find(checkPoints, item => item.terrainType == TerrainType.ForestIntro);
            forestIntro.ui.SetActive(false);
            var grassSwipe = Array.Find(checkPoints, item => item.terrainType == TerrainType.GrassSwipe);
            grassSwipe.ui.SetActive(true);

            DataController.Instance.camOffsetInfo = viewForward;

            ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Horizontal); // 이동 해제, 좌우 스와이프만 가능하도록 변경
            JoystickController.Instance.SetJoystickArea(JoystickAreaType.Full);

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.PickUpCharacter();

            while (true)
            {
                TouchSlide();

                if (swipeDir == Swipe.None && isGrassSwipe && isGrassMoveForward)
                {
                    isGrassSwipe = false;
                    isGrassMoveForward = false;
                }

                if (swipeDir == Swipe.Left && !isGrassSwipe && grassSwipeIndex % 2 == 0)
                {
                    if (grass[1 + grassSwipeIndex / 2].activeSelf)
                    {
                        StartCoroutine(MoveGrass(grass[0].transform, grass[1 + grassSwipeIndex / 2].transform,
                            Vector3.forward * 0.2f));
                    }

                    StartCoroutine(MoveForward(mainCharacter.transform.position,
                        grass[1 + grassSwipeIndex / 2].transform.position));
                    grassSwipeIndex++;
                    isGrassSwipe = true;
                }
                else if (swipeDir == Swipe.Right && !isGrassSwipe && grassSwipeIndex % 2 == 1)
                {
                    if (grass[8 + grassSwipeIndex / 2].activeSelf)
                    {
                        StartCoroutine(MoveGrass(grass[7].transform, grass[8 + grassSwipeIndex / 2].transform,
                            -Vector3.forward * 0.2f));
                    }

                    StartCoroutine(MoveForward(mainCharacter.transform.position,
                        grass[8 + grassSwipeIndex / 2].transform.position));
                    grassSwipeIndex++;
                    isGrassSwipe = true;
                }

                if (grassSwipeIndex == 12 && (!isGrassSwipe || isGrassMoveForward))
                {
                    isGrassMoveForward = false;
                    isGrassSwipe = true;
                    grassSwipeIndex++;
                    var riverCheckPoint = Array.Find(checkPoints, item => item.terrainType == TerrainType.River);
                    StartCoroutine(MoveForward(mainCharacter.transform.position,
                        riverCheckPoint.checkPoint.transform.position));
                }

                if (grassSwipeIndex == 13 && (isGrassMoveForward || !isGrassSwipe))
                {
                    isGrassMoveForward = false;
                    isGrassSwipe = false;
                    break;
                }

                if (grassSwipeIndex > 2)
                {
                    grassSwipe.ui.SetActive(false);
                }

                yield return null;
            }

            JoystickController.Instance.SetJoystickArea(JoystickAreaType.Default);
        }

        private IEnumerator MoveGrass(Transform grassTrans, Transform destGrass, Vector3 moveDelta)
        {
            destGrass.gameObject.SetActive(false);
            grassTrans.position = destGrass.position;
            var t = .0f;
            while (t < 1)
            {
                t += Time.deltaTime;
                grassTrans.position += moveDelta;
                yield return null;
            }
        }

        private IEnumerator MoveForward(Vector3 character, Vector3 grass)
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var t = 0f;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            const float speed = 3f;
            var moveVec = new Vector3(grass.x - character.x, 0, 0) * Time.fixedDeltaTime * speed;
            while (t < 1)
            {
                t += Time.fixedDeltaTime * speed;
                mainCharacter.transform.position += moveVec;
                yield return waitForFixedUpdate;
            }

            isGrassMoveForward = true;
        }

        // 물가
        private IEnumerator River()
        {
            yield return new WaitUntil(() => !isGrassSwipe);

            var mapData = DataController.Instance.CurrentMap;
            DataController.Instance.camOffsetInfo.camDis = mapData.camDis;
            DataController.Instance.camOffsetInfo.camRot = mapData.camRot;

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.UseJoystickCharacter();

            ChangeJoystickSetting(JoystickInputMethod.OneDirection, AxisOptions.Both);
        }

        public IEnumerator FallInRiver()
        {
            var tempTree = riverTrees[0].GetComponent<OutlineClickObj>();

            tempTree.enabled = true;

            yield return new WaitUntil(() => tempTree.GetIsClicked());

            tempTree.enabled = false;

            var riverCheckPoint = Array.Find(checkPoints, item => item.terrainType == TerrainType.River);
            riverCheckPoint.ui.SetActive(true);

            DataController.Instance.camOffsetInfo = viewRiver;

            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.PickUpCharacter();

            JoystickController.Instance.StopSaveLoadJoyStick(true);

            yield return new WaitUntil(() => woodKickIndex >= 3);

            riverCheckPoint.ui.SetActive(false);

            DataController.Instance.camOffsetInfo = viewRiver;

            JoystickController.Instance.StopSaveLoadJoyStick(false);

            mainCharacter.UseJoystickCharacter();

            riverTrees[0].SetActive(false);
            riverTrees[1].SetActive(true);
            riverTrees[2].SetActive(true);
        }

        private void KickWood(Slider slider)
        {
            DataController.Instance.Cam.GetComponent<CameraMoving>().Shake(shakeTime, shakeAmount);
            
            if (Mathf.Abs(slider.value) >= 0.35f)
            {
                return;
            }
            
            woodKickIndex++;
        }

        // 나무 숲
        private void Forest()
        {
            DataController.Instance.camOffsetInfo = viewQuarter;

            ChangeJoystickSetting(JoystickInputMethod.AllDirection, AxisOptions.Both);
        }

        private IEnumerator KnockDownTree()
        {
            var rau = DataController.Instance.GetCharacter(Character.Rau);
            forestTree.GetComponent<Animation>().Play();

            var forestWoodCheckPoint = Array.Find(checkPoints, item => item.terrainType == TerrainType.ForestWood);
            forestWoodCheckPoint.ui.SetActive(true);
            DataController.Instance.camOffsetInfo = viewForward;
            ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Vertical); // 이동 해제, 위아래 스와이프만 가능하도록 변경
            rau.PickUpCharacter();
            JoystickController.Instance.SetJoystickArea(JoystickAreaType.Full);
            while (true)
            {
                TouchSlide();
                if (swipeDir == Swipe.Down && woodSwipeIndex < movePoint.Length && !isWoodMoveUp)
                {
                    StartCoroutine(MoveUp(movePoint[woodSwipeIndex].position));
                    isWoodMoveUp = true;
                    woodSwipeIndex++;
                }

                if (woodSwipeIndex >= movePoint.Length && !isWoodMoveUp)
                {
                    forestWoodCheckPoint.ui.SetActive(false);
                    DataController.Instance.camOffsetInfo = viewQuarter;
                    // 둘러보기, 전방향 이동 튜토리얼
                    ChangeJoystickSetting(JoystickInputMethod.AllDirection, 0); // 전방향 이동
                    rau.UseJoystickCharacter();
                    JoystickController.Instance.SetJoystickArea(JoystickAreaType.Default);
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator MoveUp(Vector3 point)
        {
            var rau = DataController.Instance.GetCharacter(Character.Rau);
            var fixedUpdate = new WaitForFixedUpdate();
            var t = .0f;
            var charPos = rau.transform.position;
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime * 2f;
                rau.transform.position = Vector3.Lerp(charPos, point, t);
                yield return fixedUpdate;
            }

            isWoodMoveUp = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue - Color.black * 0.5f;

            for (int i = 0; i < movePoint.Length; i++)
            {
                if (movePoint.Length > i + 1)
                    Gizmos.DrawLine(movePoint[i].position, movePoint[i + 1].position);
                Gizmos.DrawSphere(movePoint[i].position, 0.2f);
            }
        }
    }
}