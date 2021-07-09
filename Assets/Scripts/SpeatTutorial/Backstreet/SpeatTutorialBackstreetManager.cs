using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeatTutorialBackstreetManager : MonoBehaviour
{
    [Header("#UI")]
    public Slider speatSlider;
    public Text endText;
    public Slider pimpSlider;
    public Text distanceText;

    [Header("#Objects")]
    public Transform startPosition;
    public GameObject pimp;
    public Transform[] trailer;
    public Transform[] patternFolder;
    private List<List<GameObject>> patterns;
    private int[] patternIndex; //각 패턴의 개수

    [Header("#Buttons")]
    public Button jumpBtn;
    public Button abilityBtn;

    [Header("#Running")]
    public bool isRunning = false;
    private int clearCount;   //넘어간 패턴의 개수

    float percentage;
    private float speatDistance;
    float pimpDistance;

    void Start()
    {
        StartCoroutine("initPatterns");
    }
    IEnumerator initPatterns()
    {
        patterns = new List<List<GameObject>>();
        patternIndex = new int[3];
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        for (int i = 0; i < 3; i++)
        {
            List<GameObject> list = new List<GameObject>();
            GameObject[] gameObjects = Resources.LoadAll<GameObject>("Run_Pattern/Pattern" + i);
            patternIndex[i] = gameObjects.Length;
            foreach (GameObject temp in gameObjects)
            {
                temp.SetActive(false);
                list.Add(Instantiate(temp));
                list[list.Count - 1].transform.SetParent(patternFolder[i]);
                list[list.Count - 1].name = temp.name;
            }
            patterns.Add(list);
            yield return waitForSeconds;
        }
    }
    void Update()
    {
        percentage = 100 / speatSlider.maxValue;
        speatDistance = (speatSlider.maxValue - speatSlider.value) * percentage; // 종료 지점과 스핏의 거리
        pimpDistance = (speatSlider.value - pimpSlider.value) * percentage; // 스핏과 포주의 거리

        // 조이스틱 입력 온오프
        foreach (Image image in DataController.instance_DataController.joyStick.GetComponentsInChildren<Image>())
            image.color = image.name == "Transparent Dynamic Joystick" ? Color.clear : speatSlider.value < speatSlider.maxValue ? Color.clear : Color.white - Color.black * 0.3f;

        // 런게임 시작
        if (!isRunning) { isRunning = true; StartCoroutine("StartRungame"); }

        // 패턴 자동 설정 - Update마다 하는 건 비효율적, 사용할때마다 체크
        //patterns = Resources.LoadAll<GameObject>("Run_Pattern/Pattern" + (speatDistance > 66 ? 0 : speatDistance > 33 ? 1 : 2));

        // 일정 거리 이후에 포주 출현
        pimp.SetActive(speatSlider.value > speatSlider.maxValue * 0.1f);
        pimpSlider.fillRect.gameObject.SetActive(speatSlider.value > speatSlider.maxValue * 0.1f);
        pimpSlider.handleRect.gameObject.SetActive(speatSlider.value > speatSlider.maxValue * 0.1f);
        // 포주와 스핏의 거리 설정
        if (pimp.activeSelf) pimp.transform.localPosition = new Vector3(20 - pimpDistance * 2, pimp.transform.localPosition.y, pimp.transform.localPosition.z);

        // 남은 거리, 포주와의 거리 텍스트 입력
        endText.text = string.Format("{0:0}m", speatDistance); distanceText.text = string.Format("{0:0}m", pimpDistance);

        // 게임 끝
        if (speatSlider.value >= speatSlider.maxValue) GetComponentInParent<MapData>().positionSets[0].clearBox.GetComponent<CheckMapClear>().isClear = true;
        else if (speatSlider.value >= 10 && speatSlider.value <= pimpSlider.value + 1) InitRungame();
    }

    #region 런게임 세팅
    float speatAccelator = 0;
    float pimpAccelator = 0;
    IEnumerator StartRungame()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.05f);
        CharacterManager speat = DataController.instance_DataController.speat;
        speat.jumpForce = 7;
        speat.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
        while (speatSlider.value < speatSlider.maxValue)
        {
            // 스핏 달리기(장애물에 막히지 않았을 때만)
            if (startPosition.position.x < speat.transform.position.x) { speatSlider.value += 0.04f + speatAccelator; speatAccelator += 0.0004f; }
            else speatAccelator *= 0.0003f;
            DataController.instance_DataController.inputDegree = 1f;
            // 포주 달리기
            if (pimpSlider.handleRect.gameObject.activeSelf) pimpSlider.value += 0.045f + pimpAccelator; pimpAccelator += 0.0002f;

            if (startPosition.position.x < speat.transform.position.x)
            {
                for (int i = 1; i < trailer.Length; i++)
                {
                    // 트레일러 이동
                    trailer[i].position = trailer[0].position.x - trailer[i].position.x >= 18f * 2 ?
                        trailer[0].position + 18.5f * 2 * Vector3.right : // 앞 위치로 이동
                        trailer[i].position - Vector3.right * (0.3f + speatAccelator); // 뒤로 밀기
                    // 제거 및 생성
                    if (trailer[0].position.x - trailer[i].position.x >= 18f * 2)
                    {
                        //int index = speatDistance > 66 ? 0 : speatDistance > 33 ? 1 : 2;>>나중에 적당한 카운터 수 찾게 되면 이걸로 바꾸기
                        int index = clearCount < 5 ? 0 : clearCount < 10 ? 1 : 2;
                        //int index = 0;
                        clearCount++;

                        trailer[i].GetChild(0).gameObject.SetActive(false);
                        trailer[i].GetChild(0).SetParent(patternFolder[index], false);

                        if (speatSlider.value < speatSlider.maxValue * 0.8f)
                        {
                            if (index > 0)
                                index -= Random.Range(0, 2);
                            GameObject pattern = GetPattern(index, "Pattern" + (index + 1) + "_" + Random.Range(1, patternIndex[index] + 1));
                            Vector3 temp = pattern.transform.position;
                            pattern.SetActive(true);
                            pattern.transform.SetParent(trailer[i]);
                            pattern.transform.localPosition = temp;
                        }
                        else
                        {
                            patterns[0][5].SetActive(true);
                            patterns[0][5].transform.SetParent(trailer[i]); // 장애물 없는 길 생성
                        }
                    }
                }
            }
            yield return waitForSeconds;
        }
        DataController.instance_DataController.currentChar.jumpForce = 4;
        DataController.instance_DataController.currentChar.UseJoystickCharacter();
    }
    // 런게임 초기화
    private void InitRungame()
    {
        StopCoroutine("StartRungame");
        speatAccelator = 0;
        speatSlider.value = 0;
        pimpAccelator = 0;
        pimpSlider.value = 0;
        isRunning = false;
    }
    private GameObject GetPattern(int index, string tag)
    {
        GameObject pattern = patterns[index].Find(x => (x.name.Equals(tag) && !x.activeSelf));
        if (!pattern)
        {
            pattern = Instantiate(patterns[index].Find(x => x.name.Equals(tag)));
            pattern.name = tag;
            patterns[index].Add(pattern);
        }
        return pattern;
    }
    #endregion

    #region 액션 버튼
    // 점프 버튼
    public void JumpBtn(){
        if (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount == 0)
        {
            DataController.instance_DataController.inputJump = true;
            StartCoroutine("JumpCooldown");
        }
    }
    IEnumerator JumpCooldown()
    {
        jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount = 1;
        while (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount > 0)
        {
            jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount -= 0.04f;
            if (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount < 0.9f) DataController.instance_DataController.inputJump = false;
            yield return new WaitForSeconds(0.002f);
        }
    }
    // 능력 사용 버튼
    public void AbilityBtn()
    {
        if (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount == 0)
        {
            DataController.instance_DataController.currentChar.gameObject.layer = 9;
            StartCoroutine("AbilityCooldown");
        }
    }
    IEnumerator AbilityCooldown()
    {
        DataController.instance_DataController.currentChar.gameObject.layer = 9;
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount < 1)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount += 0.02f;
            yield return new WaitForSeconds(0.005f);
        }
        DataController.instance_DataController.currentChar.gameObject.layer = 0;
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount > 0)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount -= 0.02f;
            yield return new WaitForSeconds(0.002f);
        }
    }
    #endregion
}
