using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamburgerStoreManager : MonoBehaviour
{
    // day 정보
    int day_info; // 1이면 day1, 2이면 day2, 3이면 day3임.

    // 스핏
    public GameObject speatPrefab; // 우선 캡슐 오브젝트로 대체
    CharacterManager speatCharacterManager;
    Animator speatAnimator;
    bool isFinishMoving_speat = false;
    bool isMoving_speat = false;
    Vector3 speatMoveDir;
    Vector3 target; // 가야할 포인트
    float speatRot;


    // 라우
    CharacterManager character;

    // 스핏이 지나갈 포인트
    public GameObject speatPointParent;
    public List<Transform> speatPoints; // 스핏이 지나갈 길의 포인트들
    public float speatSpeed;
    int nextPassPointIndex = 0; // 가야할 포인트 인덱스
    public CanvasControl canvasControl;

    bool isSpeatMoving = false; // true면 스핏이 움직이고 있는 상태
    bool isStartConversation_SpeatRau = false; // 스핏-라우 대화 시작

    // 클리어박스
    Transform clearBox;

    // 투명 벽
    public BoxCollider wall;


    // Start is called before the first frame update
    void Start()
    {

        // ★ 시네마틱으로 변경되면 if (DataController.instance_DataController.mapCode == "201110" || DataController.instance_DataController.mapCode == "202110") 으로 변경
        if (DataController.instance_DataController.mapCode == "201110" || DataController.instance_DataController.mapCode == "202210"
            || DataController.instance_DataController.mapCode == "203110" || DataController.instance_DataController.mapCode == "205110")
        {

            // 캐릭터 맵으로 이동시키기
            DataController.instance_DataController.isMapChanged = true;

            // 스토리 정보 수정
            DataController.instance_DataController.charData.story = 1;
            DataController.instance_DataController.charData.storyBranch = 2;
            DataController.instance_DataController.charData.storyBranch_scnd = 3;
            DataController.instance_DataController.charData.dialogue_index = 4;

            // 스핏 포인트 리스트에 넣기
            speatPointParent.SetActive(false);
            for (int i = 0; i < speatPointParent.transform.childCount; i++)
            {
                speatPoints.Add(speatPointParent.transform.GetChild(i).gameObject.transform);
            }

            if (!canvasControl) canvasControl = CanvasControl.instance_CanvasControl;

            if (DataController.instance_DataController.mapCode == "201110")
            {
                day_info = 1; // day1
            }
            else if (DataController.instance_DataController.mapCode == "202210")
            {
                day_info = 2; // day2
            }
            else if (DataController.instance_DataController.mapCode == "203110")
            {
                day_info = 3; // day3
            }

        }




    }

    // Update is called once per frame
    void Update()
    {
        if (DataController.instance_DataController.currentMap.mapCode == "201110" || DataController.instance_DataController.currentMap.mapCode== "202110"
            || DataController.instance_DataController.mapCode == "203110" || DataController.instance_DataController.mapCode == "205110")
        {

            // 캐릭터 맵으로 옮긴 후 카메라 무빙 끄기
            if (DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled == true)
            {
                DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled = false;
                DataController.instance_DataController.isMapChanged = true;
            }

            if (!speatCharacterManager)
            {
                speatCharacterManager = speatPrefab.GetComponent<CharacterManager>();
                speatCharacterManager.isControlled = false; // 뿌앙
            }

            if (!clearBox)
            {
                clearBox = DataController.instance_DataController.currentMap.positionSets[0].clearBox;
            }

            if (!character)
            {
                character = DataController.instance_DataController.currentChar;
            }

            if (!speatAnimator)
            {
                speatAnimator = speatPrefab.GetComponent<Animator>();
            }

            string storyNum = DataController.instance_DataController.charData.story.ToString() + "_" + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd.ToString() + "_" + DataController.instance_DataController.charData.dialogue_index.ToString();
            //print(storyNum);

            // day1.
            if (day_info == 1)
            {

                // 라우 독백끝나면 실행
                if (storyNum == "1_2_3_6" && canvasControl.dialogueCnt >= canvasControl.dialogueLen && canvasControl.isPossibleCnvs && !isStartConversation_SpeatRau)
                {
                    SpeatInteraction(1); // 스핏의 첫번째 인터랙션 플레이 - 스핏이 라우에게 다가가서 말걸기
                }

                // 스핏과 라우 대화가 끝나면 실행
                if (storyNum == "1_2_3_7" && canvasControl.dialogueCnt >= canvasControl.dialogueLen && canvasControl.isPossibleCnvs)
                {
                    SpeatInteraction(2); // 스핏의 두번째 인터랙션 플레이 - 스핏이 라우 끌고 가기
                }


            }

        }

    }


    private void SpeatInteraction(int index)
    {

        // 스핏 인터랙션1 - 라우의 독백이 끝나면, 스핏이 라우를 향해 다가간 후 말을 건다.
        if (index == 1)
        {
            // 플레이어가 라우 못 움직이게 하기
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);

            // target 설정 - 스핏이 움직이지 않을 때 타겟 설정
            if (!isSpeatMoving)
            {
                isSpeatMoving = true;
                target.x = speatPoints[nextPassPointIndex].transform.position.x;
                target.y = speatPrefab.transform.position.y;
                target.z = speatPoints[nextPassPointIndex].transform.position.z;

                SetNPCCharacterManager_Dir(speatCharacterManager, speatAnimator, speatPoints[nextPassPointIndex].position, 1.5f);

                nextPassPointIndex++;
            }
            // target으로 이동
            else
            {
                speatPrefab.transform.position = Vector3.MoveTowards(speatPrefab.transform.position, target, 0.01f * speatSpeed);
            }

            // 스핏이 target으로 이동 완료시
            if (speatPrefab.transform.position == target)
            {
                if (nextPassPointIndex == speatPoints.Count) // 마지막 speatpoint도달
                {
                    // 대화 시도
                    isStartConversation_SpeatRau = true; // 라우 독백 끝나면 스핏-라우 대화 시작!

                    canvasControl.isPossibleCnvs = false;
                    DataController.instance_DataController.LoadData(speatPrefab.name, "speat-rau.json");
                    CanvasControl.instance_CanvasControl.StartConversation();
                    print("대화시작!");

                }
                else
                {
                    isSpeatMoving = false;
                }

            }

            // 나가는 방향으로 회전
            Vector3 targetDir = target - speatCharacterManager.transform.position;
            float step = speatSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(speatCharacterManager.transform.forward, targetDir, step, 0.0f);
            speatCharacterManager.transform.rotation = Quaternion.LookRotation(newDir);


        }

        // 스핏 인터랙션2 - 라우의 손을 잡아끌고 씬 밖으로 이동. 시장 뒷골목으로 이동!
        else if (index == 2)
        {
            // 타겟을 현재 맵의 클리어 박스로 설정
            target.x = clearBox.position.x;
            target.y = speatPrefab.transform.position.y;
            target.z = clearBox.position.z;

            // 스핏, 라우 클리어박스쪽으로 이동!
            wall.enabled = false; // 벽없애서 라우랑 스핏이 클리어 박스에 닿을 수 있게 하기!
            SetNPCCharacterManager_Dir(speatCharacterManager, speatCharacterManager.GetComponent<Animator>(), target, 1.5f);
            speatPrefab.transform.position = Vector3.MoveTowards(speatPrefab.transform.position, target, 0.01f * speatSpeed); // 스핏 움직이기
            // 스핏 회전
            Vector3 targetDir = target - speatCharacterManager.transform.position;
            float step = speatSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(speatCharacterManager.transform.forward, targetDir, step, 0.0f);
            speatCharacterManager.transform.rotation = Quaternion.LookRotation(newDir);


            Vector3 tempDir = (target - speatPrefab.transform.position).normalized;
            tempDir = new Vector3(tempDir.x, 0, tempDir.z);
            SetNPCCharacterManager_Dir(character, character.GetComponent<Animator>(), target, 1.5f);
            DataController.instance_DataController.currentChar.ctrl.Move(tempDir * Time.deltaTime * 0.01f * speatSpeed); // 라우 움직이기
            // 라우 회전
            Vector3 targetDir_ = target - character.transform.position;
            float step_ = speatSpeed * Time.deltaTime;
            Vector3 newDir_ = Vector3.RotateTowards(character.transform.forward, targetDir_, step_, 0.0f);
            character.transform.rotation = Quaternion.LookRotation(newDir_);


            if (tempDir == Vector3.zero)
            { // 라우 이동 끝.
                DataController.instance_DataController.joyStick.gameObject.SetActive(true); // 조이스틱 보이게
                character.moveHorDir = Vector3.zero;
                DataController.instance_DataController.cam.GetComponent<Camera_Moving>().enabled = true;
                clearBox.GetComponent<CheckMapClear>().isClear = true;
            }


        }

    }


    // 이거 고민하기
    void MoveNPCAuto(ref CharacterManager cm, ref List<Transform> points, ref int nextPassPointIndex, ref Vector3 target, ref Vector3 npcMoveDir, float speed, ref bool isFinishMoving_absolutely, ref bool isMoving)
    { // update문에 넣기..!

        if (!cm.isControlled) cm.isControlled = true;


        if (!isMoving)
        {
            isMoving = true;
            target.x = points[nextPassPointIndex].transform.position.x;
            target.y = cm.gameObject.transform.position.y; // 이게 문제..? 혹쉬?
            target.z = points[nextPassPointIndex].transform.position.z;

            npcMoveDir = points[nextPassPointIndex].position - cm.transform.position;
            npcMoveDir = Vector3.Normalize(npcMoveDir);
            cm.moveHorDir = new Vector3(npcMoveDir.x * speed, 0, npcMoveDir.z * speed);
            //points[nextPassPointIndex].transform.position = new Vector3(points[nextPassPointIndex].transform.position.x, cm.transform.position.y, points[nextPassPointIndex].transform.position.z);

            nextPassPointIndex++;
        }
        else if (isMoving)
        {
            if (cm.transform.position == target)
            {
                print("포인트에 도착!");
                if (nextPassPointIndex != points.Count)
                {
                    isMoving = false;
                }
                else
                {
                    // 이동 끝내기
                    isFinishMoving_absolutely = true;
                    //nextPassPointIndex++;

                }
            }

        }
    }

    void SetNPCCharacterManager_Dir(CharacterManager cm, Animator aim, Vector3 point, float speed)
    { // 점프는 일단 손 안댐...

        if (!cm.isControlled) cm.isControlled = true;

        Vector3 dir = point - cm.transform.position; //ok
        dir = Vector3.Normalize(dir); // 유닛벡터
        cm.moveHorDir = new Vector3(dir.x * speed, 0, dir.z * speed);

    }

}

