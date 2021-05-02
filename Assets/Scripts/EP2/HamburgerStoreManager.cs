using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamburgerStoreManager : MonoBehaviour
{
    // day 정보
    int day_info; // 1이면 day1, 2이면 day2, 3이면 day3임.

    // 스핏
    public GameObject speatPrefab;
    CharacterManager speatCharacterManager;
    Animator speatAnimator;
    bool isFinishMoving_speat = false;
    bool isMoving_speat = false;
    Vector3 speatMoveDir;
    //public Transform target; // 가야할 포인트
    float speatRot;

    int speatMoveCnt = 0;
    bool triggerRotation = false;

    public NPCwaypoints npcWayPoints;

    // 라우
    CharacterManager character;
    public Event_IntoTheAzit eventBox; // 직접 어사인하기
    bool isCoort = false;
    bool triggerRauMoving = false;

    // 스핏이 지나갈 포인트
    public CanvasControl canvasControl;

    bool isSpeatMoving = false; // true면 스핏이 움직이고 있는 상태
    bool isStartConversation_SpeatRau = false; // 스핏-라우 대화 시작

    // 클리어박스
    public Transform clearBox;

    // 투명 벽
    public BoxCollider wall;

    public Vector2 tmp;

    // Start is called before the first frame update
    void Start()
    {
        if (DataController.instance_DataController.mapCode == "201110" || DataController.instance_DataController.mapCode == "202210"
            || DataController.instance_DataController.mapCode == "203110" || DataController.instance_DataController.mapCode == "205110")
        {

            InitialSetting();

        }

    }

    // Update is called once per frame
    void Update()
    {
        //print(DataController.instance_DataController.isMapChanged);
        if (DataController.instance_DataController.currentMap.mapCode == "201110" || DataController.instance_DataController.currentMap.mapCode== "202110"
            || DataController.instance_DataController.mapCode == "203110" || DataController.instance_DataController.mapCode == "205110")
        {
            string storyNum = DataController.instance_DataController.charData.story.ToString() + "_" + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd.ToString() + "_" + DataController.instance_DataController.charData.dialogue_index.ToString();

            if (!character)
            {
                InitialSetting();
            }

            // 라우 독백끝나면 실행
            if (storyNum == "1_2_3_6" && canvasControl.dialogueCnt >= canvasControl.dialogueLen && canvasControl.isPossibleCnvs && !isStartConversation_SpeatRau)
            {
                SpeatInteraction(1); // 스핏의 첫번째 인터랙션 플레이 - 스핏이 라우에게 다가가서 말걸기
            }

            // 스핏과 라우 대화가 끝나면 실행
            if (storyNum == "1_2_3_7" && canvasControl.dialogueCnt >= canvasControl.dialogueLen && canvasControl.isPossibleCnvs)
            {
                triggerRauMoving = true;
                SpeatInteraction(2); // 스핏의 두번째 인터랙션 플레이 - 스핏이 라우 끌고 가기
            }


        }

    }

    #region 초기세팅
    void InitialSetting()
    {
        wall.enabled = true;

        // 스핏 위치시키기
        //speatPrefab.transform.position = speatPointParent.transform.GetChild(0).transform.position;

        // 캐릭터 맵으로 이동시키기
        DataController.instance_DataController.isMapChanged = true;

        // 스토리 정보 수정
        DataController.instance_DataController.charData.story = 1;
        DataController.instance_DataController.charData.storyBranch = 2;
        DataController.instance_DataController.charData.storyBranch_scnd = 3;
        DataController.instance_DataController.charData.dialogue_index = 4;

        // 캐릭터
        if(!character) character = DataController.instance_DataController.currentChar;

        npcWayPoints = speatPrefab.GetComponent<NPCwaypoints>();

        // 클리어박스
        if (!clearBox) clearBox = DataController.instance_DataController.currentMap.positionSets[0].clearBox;

        // 스핏
        if (!speatCharacterManager)
        {
            speatCharacterManager = speatPrefab.GetComponent<CharacterManager>();
            speatCharacterManager.isControlled = false;
        }

        // 스핏 애니메이터
        if(!speatAnimator) speatAnimator = speatPrefab.GetComponent<Animator>();

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
    #endregion

    #region 인터랙션
    private void SpeatInteraction(int index)
    {


        // 스핏 인터랙션1 - 라우의 독백이 끝나면, 스핏이 라우를 향해 다가간 후 말을 건다.
        if (index == 1)
        {
            if (!npcWayPoints) npcWayPoints = speatPrefab.GetComponent<NPCwaypoints>();
            
            // 플레이어가 라우 못 움직이게 하기
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);

            // 웨이포인트에 따라 스핏 움직이게 하기.
            npcWayPoints.StartMoving();
            
            // 스핏이 라우쪽으로 이동완료
            if (npcWayPoints.pointIndex == 4)
            {
                // 대화 시도
                isStartConversation_SpeatRau = true; // 라우 독백 끝나면 스핏-라우 대화 시작!

                canvasControl.isPossibleCnvs = false;
                DataController.instance_DataController.LoadData(speatPrefab.name, "speat-rau.json");
                CanvasControl.instance_CanvasControl.StartConversation();
                print("대화시작!");

            }

        }

        // 스핏 인터랙션2 - 라우의 손을 잡아끌고 씬 밖으로 이동. 시장 뒷골목으로 이동!
        else if (index == 2)
        {

            if (wall.enabled) wall.enabled = false; // 벽없애서 라우랑 스핏이 클리어 박스에 닿을 수 있게 하기!

            npcWayPoints.StartMoving(); // 스핏 웨이포인트에 따라 다시 이동 ㄱㄱ

            // 라우도 스핏의 웨이포인트 마지막 지점으로 ㄱㄱ
            if (triggerRauMoving)
            {
                StartRauMoving();
            }
            else
            {
                StopRauMoving();
            }
        }
    }
    #endregion

   
    void OnEnable() {
        wall.enabled = true;
        InitialSetting();
    }

    void StartRauMoving() {
        character.PickUpCharacter();
        Transform tmpPoint = npcWayPoints.pointPos[npcWayPoints.pointIndex];
        character.transform.position = Vector3.MoveTowards(character.transform.position, tmpPoint.position, npcWayPoints.speed * Time.deltaTime * 0.95f);
        character.transform.LookAt(tmpPoint);
        character.anim.SetFloat("Speed", 1f);
    }

    void StopRauMoving()
    {
        character.PutDownCharacter();
        character.anim.SetFloat("Speed", 0f);
    }


    void OnTriggerEnter(Collider other) {

        if (other.name == character.name) {

            triggerRauMoving = false;
        }

    }



}

