using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamburgerStoreManager : MonoBehaviour
{
    int day_info; // 1이면 day1, 2이면 day2, 3이면 day3임.

    // Start is called before the first frame update
    void Start()
    {

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

        // 대사 로드 ????? 왜 안될까요~^^~^;
        DataController.instance_DataController.LoadData("HamburgerStore_day" + day_info.ToString(), "EnrtyHamburgerStore.json"); // 햄버거 가게 들어와서 첫 대화
        CanvasControl.instance_CanvasControl.StartConversation();

    }

    // Update is called once per frame
    void Update()
    {
        print("현재: " + DataController.instance_DataController.charData.story + "_"  + DataController.instance_DataController.charData.storyBranch + "_" + DataController.instance_DataController.charData.storyBranch_scnd + "_"  + DataController.instance_DataController.charData.dialogue_index + ".json");
    }
}
