using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamburgerStoreManager : MonoBehaviour
{

    public TextAsset jsonFile;
    // 스핏
    public GameObject speatPrefab;
    CharacterManager speatCharacterManager;
    Animator speatAnimator;

    private NPCwaypoints wayPoints;


    public InteractionObj_stroke part;
    public InteractionObj_stroke sofa;
    public NPCwaypoints speat;
    CharacterManager currentCharacter;


    // 투명 벽
    public BoxCollider wall;

    void Start()
    {
        {

            InitialSetting();

        }

    }
    void Update()
    {
        if (!currentCharacter)
        {
            InitialSetting();
        }
    }

    #region 초기세팅
    void InitialSetting()
    {
        //wall.enabled = true;

        // 스핏 위치시키기
        //speatPrefab.transform.position = speatPointParent.transform.GetChild(0).transform.position;

        // 캐릭터 맵으로 이동시키기
        //DataController.instance_DataController.isMapChanged = true;

        // 스토리 정보 수정
        //DataController.instance_DataController.charData.story = 1;
        //DataController.instance_DataController.charData.storyBranch = 2;
        //DataController.instance_DataController.charData.storyBranch_scnd = 3;
        //DataController.instance_DataController.charData.dialogue_index = 4;

        // 캐릭터
        currentCharacter = DataController.instance_DataController.currentChar;

        if (!wayPoints) wayPoints = speatPrefab.GetComponent<NPCwaypoints>();

        // 클리어박스
        //if (!clearBox) clearBox = DataController.instance_DataController.currentMap.positionSets[0].clearBox;

        // 스핏
        if (!speatCharacterManager)
        {
            speatCharacterManager = speatPrefab.GetComponent<CharacterManager>();
            speatCharacterManager.isControlled = false;
        }

        // 스핏 애니메이터
        if (!speatAnimator) speatAnimator = speatPrefab.GetComponent<Animator>();

        //if (!canvasControl) canvasControl = CanvasControl.instance_CanvasControl;

        if (DataController.instance_DataController.mapCode == "201110")
        {
            //day_info = 1; // day1

            // 이해하기 본인도 힘듬
            // SetDialogueEndEvent는 터치로 대화를 시작할때 대화 시작을 임의로 조정할 수 없기 때문에 사용
            part.SetDialogueEndEvent(() => { sofa.enabled = true; });
            sofa.SetDialogueEndEvent(() => { SpeatInteraction(1); });
            sofa.SetDialogueStartEvent(() => { currentCharacter.PickUpCharacter(); currentCharacter.transform.position = sofa.transform.position; currentCharacter.transform.rotation = sofa.transform.rotation; });
            // 대화 시작이 "포인트에 도착했을 때"이기 때문에 바로 해줘도 됨
            wayPoints.SetPointEvent(() =>
            {
                CanvasControl.instance_CanvasControl.SetDialougueEndAction(() => { SpeatInteraction(2); });
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                //Json 파일
                CanvasControl.instance_CanvasControl.StartConversation(jsonFile.text);
            }, 3);
        }
        else if (DataController.instance_DataController.mapCode == "202210")
        {
            //day_info = 2; // day2
        }
        else if (DataController.instance_DataController.mapCode == "203110")
        {
            //day_info = 3; // day3
        }

    }
    #endregion

    #region 인터랙션
    private void SpeatInteraction(int index)
    {
        // 스핏 인터랙션1 - 라우의 독백이 끝나면, 스핏이 라우를 향해 다가간 후 말을 건다.
        if (index == 1)
        {
            // 플레이어가 라우 못 움직이게 하기
            DataController.instance_DataController.joyStick.gameObject.SetActive(false);

            // 웨이포인트에 따라 스핏 움직이게 하기.
            wayPoints.StartMoving();
        }

        // 스핏 인터랙션2 - 라우의 손을 잡아끌고 씬 밖으로 이동. 시장 뒷골목으로 이동!
        else if (index == 2)
        {
            wayPoints.StartMoving(); // 스핏 웨이포인트에 따라 다시 이동 ㄱㄱ

            StartCoroutine(StartRauMoving());
        }
    }
    #endregion

    private IEnumerator StartRauMoving()
    {
        currentCharacter.PickUpCharacter();
        int index = wayPoints.pointIndex;
        PointData desPoint = wayPoints.point[index];

        yield return new WaitForSeconds(2f);

        currentCharacter.anim.SetFloat("Speed", 1f);

        currentCharacter.transform.LookAt(desPoint.transform);

        while (index < wayPoints.point.Length || !wayPoints.point[index].isStop)
        {
            if (currentCharacter.transform.position == wayPoints.point[index].transform.position)
            {
                desPoint = wayPoints.point[++index];
                currentCharacter.transform.LookAt(desPoint.transform);
            }
            else
            {
                currentCharacter.transform.position = Vector3.MoveTowards(currentCharacter.transform.position, desPoint.transform.position, wayPoints.speed * Time.deltaTime * 0.95f);
            }
            yield return null;
        }
    }
}

