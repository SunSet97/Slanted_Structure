using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Data.CustomEnum;

public class RauTutorialManager : MonoBehaviour
{
    public Button riverTreeButton;
    public Slider riverTreeSlider;
    public GameObject[] ui;
    public List<Transform> checkPoint;
    public bool[] isCheckPoint;
    public GameObject[] grass;
    
    internal bool isFallInRiver = false;
    public GameObject[] riverTrees;
    
    public GameObject forestTree;
    public Transform[] movePoint;
    
    
    [SerializeField]
    private CamInfo view_forward = new CamInfo { camDis = new Vector3(-1f, 1.5f, 0), camRot = new Vector3(10, 90, 0) };
    [SerializeField]
    private CamInfo view_river = new CamInfo { camDis = new Vector3(3f, 3f, -10f), camRot = new Vector3(10, 0, 0) };
    [SerializeField]
    private CamInfo view_quarter = new CamInfo { camDis = new Vector3(-6f, 5f, 0f), camRot = new Vector3(20, 90, 0) };
    
    
    private Swipe swipeDir;


    public void Play(int checkedIndex)
    {
        isCheckPoint[checkedIndex] = true;


        if (checkedIndex == 0)
        {
            ForestIntro();
        }
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
        if (JoystickController.instance.joystick.Horizontal > 0) swipeDir = Swipe.Right;
        else if (JoystickController.instance.joystick.Horizontal < 0) swipeDir = Swipe.Left;
        else if (JoystickController.instance.joystick.Vertical < 0) swipeDir = Swipe.Down;
        else swipeDir = Swipe.None;
    }

    // 조이스틱 이미지 껐다 끄기
    void OnOffJoystick(bool isOn)
    {
        foreach (var component in JoystickController.instance.joystick.GetComponentsInChildren(typeof(Image), true))
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
    void ChangeJoystickSetting(JoystickInputMethod methodNum, AxisOptions axisNum)
    {
        var mapData = DataController.instance.currentMap;
        mapData.method = methodNum;
        OnOffJoystick(methodNum != JoystickInputMethod.Other);
        JoystickController.instance.joystick.AxisOptions = axisNum;
    }

    // 숲 초입길
    private void ForestIntro()
    {
        var mapData = DataController.instance.currentMap;
        // 카메라 방향 side
        DataController.instance.camInfo.camDis = mapData.camDis;
        DataController.instance.camInfo.camRot = mapData.camRot;
        // side 이동 및 점프 튜토리얼
        ChangeJoystickSetting(JoystickInputMethod.OneDirection, AxisOptions.Both); // 2D side view 이동
    }

    private int swipeIndex = 0;
    private bool isSwipe = false;
    private bool isMoveForward = false;
    // 수풀길
    private IEnumerator GrassSwipe()
    {
        var rau = DataController.instance.GetCharacter(Character.Rau);
        // 카메라 방향 앞, 어깨 뒤 방향
        DataController.instance.camInfo.camDis = view_forward.camDis;
        DataController.instance.camInfo.camRot = view_forward.camRot;
        // 수풀길 헤쳐가기
        ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Horizontal); // 이동 해제, 좌우 스와이프만 가능하도록 변경
        rau.PickUpCharacter();
        JoystickController.instance.SetJoystickArea(JoystickAreaType.FULL);
        while (true)
        {
            TouchSlide();

            if (swipeDir == Swipe.None && isSwipe && isMoveForward)
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
                break;
            }

            if (swipeIndex > 2)
            {
                ui[2].SetActive(false);
            }

            yield return null;
        }
        JoystickController.instance.SetJoystickArea(JoystickAreaType.DEFAULT);
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
        var rau = DataController.instance.GetCharacter(Character.Rau);
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
    
    // 물가
    private IEnumerator River()
    {
        var rau = DataController.instance.GetCharacter(Character.Rau);
        var mapData = DataController.instance.currentMap;
        yield return new WaitUntil(() => !isSwipe);
        
        rau.UseJoystickCharacter();
        // 카메라 방향 side
        DataController.instance.camInfo.camDis = mapData.camDis;
        DataController.instance.camInfo.camRot = mapData.camRot;
        ChangeJoystickSetting(JoystickInputMethod.OneDirection, AxisOptions.Both); // 2D side view 이동

        
        yield return new WaitUntil(() => isCheckPoint[3] && !isCheckPoint[4]);
        
        ui[3].SetActive(true);
        ChangeJoystickSetting(JoystickInputMethod.OneDirection, AxisOptions.Both); // 2D side view 이동
        
        
        yield return new WaitUntil(() => isFallInRiver);
        
        riverTreeButton.onClick.AddListener(() =>
        {
            KickWood(riverTreeSlider);
        });
        
        var tempTree = riverTrees[0].GetComponent<OutlineClickObj>();
        
        tempTree.enabled = true;
        
        yield return new WaitUntil(() => tempTree.GetIsClicked());

        tempTree.enabled = false;
        // 아웃라인 끄기
        
        ui[4].SetActive(true);
        DataController.instance.camInfo.camDis = view_river.camDis;
        DataController.instance.camInfo.camRot = view_river.camRot;
        ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Both); // 이동 해제
        rau.PickUpCharacter();
        JoystickController.instance.inputDegree = 0;
        JoystickController.instance.inputDirection = Vector2.zero;
        
        
        yield return new WaitUntil(() => kickIndex >= 3);
        
        ui[4].SetActive(false);
        // 카메라 방향 side
        DataController.instance.camInfo.camDis = view_river.camDis;
        DataController.instance.camInfo.camRot = view_river.camRot;
        ChangeJoystickSetting(0, 0); // 2D side view 이동
        rau.UseJoystickCharacter();
        riverTrees[0].SetActive(false);
        riverTrees[1].SetActive(true);
        riverTrees[2].SetActive(true);
    }
    
    private int kickIndex = 0;

    public void KickWood(Slider slider)
    {
        if (Mathf.Abs(slider.value) >= 0.35f) return;

        kickIndex++;
        Debug.Log("kick");
    }

    // 나무 숲
    private void Forest()
    {
        // 카메라 방향 앞, 쿼터뷰
        DataController.instance.camInfo.camDis = view_quarter.camDis;
        DataController.instance.camInfo.camRot = view_quarter.camRot;
        // 둘러보기, 전방향 이동 튜토리얼
        ChangeJoystickSetting(JoystickInputMethod.AllDirection, AxisOptions.Both); // 전방향 이동
        ui[5].SetActive(true);
    }
    private int woodSwipeIndex = 0;
    private bool isMoveUp = false;

    private IEnumerator KnockDownTree()
    {
        var rau = DataController.instance.GetCharacter(Character.Rau);
        forestTree.GetComponent<Animation>().Play();
        ui[6].SetActive(true);
        DataController.instance.camInfo.camDis = view_forward.camDis;
        DataController.instance.camInfo.camRot = view_forward.camRot;
        ChangeJoystickSetting(JoystickInputMethod.Other, AxisOptions.Vertical); // 이동 해제, 위아래 스와이프만 가능하도록 변경
        rau.PickUpCharacter();
        JoystickController.instance.SetJoystickArea(JoystickAreaType.FULL);
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
                DataController.instance.camInfo.camDis = view_quarter.camDis;
                DataController.instance.camInfo.camRot = view_quarter.camRot;
                // 둘러보기, 전방향 이동 튜토리얼
                ChangeJoystickSetting(JoystickInputMethod.AllDirection, 0); // 전방향 이동
                rau.UseJoystickCharacter();
                JoystickController.instance.SetJoystickArea(JoystickAreaType.DEFAULT);
                yield break;
            }
            yield return null;
        }
    }
    
    IEnumerator MoveUp(Vector3 point)
    {
        var rau = DataController.instance.GetCharacter(Character.Rau);
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
