using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 클릭한 버튼 정보 받아오려고 using함.


[System.Serializable]
public class PhoneDialogue
{
    public string name; // 말하는 사람, 문자 친 사람 이름
    public string contexts; // 대사 내용

    // 생성자
    public PhoneDialogue(string name, string contexts)
    {
        this.name = name;
        this.contexts = contexts;
    }
    
}

public class Phone : MonoBehaviour
{
    public Text nameText_call;
    public Text dialogueText_call;





    // 스크린 이미지들
    public Image mainScreen;

    // parent들(자식들 인덱스로 불러올라고!) - 변수 이름은 오브젝트이름 + _parent 임.
    public GameObject AddressBookList_parent;
    public GameObject PhoneScreen_profilePanels_parent;




    // 핸드폰 아이콘 클릭
    bool isClickPhoneIcon = false;



    // 필요한 스크린: 메인화면, 친구들 프로필 화면(전화걸기, 문자걸기 버튼 보이는!), 전화 발신 창, 전화 수신 창

    //public Text nameText_chatting; 일단 보류
    //public Text dialogueText_chatting; 일단 보류

    // 데이터 담는 Dictionary
    Dictionary<int, List<PhoneDialogue>> callDataDic = new Dictionary<int, List<PhoneDialogue>>(); // key는 정상적인 call이 불려야 하는 이벤트id. 만약 불려야할 타이밍이 아니라면 음성사서함으로 연결되며~ 다이얼로그 보이기
    Dictionary<int, List<PhoneDialogue>> chattingDataDic = new Dictionary<int, List<PhoneDialogue>>(); // 전화와 마찬가지
        
    public int[] OriginEventIDs; // 원래 이벤트 아이디 => 각 모든 이벤트에 아이디 있다고 가정
    // 디버깅용
    public int nowEventID = 0;


    // Start is called before the first frame update
    void Start()
    {
        // 데이터 파싱
        ParsingData("PhoneData/callDataCSV", callDataDic); // 전화내용 데이터 파싱
        ParsingData("PhoneData/chattingDataCSV", chattingDataDic); // 채팅(문자)내용 데이터 파싱
        
    }

    // Update is called once per frame
    void Update()
    {
        // nowEventID가 변하는지 계속 확인
    }

    // 전화할 타이밍이 아닐 때 전화하면 실행됨.
    void IsNotTImeToCall() {
        print("삐~ 소리이후 소리사서함으로 연결되며 통화료과 부과될 수 있습니다.");
    }


    void ParsingData(string fileRoute, Dictionary<int, List<PhoneDialogue>> dic)
    { // fileRoute: 파싱할 cvs확장자 파일 경로 / dic: 파싱한 데이터를 어떤 dictionary 변수에 저장할 것인지.

        List<PhoneDialogue> DialougeList = new List<PhoneDialogue>();

        TextAsset csvData = Resources.Load(fileRoute) as TextAsset; // TextAsset은 csv 파일을 받기 위한 타입. csv 파일을 로드 해옴 =>  c++의 template처럼 <TextAsset> 하면 오류남... 소괄호 써서 (TextAsset) 이렇게 소괄호써서 타입캐스팅하면 오류남... 이유는 모름!^&^ 반드시 as TextAsset으로 해주기! 그러면 오류 안나여

        string[] rowData = csvData.text.Split(new char[] { '\n' }); // 엔터를 기준으로 파일을 나눔 (이 배열의 각 인덱스에 행단위로 데이터 집어넣은다고 생각. => 콤마까지 포함되어있어)

        //PhoneDialogue[] arr = new PhoneDialogue[rowData.Length - 2]; // rowData에 csv파일의 맨 마지막 아래 칸도 들어가있어서 -2 해준 것!
        
        string currentEventIDstr = null;

        // 대사 종류 개수 알아내기(== 서로다른 eventID 개수 == key 개수)
        int dialogueNumOfDialogue = CalculateNumOfDialogue(rowData);
        PhoneDialogue phoneDialogueObj; //aa

        int k = 0; // 지금까지 몇개의 key가 dic에 들어갔는지.

        for (int i = 1; i < rowData.Length - 1  ; i++) // rowData에 csv파일의 맨 마지막 아래 빈칸도 들어가있어서 rowData.Length - 1인거!
        {

            string[] colData = rowData[i].Split(new char[] { ',' }); // 각 행마다 콤마 단위로 짤려서 배열에 넣어짐.

            if (colData[0].ToString() != "" && currentEventIDstr == null) // 맨 첫번째로 eventID가 적힌 칸
            {
                currentEventIDstr = colData[0];
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]); //aa
                DialougeList.Add(phoneDialogueObj);

            }
            else if ((colData[0].ToString() != "" && currentEventIDstr != null) || i == rowData.Length - 2) // 다른 종류 대사 넣기
            {
                dic.Add(int.Parse(currentEventIDstr), DialougeList); // Dictionary 자료구조에 넣기.
                k++;
                currentEventIDstr = colData[0]; // crrentEventDstr 업데이트
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]); //aa

                DialougeList = new List<PhoneDialogue>(); // callDialougeList를 새롭게 만들어서
                DialougeList.Add(phoneDialogueObj); // 새로운 리스트에 다시 채워 넣기~

            }
            else if (colData[0].ToString() == "")
            {
                colData[0] = currentEventIDstr;
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]); //aa
                DialougeList.Add(phoneDialogueObj);
                
            }


            /*
            print("colData[0] = " + colData[0]); // eventID 들어감
            print("colData[1] = " + colData[1]); // 이름 들어감
            print("colData[2] = " + colData[2]); // 내용 들어감
            print("*************************************");
            */


        }

        // 참조하던 phoneDialgoeuObj가 사라져버려ㅜ
        //print("callD" + callDialougeList);
        /*for (int i = 0; i < callDialougeList.Count; i++)
        {
            print("후callDialougeList[" + i + "] = " + callDialougeList[i].name + " " + callDialougeList[i].contexts);
        }
        */






        // return arr;

        /*
        print("@@@@" + a.Length);
        for (int i = 0; i < a.Length; i++) {
            print("a[" + i + "] =  name: " + a[i].name + "/ context: " + a[i].contexts);
        }*/


        // 갹 print("길이" + PhoneDialougeList.Count);


        /*
        for (int i = 0; i < PhoneDialougeList.Count; i++) {
            print("PhoneDialougeList[" + i + "] = " + PhoneDialougeList[i].name);
        }*/


        // rowData의 각각의 인덱스에 저장된 행데이터를 각각의 열로 저장시키기.
        // rowData의 의미있는 데이터는 rowData[1]부터! rowData[0]은 attribute 이름임. 그래서 i가 1부터 시작
        //int attributeNum = 3; // attribute 개수~! 지금은 3개. ★ 바뀌면 수정하기
        //int attributeIndex;



        /*
    for (int i = 1; i < rowData.Length ; i++)
    {

        // 각 행의 이름, 내용을 하나의 객체라보기


        // 우선 이름과 대사 내용을 리스트에 담기
        for (attributeIndex = 1; attributeIndex < attributeNum; attributeNum++)
        {
            if (attributeIndex == 1) // attribute: 이름
            {
                PhoneDialougeList.Add
            }
            else if (attributeIndex == 2) // attribute: 내용
            {

            }
        }


        // 첫번째 attribute에 해당하는 데이터 값은 int로 변환해서 Dictionary 자료구조의 key부분에 ㄱㄱ
    }
    */





    }

    // 파싱한 데이터를 dictionary 자료구조에 저장하기
    int CalculateNumOfDialogue(string[] rowData)
    {
        int dialogueNumOfDialogue = 0;
        string[] colData;
        string currentEventIDstr = null;

        for (int i = 1; i < rowData.Length; i++)
        {
            colData = rowData[i].Split(new char[] { ',' });

            if (colData[0].ToString() != "") // eventID가 적힌 칸
            {
                dialogueNumOfDialogue++;
                currentEventIDstr = colData[0];
            }
            else if (colData[0].ToString() == "")
            {
                colData[0] = currentEventIDstr;
            }

        }

        return dialogueNumOfDialogue;

    }

    // 핸드폰 스크린 on, off 관련 
    void OnPhoneMainScreen() // 메인화면 띄우기
    {


    }

    void OffPhoneMainScreen()  // 메인화면 끄기
    { 


    }

    // 전화 대화 다이얼로그 띄우기 관련
    void OnCallDialoguePanel()
    {

    }

    void OffCallDialoguePanel ()
    {


    }

    // 프로필 띄우기
    void OnProfileScreen(int addressIndex)
    {

        PhoneScreen_profilePanels_parent.transform.GetChild(addressIndex).gameObject.SetActive(true);

    }

    // 프로필 끄기
    void OffProfileScreen(int addressIndex)
    {

        PhoneScreen_profilePanels_parent.transform.GetChild(addressIndex).gameObject.SetActive(false);

    }


    // 타이밍 맞는지 확인하기
    void checkCallTiming()
    {

    }

    void checkChattingTime()
    {

    }

    // ########################### 버튼 클릭 함수

    
    public void ClickPhoneIcon() {

        if (!isClickPhoneIcon) // 핸드폰 메인화면이 꺼져 있는 상태에서 클릭
        {
            isClickPhoneIcon = true;
            OnPhoneMainScreen();
        }
        else if (isClickPhoneIcon)
        {
            isClickPhoneIcon = false;
            OffPhoneMainScreen();
        }
               
    }

    // 주소록 중 연락 취할 사람 선택 - 버튼
    
    public void ClickAdressBookList() {

        // 클릭된 버튼 오브젝트 받아오기
        GameObject ClickedBtnObj = EventSystem.current.currentSelectedGameObject;

        for (int i = 0 ; i < AddressBookList_parent.transform.childCount; i++)
        {
            if (AddressBookList_parent.transform.GetChild(i).Equals(ClickedBtnObj))
            {   // i가 인덱스인거임!
                // 프로필 띄우기
                OnProfileScreen(i); // 인덱스 던져줌.
                break;
            }
            else
            {
                continue;
            }
        }

    }

    // 프로필화면에서 채팅하기 눌렀을 때

    public void ClickChatting()
    {


    }

    // 프로필 화면에서 전화하기 눌렀을 때
    public void ClickCall()
    {

    }


}

