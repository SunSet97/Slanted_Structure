using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDialogueData : MonoBehaviour
{
    public Object[] jsonFiles;
    private int dialogueCount = 0;  //다음 대화로 넘어갈 경우 ++
    // 한 캐릭터에 들어가는 대화가 여러개인 경우 json파일 Dialogue0, Dialogue1, Dialogue2 이런식으로 이름 지어주기


    // LoadAll vs 계속 불러오기
    //대화를 시작할때 렉이 걸리면 LoadAll로 처음부터
    UISU[] dialogues;
    void Awake()
    {
        //jsonFiles = Resources.LoadAll("Path");
        //jsonFile에서 "MapCode" + "CharacterName" + dialogueCount으로 찾기 그런데 아마 이름 순서대로 저장될거야 그러면 count만 봐도 됨

        //jsonFile or UISU[] = Mapcode + NPC name으로 가져오기 (맵마다 캐릭터는 하나니까)

        //dialogues = JsontoString.LoadJsonFromClassName<UISU>("MapCode" + "CharacterName" + dialogueCount);
    }
    private void Update()
    {
        //if(Count가 증가하거나 하면 )
        //dialogues = JsontoString.LoadJsonFromClassName<UISU>("MapCode" + "CharacterName" + dialogueCount);
        //Control.instance.dialogues = dialogues;
    }

}
