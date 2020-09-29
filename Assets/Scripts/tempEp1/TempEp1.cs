using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEp1 : MonoBehaviour
{
    // 라우
    public CharacterManager rau;

    // 출입 관리 노인
    public GameObject oldManager;

    // 대화
    CanvasControl canvasCtrl;

    // 다음 활성화 되어야 하는 대화 파일 이름
    string beActiveDiologueFileName;

    // 해당 맵 처음 시작
    bool initialStart = false;

    // tempEp1Manager가 관리하는 Map 변경 관련
    int mapIndex = 0; // 현재 tempEp1Manager가 있는 Map 인덱스 ( 0: 101010 / 1: 101110 / 2: 102010 / 3: 103010 )
    bool mapChange = false; // Map 변경할 때마다 쓰임. true일 때, Map 변경할 타이밍! 주로 맵 시작하자마자 하는 독백 or 대화때문에 사용
    CheckMapClear checkMapClear = new CheckMapClear();
    Transform currMapCode;

    // (노인, tempEp1Manager를 맵 자식으로 넣기 위해서)
    public Transform ep1;
    public List<Transform> ep1Maps = new List<Transform>();


    // Start is called before the first frame update
    void Start()
    {
        // ep1Maps에 ep1 맵들 쫙 넣기
        for (int i = 0; i < ep1.childCount; i++)
        {
            ep1Maps.Add(ep1.GetChild(i));
        }

        mapChange = true;

        currMapCode = ep1Maps[1]; // currMapCode를 101010으로!
    }

    void Update()
    {

        // 대화 파일 정보
        beActiveDiologueFileName = DataController.instance_DataController.charData.story + "_" + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_" + DataController.instance_DataController.charData.dialogue_index;

        // 맵 변경하는거 체크
        CheckMapChange();

        // tempEp1Manager가 101010에 있을 때
        if (mapIndex == 0)
        {
            if (mapChange)
            {
                mapChange = false;

                DataController.instance_DataController.LoadData("tempOldEntranceManager", "Ep1start.json"); // LoadData(dataType(폴더명), fileName)
                CanvasControl.instance_CanvasControl.StartConversation(); // 대화 시작

                // 스토리 정보 변경
                DataController.instance_DataController.charData.story = 1;
                DataController.instance_DataController.charData.storyBranch = 1;
                DataController.instance_DataController.charData.storyBranch_scnd = 0;
                DataController.instance_DataController.charData.dialogue_index = 1;

            }


            /*else if (!mapChange && beActiveDiologueFileName == "1_1_0_4")
            {
                // 라우와 노인의 대화 && 치료 후 출입관리실로 이동해야할 때.
                oldManager.transform.position = rau.transform.position + new Vector3(0.5f, rau.ctrl.height, 0);

            }*/

        }


        //print(beActiveDiologueFileName);
    }

    // 맵 변경 체크하고, 맵 변경이 이있으면 tempEp1Manager랑 기타 오브젝들(예. 출입관리 노인 등) 해당 맵으로 이동
    void CheckMapChange()
    {
        // 현재 맵 코드
        string mapCode = DataController.instance_DataController.mapCode;
        
        // 맵변경 일어나면, 매니저 이동 ㄱㄱ
        if (DataController.instance_DataController.isMapChanged)
        {

            if (mapCode == "101010")
            {
                mapIndex = 0;
                currMapCode = ep1Maps[mapIndex];
                transform.SetParent(currMapCode.GetChild(0));

            }
            else if (mapCode == "101110")
            {
                mapIndex = 1;
                currMapCode = ep1Maps[mapIndex];
                transform.SetParent(currMapCode.GetChild(0));

            }
            else if (mapCode == "102010")
            {
                mapIndex = 2;
                currMapCode = ep1Maps[mapIndex];
                transform.SetParent(currMapCode.GetChild(0));

            }
            else if (mapCode == "103010")
            {
                mapIndex = 3;
                currMapCode = ep1Maps[mapIndex];
                transform.SetParent(currMapCode.GetChild(0));

            }

        }
        // 선택지에 의해 맵 변경된 경우
        else if (!checkMapClear.isClear && currMapCode.name != mapCode)
        {
            for (int i = 0; i < ep1Maps.Count; i++)
            {
                if (mapCode == ep1Maps[i].name)
                {
                    mapIndex = i;
                    currMapCode = ep1Maps[i];
                    break;
                }
            }

        }



    }

    /*
    void OnTriggerEnter(Collider other)
    {

        

        if (canvasCtrl.isPossibleCnvs)
        {
            DataController.instance_DataController.LoadData("tempOldManager","tempOld.json" ); // LoadData(dataType(폴더명), fileName)
            canvasCtrl.StartConversation(); // 대화 시작
            //choice.SetActive(true); // 대화 끝나면 선택창 뜸.
        }
        

    }*/
}
