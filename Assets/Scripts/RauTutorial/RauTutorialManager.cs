using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RauTutorialManager : MonoBehaviour
{
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

    public MapData mapdata;
    public Swipe swipeDir;
    public GameObject[] ui;
    public List<Transform> checkPoint;
    public bool[] isCheckPoint;
    public GameObject[] grass;
    private CameraSetting view_side = new CameraSetting() { camDis = new Vector3(0, 1.5f, -5f), camRot = Vector3.zero };
    private CameraSetting view_forward = new CameraSetting() { camDis = new Vector3(-1f, 1.5f, 0), camRot = new Vector3(10, 90, 0) };
    private CameraSetting view_river = new CameraSetting() { camDis = new Vector3(3f, 3f, -10f), camRot = new Vector3(10, 0, 0) };
    private CameraSetting view_quarter = new CameraSetting() { camDis = new Vector3(-6f, 5f, 0f), camRot = new Vector3(20, 90, 0) };

    void Update()
    {
        if (isCheckPoint[5]) { ui[3].SetActive(false); Forest(); }
        else if (isCheckPoint[3]) { ui[2].SetActive(false); River(); }
        else if (isCheckPoint[2]) { ui[2].SetActive(true); ui[1].SetActive(false); ui[0].SetActive(false); GrassSwipe(); }
        else if (isCheckPoint[0]) ForestIntro();
        
    }

    // 슬라이드 판정
    void TouchSlide()
    {
        // https://www.youtube.com/watch?v=98dQBWUyy9M 멀티 터치 참고
        if (DataController.instance_DataController.joyStick.Horizontal > 0) swipeDir = Swipe.Right;
        else if (DataController.instance_DataController.joyStick.Horizontal < 0) swipeDir = Swipe.Left;
        else if (DataController.instance_DataController.joyStick.Vertical < 0) swipeDir = Swipe.Down;
        else swipeDir = Swipe.none;
    }

    // 조이스틱 이미지 껐다 끄기
    void OnOffJoystick(bool isOn)
    {
        foreach (Image image in DataController.instance_DataController.joyStick.GetComponentsInChildren<Image>())
        {
            if (isOn && image.name != "Transparent Dynamic Joystick") image.color = Color.white - Color.black * 0.3f;
            else image.color = Color.clear;
        }
    }

    // 이동 방식 변환 함수
    void ChangeJoystickSetting(int methodNum, int axisNum)
    {
        mapdata.method = (MapData.JoystickInputMethod)methodNum; // 0 = one dir, 1 = all dir, 2 = other
        OnOffJoystick(methodNum != 2); // other일 경우 인터렉션 부분이므로 조이스틱 안보이게 함
        DataController.instance_DataController.joyStick.AxisOptions = (AxisOptions)axisNum; // 0 = both, 1 = hor, 2 = ver
    }

    // 숲 초입길
    void ForestIntro()
    {
        // 카메라 방향 side
        DataController.instance_DataController.camDis = view_side.camDis; DataController.instance_DataController.rot = view_side.camRot;
        // side 이동 및 점프 튜토리얼
        ChangeJoystickSetting(0, 0); // 2D side view 이동
    }

    int swipe = 0;
    bool isSwipe = false;
    bool isMoveForward = false;
    // 수풀길
    void GrassSwipe()
    {
        // 카메라 방향 앞, 어깨 뒤 방향
        DataController.instance_DataController.camDis = view_forward.camDis; DataController.instance_DataController.rot = view_forward.camRot;
        // 수풀길 헤쳐가기
        ChangeJoystickSetting(2, 1); // 이동 해제, 좌우 스와이프만 가능하도록 변경
        DataController.instance_DataController.currentChar.PickUpCharacter();
        TouchSlide();
        if (swipeDir == Swipe.Left && !isSwipe && swipe % 2 == 0)
        {
            if (grass[1 + swipe / 2].activeSelf)
            {
                grass[0].transform.position = grass[1 + swipe / 2].transform.position;
                grass[1 + swipe / 2].SetActive(false);
            }
            isSwipe = true;
            StartCoroutine(MoveForward(DataController.instance_DataController.currentChar.transform.position, grass[1 + swipe / 2].transform.position));
            swipe++;
        }
        else if (swipeDir == Swipe.Right && !isSwipe && swipe % 2 == 1)
        {
            if (grass[8 + swipe / 2].activeSelf)
            {
                grass[7].transform.position = grass[8 + swipe / 2].transform.position;
                grass[8 + swipe / 2].SetActive(false);
            }
            isSwipe = true;
            StartCoroutine(MoveForward(DataController.instance_DataController.currentChar.transform.position, grass[8 + swipe / 2].transform.position));
            swipe++;
        }
        else if (swipeDir == Swipe.none && isSwipe && !isMoveForward)
        {
            isSwipe = false;
        }
        if (swipe == 12 && !isMoveForward)
        {
            swipe++;
            StartCoroutine(MoveForward(DataController.instance_DataController.currentChar.transform.position, checkPoint[3].transform.position));
        }
        grass[0].transform.position += Vector3.forward * 0.2f;
        grass[7].transform.position -= Vector3.forward * 0.2f;
        if (swipe > 2) ui[2].SetActive(false);
    }

    IEnumerator MoveForward(Vector3 character,Vector3 grass)
    {
        isMoveForward = true;
        for (int i=0; i < 20; i++)
        {
            DataController.instance_DataController.currentChar.transform.position += (new Vector3(grass.x, character.y, character.z) - character) / 20f;
            yield return new WaitForSeconds(0.0001f);
        }
        isMoveForward = false;
    }


    bool isRiver = false;
    public bool isFallInRiver = false;
    bool isKickWood = false;
    bool isWoodBridge = false;
    public GameObject[] riverTrees;
    // 물가
    void River()
    {
        if (!isRiver)
        {
            DataController.instance_DataController.currentChar.UseJoystickCharacter();
            // 카메라 방향 side
            DataController.instance_DataController.camDis = view_side.camDis; DataController.instance_DataController.rot = view_side.camRot;
            ChangeJoystickSetting(0, 0); // 2D side view 이동
            isRiver = true;
        }

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
            DataController.instance_DataController.camDis = view_river.camDis; DataController.instance_DataController.rot = view_river.camRot;
            ChangeJoystickSetting(2, 0); // 이동 해제
            DataController.instance_DataController.currentChar.PickUpCharacter();
            DataController.instance_DataController.inputDegree = 0;
            DataController.instance_DataController.inputDirection = Vector2.zero;
        }
        else if (isKickWood && !isWoodBridge && kickIndex >= 3)
        {
            ui[4].SetActive(false);
            // 카메라 방향 side
            DataController.instance_DataController.camDis = view_river.camDis; DataController.instance_DataController.rot = view_river.camRot;
            ChangeJoystickSetting(0, 0); // 2D side view 이동
            DataController.instance_DataController.currentChar.UseJoystickCharacter();
            riverTrees[0].SetActive(false);
            riverTrees[1].SetActive(true);
            riverTrees[2].SetActive(true);
            isWoodBridge = true;
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

    public GameObject forestTree;
    bool isWoodDrop=false;
    bool isSwipeWood = false;
    int woodSwipeIndex = 0;
    bool isMoveUp = false;
    public Transform[] movePoint;
    // 나무 숲
    void Forest()
    {
        // 카메라 방향 앞, 쿼터뷰
        DataController.instance_DataController.camDis = view_quarter.camDis; DataController.instance_DataController.rot = view_quarter.camRot;
        // 둘러보기, 전방향 이동 튜토리얼
        ChangeJoystickSetting(1, 0); // 전방향 이동
        ui[5].SetActive(true);

        // 특정 지점에서 나무 쓰러짐
        if (isCheckPoint[6]&& !isWoodDrop)
        {
            isWoodDrop = true;
            forestTree.GetComponent<Animation>().Play();
        }
        // 스와이프로 나무 넘어가기
        else if(isCheckPoint[6]&&isWoodDrop&& !isSwipeWood)
        {
            ui[6].SetActive(true);
            // 카메라 방향 앞, 쿼터뷰
            DataController.instance_DataController.camDis = view_forward.camDis; DataController.instance_DataController.rot = view_forward.camRot;
            ChangeJoystickSetting(2, 2); // 이동 해제, 위아래 스와이프만 가능하도록 변경
            TouchSlide();
            DataController.instance_DataController.currentChar.PickUpCharacter();
            if (swipeDir == Swipe.Down && woodSwipeIndex <= 3 && !isMoveUp)
            {
                StartCoroutine(MoveUp(DataController.instance_DataController.currentChar.transform.position, movePoint[woodSwipeIndex].position));
                woodSwipeIndex++;
            }
            if (woodSwipeIndex > 3)
            {
                ui[6].SetActive(false);
                isSwipeWood = true;
            }
        }
        else if (isCheckPoint[6] && isSwipeWood)
        {
            // 카메라 방향 앞, 쿼터뷰
            DataController.instance_DataController.camDis = view_quarter.camDis; DataController.instance_DataController.rot = view_quarter.camRot;
            // 둘러보기, 전방향 이동 튜토리얼
            ChangeJoystickSetting(1, 0); // 전방향 이동
            DataController.instance_DataController.currentChar.UseJoystickCharacter();
        }

        // 시네마틱 씬 전환
    }
    
    IEnumerator MoveUp(Vector3 character, Vector3 point)
    {
        isMoveUp = true;
        for (int i = 0; i < 20; i++)
        {
            DataController.instance_DataController.currentChar.transform.position += (point-character) / 20f;
            yield return new WaitForSeconds(0.0001f);
        }
        isMoveUp = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue - Color.black * 0.5f;
        for (int i = 0; i < movePoint.Length - 1; i++) { Gizmos.DrawLine(movePoint[i].position, movePoint[i + 1].position); Gizmos.DrawSphere(movePoint[i].position, 0.2f); }
        Gizmos.DrawSphere(movePoint[movePoint.Length - 1].position, 0.2f);
    }
}
