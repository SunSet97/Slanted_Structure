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

    public PhoneDialogue(string name, string contexts)
    {
        this.name = name;
        this.contexts = contexts;

    }

}


[System.Serializable]
public class PhoneDictionary
{
    public int key;
    public List<PhoneDialogue> dialgueList;

    public PhoneDictionary(int key, List<PhoneDialogue> dialgueList)
    {
        this.key = key;
        this.dialgueList = dialgueList;
    }

}


public class Phone : MonoBehaviour
{
    [Header("전화 걸려옴")]
    public int vibrationTime = 5; // 진동 횟수
    float force; // 진동 세기
    bool isVibration = false; // 진동때문에 선언한 변수
    public int maxNumberOfRejection = 4; // 최대 거절 가능 횟수. 
    int rejectionTime = 0; // 지금까지 거절한 횟수
    // 디버깅
    public bool makePhoneCall = false; // 이거 클릭하면 전화 걸림


    [Header("카메라")]
    public Camera cam;
    // parent들(자식들 인덱스로 불러올라고!) - 변수 이름은 오브젝트이름 + _parent 임.
    public GameObject Phonecreen_mainPanel_parent;
    public GameObject addressBookList_parent;
    public GameObject PhoneScreen_profilePanels_parent;
    public GameObject PhoneScreen;
    public GameObject incomingCallScreen;


    [Header("전화 대화창 관련")]
    public GameObject PhoneScreen_callPanel;
    public Text nameText_call;
    public Text dialogueText_call;


    [Header("채팅 대화창 관련")]
    public GameObject PhoneScreen_chattingPanels;
    public GameObject ChattingText;
    public Text nameText_chatting;
    Text dialogueText_chatting;

    int currentClickedInfo; // 0: 엄마, 1: 아빠, 2: 스핏, 3:오운

    int[] chattingBoxIndex = { 0, 0, 0, 0 }; // 00: 엄마와채팅, 엄마 채팅박스1 / 01: 엄마와 채팅, 라우 채팅박스1 / 02: 엄마와채팅, 엄마 채팅박스2 / 03: 엄마와 채팅, 라우 채팅박스2 ....

    // 핸드폰 아이콘 클릭
    bool isClickPhoneIcon = false;

    // 상태 나타내는 bool타입 변수들
    bool isCalling = false; // 전화중일 때 true
    bool isChatting = false;
    bool isMissedCall = false;
    bool isUsingPhone = false;


    [Header("로드한 데이터들")]
    public List<PhoneDictionary> callDataDic = new List<PhoneDictionary>();
    public List<PhoneDictionary> chattingDataDic = new List<PhoneDictionary>();


    [Header("디버깅용")]
    public int[] OriginEventIDs; // 원래 이벤트 아이디 => 각 모든 이벤트에 아이디 있다고 가정
    public int nowEventID;
    public int[] callEventIDs;
    public int[] chattingEventIDs;
    public bool nextChattingTrigger = false; // 디버깅용으로 다음 채팅 띄우기
    int callDataDic_index = 0;
    int callDataDic_DialogueListIndex = 0;
    int callEventID_index = 0;
    int chattingDataDic_index = 0;
    int chattingDataDic_DialogueListIndex = 0;
    int chattingEventID_index = 0;


    // 전화 실패
    List<PhoneDictionary> missedCallDicList;

    // Start is called before the first frame update
    void Start()
    {
        // 데이터 파싱
        ParsingData("PhoneData/callDataCSV", callDataDic); // 전화내용 데이터 파싱
        ParsingData("PhoneData/chattingDataCSV", chattingDataDic); // 채팅(문자)내용 데이터 파싱
        PhoneDialogue missedCall = new PhoneDialogue("음성", "연결이 되지 않아 삐소리후 소리샘으로 연결되오며 통화료가 부과됩니다. 삐---");
        List<PhoneDialogue> missedCallDialogueList = new List<PhoneDialogue>();
        missedCallDialogueList.Add(missedCall);
        PhoneDictionary phoneDic = new PhoneDictionary(-1, missedCallDialogueList);
        missedCallDicList = new List<PhoneDictionary>();
        missedCallDicList.Add(phoneDic);

    }

    // Update is called once per frame
    void Update()
    {
        // nowEventID가 변하는지 계속 확인


        if (Input.GetMouseButtonDown(0) && isCalling)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (isMissedCall)
                {
                    isCalling = false;
                    PhoneScreen_callPanel.SetActive(false);
                }
                else
                {
                    UpdateDialgue(callDataDic, callDataDic_index, callDataDic_DialogueListIndex++, nameText_call, dialogueText_call);

                }
            }
        }

        if (PhoneScreen.activeSelf)
        {
            isUsingPhone = true;
        }
        else
        {
            isUsingPhone = false;
        }

        // 디버깅용: 전화 걸기
        if (makePhoneCall && !isUsingPhone)
        {
            makePhoneCall = false;
            MakeACall();
        }
        else if (makePhoneCall && isUsingPhone) {
            makePhoneCall = false;
        }


        // 전화 걸려올 때 진동 구현
        if (isVibration)
        {

            Vector3 newPos = Random.insideUnitSphere * (vibrationTime);
            incomingCallScreen.transform.position = new Vector3(newPos.x + incomingCallScreen.transform.position.x, incomingCallScreen.transform.position.y, incomingCallScreen.transform.position.z);

        }



        // 디버깅용: 다음 채팅 띄우기
        if (nextChattingTrigger && isChatting && !isCalling)
        {

            nextChattingTrigger = false;
            OnDialogueWindow_chatting(checkRightTiming_chatting());

        }



    }


    // 전화 걸기
    void MakeACall()
    {

        incomingCallScreen.SetActive(true); // 수신스크린 보이게 하기.
        StartCoroutine(PhoneVibration(2.5f)); // 5초동안 진동 후

    }




    void ParsingData(string fileRoute, List<PhoneDictionary> dic) // 원래는 Dictionary<int, List<PhoneDialogue>>
    { // fileRoute: 파싱할 cvs확장자 파일 경로 / dic: 파싱한 데이터를 어떤 dictionary 변수에 저장할 것인지.

        List<PhoneDialogue> DialougeList = new List<PhoneDialogue>();

        TextAsset csvData = Resources.Load(fileRoute) as TextAsset; // TextAsset은 csv 파일을 받기 위한 타입. csv 파일을 로드 해옴 =>  c++의 template처럼 <TextAsset> 하면 오류남... 소괄호 써서 (TextAsset) 이렇게 소괄호써서 타입캐스팅하면 오류남... 이유는 모름!^&^ 반드시 as TextAsset으로 해주기! 그러면 오류 안나여

        string[] rowData = csvData.text.Split(new char[] { '\n' }); // 엔터를 기준으로 파일을 나눔 (이 배열의 각 인덱스에 행단위로 데이터 집어넣은다고 생각. => 콤마까지 포함되어있어)

        string currentEventIDstr = null;

        // 대사 종류 개수 알아내기(== 서로다른 eventID 개수 == key 개수)
        int dialogueNumOfDialogue = CalculateNumOfDialogue(rowData);
        PhoneDialogue phoneDialogueObj; //aa

        for (int i = 1; i < rowData.Length - 1; i++) // rowData에 csv파일의 맨 마지막 아래 빈칸도 들어가있어서 rowData.Length - 1인거!
        {

            string[] colData = rowData[i].Split(new char[] { ',' }); // 각 행마다 콤마 단위로 짤려서 배열에 넣어짐.

            if (colData[0].ToString() != "" && currentEventIDstr == null) // 맨 첫번째로 eventID가 적힌 칸
            {
                currentEventIDstr = colData[0];
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]); // PhoneDialogue 클래스 대사 생성 
                DialougeList.Add(phoneDialogueObj);

            }
            else if ((colData[0].ToString() != "" && currentEventIDstr != null) || i == rowData.Length - 2) // 다른 종류의 대사를 처음으로 만날 때!
            {
                PhoneDictionary tempPhoneDictionary = new PhoneDictionary(int.Parse(currentEventIDstr), DialougeList);
                dic.Add(tempPhoneDictionary);
                currentEventIDstr = colData[0]; // crrentEventDstr 업데이트
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]);

                DialougeList = new List<PhoneDialogue>(); // callDialougeList를 새롭게 만들어서
                DialougeList.Add(phoneDialogueObj); // 새로운 리스트에 다시 채워 넣기~

            }
            else if (colData[0].ToString() == "")
            {
                colData[0] = currentEventIDstr;
                phoneDialogueObj = new PhoneDialogue(colData[1], colData[2]); //aa
                DialougeList.Add(phoneDialogueObj);

            }

        }

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

    void UpdateDialgue(List<PhoneDictionary> dic, int dic_index, int dic_DialogueListIndex, Text speakerName, Text dialogueContents)
    {

        // 전화 다이얼로그 업데이트
        if (isCalling && !isChatting)
        {
            if (dic_DialogueListIndex >= dic[dic_index].dialgueList.Count) // 마지막 대화
            {
                // 대화창 끄기
                OffDialogueWindow_call();
            }
            else
            {
                speakerName.text = dic[dic_index].dialgueList[dic_DialogueListIndex].name;
                dialogueContents.text = dic[dic_index].dialgueList[dic_DialogueListIndex].contexts;
            }

        }
        // 채팅 다이얼로그 업데이트
        else if (!isCalling && isChatting)
        {
            if (dic_DialogueListIndex < dic[dic_index].dialgueList.Count)
            {

                dialogueContents.text = dic[dic_index].dialgueList[dic_DialogueListIndex].contexts;

            }

        }


    }

    void OnDialogueWindow_call(bool isTiming)
    {
        PhoneScreen_callPanel.SetActive(true);
        isCalling = true;
        if (isTiming)
        {
            UpdateDialgue(callDataDic, callDataDic_index, callDataDic_DialogueListIndex, nameText_call, dialogueText_call);

        }
        else if (!isTiming)
        {
            isMissedCall = true;
            UpdateDialgue(missedCallDicList, 0, 0, nameText_call, dialogueText_call);

        }

    }

    void OffDialogueWindow_call()
    {
        isCalling = false;
        PhoneScreen_callPanel.SetActive(false);
        callDataDic_index++;
        callDataDic_DialogueListIndex = 0;
    }

    void OnDialogueWindow_chatting(bool isTiming)
    {
        // 채팅 화면 띄우기
        isChatting = true;
        PhoneScreen_chattingPanels.transform.GetChild(currentClickedInfo).gameObject.SetActive(true);
        ChattingText.transform.GetChild(currentClickedInfo).gameObject.SetActive(true);

        if (isTiming)
        {

            // 0 -> 1 -> 2 -> 3 -> 다 지우고 다시 0 -> 1 -> 2 -> 3 -> ... 반복
            if (chattingBoxIndex[currentClickedInfo] == 4)
            {
                // 다 setActive(false)
                for (int i = 0; i < chattingBoxIndex[currentClickedInfo]; i++)
                {
                    PhoneScreen_chattingPanels.transform.GetChild(currentClickedInfo).GetChild(i).gameObject.SetActive(false);
                    ChattingText.transform.GetChild(currentClickedInfo).GetChild(i).gameObject.SetActive(false);
                }
                chattingBoxIndex[currentClickedInfo] = 0;
            }

            dialogueText_chatting = ChattingText.transform.GetChild(currentClickedInfo).GetChild(chattingBoxIndex[currentClickedInfo]).GetComponent<Text>();
            dialogueText_chatting.gameObject.SetActive(true);
            UpdateDialgue(chattingDataDic, chattingDataDic_index, chattingDataDic_DialogueListIndex++, nameText_chatting, dialogueText_chatting);
            GameObject chattingBox = PhoneScreen_chattingPanels.transform.GetChild(currentClickedInfo).GetChild(chattingBoxIndex[currentClickedInfo]).gameObject;
            chattingBox.SetActive(true);
            chattingBoxIndex[currentClickedInfo]++;

        }

    }

    public void OffDialogueWindow_chatting() // 채팅화면에서 X눌렀을 때! 호출
    {
        PhoneScreen_chattingPanels.transform.GetChild(currentClickedInfo).gameObject.SetActive(false);
        ChattingText.transform.GetChild(currentClickedInfo).gameObject.SetActive(false);
        isChatting = false;

    }



    // 핸드폰 전체 스크린 on, off 관련 
    void OnPhoneScreen() // 메인화면 키기
    {
        PhoneScreen.SetActive(true);
    }

    void OffPhoneScreen()  // 메인화면 끄기
    {
        PhoneScreen.SetActive(false);
    }

    // 핸드폰 메인화면(연락처 화면) on, off
    void OnMainScreen()
    {
        Phonecreen_mainPanel_parent.SetActive(true);
    }

    void OffMainScreen()
    {
        Phonecreen_mainPanel_parent.SetActive(false);
    }

    // 전화 대화 다이얼로그 띄우기 관련
    void OnCallDialoguePanel()
    {

    }

    void OffCallDialoguePanel()
    {


    }

    // 프로필 띄우기
    void OnProfileScreen(int addressIndex)
    {
        PhoneScreen_profilePanels_parent.SetActive(true);
        PhoneScreen_profilePanels_parent.transform.GetChild(addressIndex).gameObject.SetActive(true);

    }

    // 프로필 끄기
    void OffProfileScreen(int addressIndex)
    {
        PhoneScreen_profilePanels_parent.SetActive(false);
        PhoneScreen_profilePanels_parent.transform.GetChild(addressIndex).gameObject.SetActive(false);

    }


    // 타이밍 맞는지 확인하기
    bool checkRightTiming_call()
    {
        if (callDataDic[callDataDic_index].key == nowEventID) return true;
        else return false;

    }

    bool checkRightTiming_chatting()
    {
        if (chattingDataDic[chattingDataDic_index].key == nowEventID) return true;
        else return false;
    }

    // ########################### 버튼 클릭 함수


    public void ClickPhoneIcon()
    {

        if (!isClickPhoneIcon) // 핸드폰 메인화면이 꺼져 있는 상태에서 클릭
        {
            isClickPhoneIcon = true;
            OnPhoneScreen();
        }
        else if (isClickPhoneIcon)
        {
            isClickPhoneIcon = false;
            OffPhoneScreen();
        }

    }

    // 주소록 중 연락을 취할 사람 선택 - 버튼

    public void ClickAdressBookList()
    {

        // 클릭된 주소록 오브젝트 받아오기
        GameObject ClickedBtnObj = EventSystem.current.currentSelectedGameObject;

        for (int i = 0; i < addressBookList_parent.transform.childCount; i++)
        {
            if (addressBookList_parent.transform.GetChild(i).gameObject.Equals(ClickedBtnObj))
            {   // i가 인덱스인거임!
                // 프로필 띄우기
                currentClickedInfo = i; // 누구 클릭했는지 업뎃
                OnProfileScreen(currentClickedInfo); // 인덱스 던져주고 해당 연락처 프로필(전화하기, 채팅하기 버튼) 띄우기
                OffMainScreen(); // 메인 스크린(연락처 스크린) 끄기
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
        OnDialogueWindow_chatting(checkRightTiming_chatting());

    }

    // 프로필 화면에서 전화하기 눌렀을 때
    public void ClickCall()
    {
        OnDialogueWindow_call(checkRightTiming_call());

    }

    // 프로필 화면에서 x클릭
    public void ClickX()
    {
        OffProfileScreen(currentClickedInfo); // 프로필 화면 끄기 //// @ test!! 원래 i였어!
        OnMainScreen();

    }

    public void SwipeRejectionBtn()
    {
        print("전화 거절");

        if (rejectionTime > maxNumberOfRejection)
        {
            Debug.Log("총 " + maxNumberOfRejection + "이상 전화를 거절했으니, 인성 문제있는 엔딩!!");
            rejectionTime = 0;
        }

        rejectionTime++;

        incomingCallScreen.SetActive(false); // 수신스크린 안보이게 하기.
    }

    public void SwipeReceptionBtn()
    {
        print("전화 받음... 대사창 띄우기");
        incomingCallScreen.SetActive(false); // 수신스크린 안보이게 하기.
        OnPhoneScreen();
        OnDialogueWindow_call(checkRightTiming_call()); // 대사창 띄우기
    }

    IEnumerator PhoneVibration(float waitTime)
    {

        Vector3 originPos = incomingCallScreen.transform.position;

        if (!isVibration) isVibration = true;

        yield return new WaitForSeconds(waitTime);

        isVibration = false;

        incomingCallScreen.transform.position = originPos;


    }

}
