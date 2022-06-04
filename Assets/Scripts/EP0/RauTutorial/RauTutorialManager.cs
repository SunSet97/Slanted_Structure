using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Data.CustomEnum;

public class RauTutorialManager : MonoBehaviour
{
    [System.Serializable]
    public struct CameraSetting
    {
        public Vector3 camDis;
        public Vector3 camRot;
    }

    public enum Swipe
    {
        Left,
        Right,
        Down,
        none
    }
    private CharacterManager rau;
    public MapData mapData;
    public Swipe swipeDir;
    public GameObject[] ui;
    public List<Transform> checkPoint;
    public bool[] isCheckPoint;
    public GameObject[] grass;
    [SerializeField] private CameraSetting view_side = new CameraSetting() { camDis = new Vector3(0, 1.5f, -5f), camRot = Vector3.zero };
    private CameraSetting view_forward = new CameraSetting() { camDis = new Vector3(-1f, 1.5f, 0), camRot = new Vector3(10, 90, 0) };
    private CameraSetting view_river = new CameraSetting() { camDis = new Vector3(3f, 3f, -10f), camRot = new Vector3(10, 0, 0) };
    private CameraSetting view_quarter = new CameraSetting() { camDis = new Vector3(-6f, 5f, 0f), camRot = new Vector3(20, 90, 0) };

    void Start()
    {
        rau = DataController.instance.GetCharacter(Character.Rau);
        mapData = DataController.instance.currentMap;
    }

    public void Play(int checkedIndex)
    {
        if(checkedIndex == 0)
            ForestIntro();
        else if (checkedIndex == 2)
        {
            ui[0].SetActive(false);
            ui[1].SetActive(false);
            ui[2].SetActive(true);
            StartCoroutine(GrassSwipe());
        }else if (checkedIndex == 3)
        {
            ui[2].SetActive(false);
            StartCoroutine(River());
        }else if (checkedIndex == 5)
        {
            ui[3].SetActive(false);
            Forest();
        }else if (checkedIndex == 6)
        {
            StartCoroutine(KnockDownTree());
        }
    }

    // 슬라이드 판정
    void TouchSlide()
    {
        // https://www.youtube.com/watch?v=98dQBWUyy9M 멀티 터치 참고
        if (DataController.instance.joyStick.Horizontal > 0) swipeDir = Swipe.Right;
        else if (DataController.instance.joyStick.Horizontal < 0) swipeDir = Swipe.Left;
        else if (DataController.instance.joyStick.Vertical < 0) swipeDir = Swipe.Down;
        else swipeDir = Swipe.none;
    }

    // 조이스틱 이미지 껐다 끄기
    void OnOffJoystick(bool isOn)
    {
        foreach (Image image in DataController.instance.joyStick.GetComponentsInChildren(typeof(Image), true))
        {
            if (isOn && !image.name.Equals("Transparent Dynamic Joystick")) image.color = Color.white - Color.black * 0.3f;
            else image.color = Color.clear;
        }
    }

    // 이동 방식 변환 함수
    void ChangeJoystickSetting(JoystickInputMethod methodNum, AxisOptions axisNum)
    {
        mapData.method = methodNum; // 0 = one dir, 1 = all dir, 2 = other
        OnOffJoystick(methodNum != JoystickInputMethod.Other); // other일 경우 인터렉션 부분이므로 조이스틱 안보이게 함
        DataController.instance.joyStick.AxisOptions = axisNum; // 0 = both, 1 = hor, 2 = ver
    }

    // 숲 초입길
    private void ForestIntro()
    {
        // 카메라 방향 side
        DataController.instance.camDis = view_side.camDis;
        DataController.instance.camRot = view_side.camRot;
        // side 이동 및 점프 튜토리얼
        ChangeJoystickSetting(JoystickInputMethod.OneDirection, AxisOptions.Both); // 2D side view 이동
    }

    private int swipeIndex = 0;
    private bool isSwipe = false;
    private bool isMoveForward = false;
    // 수풀길
    private IEnumerator GrassSwipe()
    {
        // 카메라 방향 앞, 어깨 뒤 방향
        DataController.instance.camDis = view_forward.camDis;
        DataController.instance.camRot = view_forward.camRot;
        // 수풀길 헤쳐가기
        ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Horizontal); // 이동 해제, 좌우 스와이프만 가능하도록 변경
        rau.PickUpCharacter();
        while (true)
        {
            TouchSlide();

            if (swipeDir == Swipe.none && isSwipe && isMoveForward)
            {
                isSwipe = false;
                isMoveForward = false;
            }

            if (swipeDir == Swipe.Left && !isSwipe && swipeIndex % 2 == 0)
            {
                if (grass[1 + swipeIndex / 2].activeSelf)
                {
                    StartCoroutine(MoveGrass(grass[0].transform, grass[1 + swipeIndex / 2].transform,
                        Vector3.forward * 0.2f));
                }

                StartCoroutine(MoveForward(rau.transform.position, grass[1 + swipeIndex / 2].transform.position));
                swipeIndex++;
                isSwipe = true;
            }
            else if (swipeDir == Swipe.Right && !isSwipe && swipeIndex % 2 == 1)
            {
                if (grass[8 + swipeIndex / 2].activeSelf)
                {
                    StartCoroutine(MoveGrass(grass[7].transform, grass[8 + swipeIndex / 2].transform,
                        -Vector3.forward * 0.2f));
                }

                StartCoroutine(MoveForward(rau.transform.position, grass[8 + swipeIndex / 2].transform.position));
                swipeIndex++;
                isSwipe = true;
            }

            if (swipeIndex == 12 && (!isSwipe || isMoveForward))
            {
                isMoveForward = false;
                isSwipe = true;
                swipeIndex++;
                StartCoroutine(MoveForward(rau.transform.position, checkPoint[3].transform.position));
            }
            if (swipeIndex == 13 && (isMoveForward || !isSwipe))
            {
                isMoveForward = false;
                isSwipe = false;
                yield break;
            }



            if (swipeIndex > 2) ui[2].SetActive(false);

            yield return null;
        }
    }

    IEnumerator MoveGrass(Transform grassTrans, Transform destGrass, Vector3 moveVec)
    {
        destGrass.gameObject.SetActive(false);
        grassTrans.position = destGrass.position;
        var t = .0f;
        while (t < 1)
        {
            t += Time.deltaTime;
            grassTrans.position += moveVec;
            yield return null;
        }
    }
    IEnumerator MoveForward(Vector3 character,Vector3 grass)
    {
        var t = .0f;
        var fixedUpdate = new WaitForFixedUpdate();
        var moveVec = new Vector3(grass.x - character.x, 0, 0) * Time.fixedDeltaTime * 3;
        while (t < 1)
        {
            t += Time.fixedDeltaTime * 3;
            rau.transform.position += moveVec;
            yield return fixedUpdate;
        }
        isMoveForward = true;
    }


    public bool isFallInRiver = false;
    bool isKickWood = false;
    bool isWoodBridge = false;
    public GameObject[] riverTrees;
    // 물가
    private IEnumerator River()
    {
        while (true)
        {
            if (!isSwipe)
            {
                rau.UseJoystickCharacter();
                // 카메라 방향 side
                DataController.instance.camDis = view_side.camDis;
                DataController.instance.camRot = view_side.camRot;
                ChangeJoystickSetting(0, 0); // 2D side view 이동
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (isCheckPoint[3] && !isCheckPoint[4])
            {
                ui[3].SetActive(true);
                ChangeJoystickSetting(0, 0); // 2D side view 이동
            }

            // 인터렉트할 물건 찾기
            if (isFallInRiver && !isKickWood)
            {
                riverTrees[0].GetComponent<Interact_ObjectWithRau>().enabled = true;
                if (riverTrees[0].GetComponent<Interact_ObjectWithRau>().isTouched)
                {
                    riverTrees[0].GetComponent<Interact_ObjectWithRau>().offset = Vector3.zero;
                    isKickWood = true;
                }
            }

            // 나무 발로 차서 넘어뜨리기
            if (isKickWood && !isWoodBridge && kickIndex < 3)
            {
                ui[4].SetActive(true);
                DataController.instance.camDis = view_river.camDis;
                DataController.instance.camRot = view_river.camRot;
                ChangeJoystickSetting(JoystickInputMethod.Other, 0); // 이동 해제
                rau.PickUpCharacter();
                DataController.instance.inputDegree = 0;
                DataController.instance.inputDirection = Vector2.zero;
            }
            else if (isKickWood && !isWoodBridge && kickIndex >= 3)
            {
                ui[4].SetActive(false);
                // 카메라 방향 side
                DataController.instance.camDis = view_river.camDis;
                DataController.instance.camRot = view_river.camRot;
                ChangeJoystickSetting(0, 0); // 2D side view 이동
                rau.UseJoystickCharacter();
                riverTrees[0].SetActive(false);
                riverTrees[1].SetActive(true);
                riverTrees[2].SetActive(true);
                isWoodBridge = true;
            }

            yield return null;
        }
    }

    int kickIndex = 0;
    public void KickWood(Slider slider)
    {
        if (Mathf.Abs(slider.value) < 0.35f)
        {
            kickIndex++;
            Debug.Log("kick");
        }
    }

    // 나무 숲
    private void Forest()
    {
        // 카메라 방향 앞, 쿼터뷰
        DataController.instance.camDis = view_quarter.camDis;
        DataController.instance.camRot = view_quarter.camRot;
        // 둘러보기, 전방향 이동 튜토리얼
        ChangeJoystickSetting(JoystickInputMethod.AllDirection, 0); // 전방향 이동
        ui[5].SetActive(true);
    }
    
    public GameObject forestTree;
    int woodSwipeIndex = 0;
    bool isMoveUp = false;
    public Transform[] movePoint;

    private IEnumerator KnockDownTree()
    {
        forestTree.GetComponent<Animation>().Play();
        ui[6].SetActive(true);
        DataController.instance.camDis = view_forward.camDis;
        DataController.instance.camRot = view_forward.camRot;
        ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Vertical); // 이동 해제, 위아래 스와이프만 가능하도록 변경
        rau.PickUpCharacter();
        
        while (true)
        {
            TouchSlide();
            if (swipeDir == Swipe.Down && woodSwipeIndex < movePoint.Length && !isMoveUp)
            {
                StartCoroutine(MoveUp(movePoint[woodSwipeIndex].position));
                isMoveUp = true;
                woodSwipeIndex++;
            }

            if (woodSwipeIndex >= movePoint.Length && !isMoveUp)
            {
                ui[6].SetActive(false);
                DataController.instance.camDis = view_quarter.camDis;
                DataController.instance.camRot = view_quarter.camRot;
                // 둘러보기, 전방향 이동 튜토리얼
                ChangeJoystickSetting(JoystickInputMethod.AllDirection, 0); // 전방향 이동
                rau.UseJoystickCharacter();
                yield break;
            }
            yield return null;
        }
    }
    
    IEnumerator MoveUp(Vector3 point)
    {
        var fixedUpdate = new WaitForFixedUpdate();
        var t = .0f;
        var charPos = rau.transform.position;
        while (t <= 1f)
        {
            t += Time.fixedDeltaTime * 2f;
            rau.transform.position = Vector3.Lerp(charPos, point, t);
            yield return fixedUpdate;
        }
        isMoveUp = false;
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
