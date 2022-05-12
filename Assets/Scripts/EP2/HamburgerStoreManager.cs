using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Data.CustomEnum;

public class HamburgerStoreManager : MonoBehaviour
{

    public TextAsset jsonFile;
    // 스핏
    public GameObject speatPrefab;
    private CharacterManager speat;
    private NPCwaypoints _wayPoints;

    [Range(0, 1f)] public float rauAniSpeed;

    public InteractionObj_stroke partTimeJob;
    public InteractionObj_stroke sofa;
    private CharacterManager curChar;

    void Start()
    {
        InitialSetting();
    }

    #region 초기세팅
    void InitialSetting()
    {
        curChar = DataController.instance.GetCharacter(Character.Main);

        if (speatPrefab != null)
        {
            _wayPoints = speatPrefab.GetComponent<NPCwaypoints>();

            speat = speatPrefab.GetComponent<CharacterManager>();
            speat.IsMove = false;
        }
        else
        {
            Debug.LogError("오류 public 스핏 오브젝트 비어있음");
        }

        {
            // 이해하기 본인도 힘듬
            // SetDialogueEndEvent는 터치로 대화를 시작할때 대화 시작을 임의로 조정할 수 없기 때문에 사용
            partTimeJob.SetDialogueEndEvent(() => { sofa.enabled = true; });
            sofa.SetDialogueEndEvent(() => { SpeatInteraction(1); });
            sofa.SetDialogueStartEvent(() => { curChar.PickUpCharacter(); curChar.transform.position = sofa.transform.GetChild(0).position; curChar.transform.rotation = sofa.transform.GetChild(0).rotation; curChar.anim.SetBool("Seat", true); });
            // 대화 시작이 "포인트에 도착했을 때"이기 때문에 바로 해줘도 됨
            _wayPoints.SetPointEvent(() =>
            {
                CanvasControl.instance.SetDialougueEndAction(() => { SpeatInteraction(2); });
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
                CanvasControl.instance.StartConversation(jsonFile.text);
            }, 3);
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
            DataController.instance.InitializeJoyStick(false);

            // 웨이포인트에 따라 스핏 움직이게 하기.
            _wayPoints.StartMoving();
        }

        // 스핏 인터랙션2 - 라우의 손을 잡아끌고 씬 밖으로 이동. 시장 뒷골목으로 이동!
        else if (index == 2)
        {
            _wayPoints.StartMoving(); // 스핏 웨이포인트에 따라 다시 이동 ㄱㄱ

            StartCoroutine(StartRauMoving());
        }
    }
    #endregion

    private IEnumerator StartRauMoving()
    {
        curChar.PickUpCharacter();
        int index = _wayPoints.pointIndex;
        PointData desPoint = _wayPoints.point[index];

        curChar.anim.SetBool("Seat", false);
        yield return new WaitForSeconds(2f);
        curChar.anim.SetFloat("Speed", rauAniSpeed);

        curChar.transform.LookAt(desPoint.transform);

        while (index < _wayPoints.point.Length || !_wayPoints.point[index].isStop)
        {
            if (curChar.transform.position == _wayPoints.point[index].transform.position)
            {
                desPoint = _wayPoints.point[++index];
                curChar.transform.LookAt(desPoint.transform);
            }
            else
            {
                curChar.transform.position = Vector3.MoveTowards(curChar.transform.position, desPoint.transform.position, _wayPoints.speed * Time.deltaTime * 0.95f);
            }
            yield return null;
        }
    }
}

