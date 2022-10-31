using System.Collections;
using System.Linq;
using Play;
using UnityEngine;
using UnityEngine.UI;
using static Data.CustomEnum;
public class SpeatTutorialBackstreetManager : MonoBehaviour, IGamePlayable
{
    public bool IsPlay { get; set; } = false;

    [Header("End Dialogue")]
    [SerializeField] private TextAsset _jsonFile;

    [Header("#UI")]
    public Slider speatSlider;
    public Text endText;
    public Slider pimpSlider;
    public Text distanceText;

    [Header("#Objects")]
    public Transform startPosition;
    public GameObject pimp;
    public Transform[] trailer;
    private GameObject[][] patterns;

    [Header("#Buttons")]
    public Button jumpBtn;
    public Button abilityBtn;

    float percentage;
    private float speatDistance;
    float pimpDistance;
    public float runSpeed;

    private AssetBundle _assetBundle;
    
    private readonly string PATH = "/AssetBundles/backstreetrun";
    void Start()
    {
        InitRunGame();
    }
    private void InitRunGame()
    {
        _assetBundle = AssetBundle.LoadFromFile(Application.dataPath + PATH);
        
        Debug.Log(_assetBundle == null ? "Fail to load" : "Success to load");
        var objs = _assetBundle.LoadAllAssets<GameObject>();
        
        patterns = new GameObject[4][];
        for (var idx = 0; idx < patterns.Length; idx++)
        {
            GameObject[] t = objs.Where(item => item.name.Substring(7, 1) == idx.ToString()).Distinct().ToArray();
            foreach (var tt in t)
            {
                Debug.Log(tt);
            }
            patterns[idx] = t;

            Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
        }
        _assetBundle.Unload(false);
    }

    public void Play()
    {
        IsPlay = true;
        StartCoroutine(StartRungame());
    }

    public void EndPlay()
    {
        _assetBundle.Unload(true);
        if (_jsonFile != null)
        {
            DialogueController.instance.SetDialougueEndAction(() =>
            {
                IsPlay = false;
                DataController.instance.currentMap.MapClear();
            });
            DialogueController.instance.StartConversation(_jsonFile.text);
        }
        else
        {
            IsPlay = false;
            DataController.instance.currentMap.MapClear();
        }
    }

    void Update()
    {
        if (IsPlay && DialogueController.instance.isTalking)
        {
            percentage = 100 / speatSlider.maxValue;
            speatDistance = (speatSlider.maxValue - speatSlider.value) * percentage; // 종료 지점과 스핏의 거리
            pimpDistance = (speatSlider.value - pimpSlider.value) * percentage; // 스핏과 포주의 거리


            // 일정 거리 이후에 포주 출현
            if (!pimp.activeSelf && speatSlider.value > speatSlider.maxValue * 0.1f)
            {
                pimp.SetActive(true);
                pimpSlider.fillRect.gameObject.SetActive(true);
                pimpSlider.handleRect.gameObject.SetActive(true);
            }

            // 포주와 스핏의 거리 설정
            if (pimp.activeSelf)
            {
                Vector3 localPosition = pimp.transform.localPosition;
                localPosition = new Vector3(20 - pimpDistance * 2, localPosition.y,
                    localPosition.z);
                pimp.transform.localPosition = localPosition;
            }

            // 남은 거리, 포주와의 거리 텍스트 입력
            endText.text = $"{speatDistance:0}m";
            distanceText.text = $"{pimpDistance:0}m";

            // 게임 끝
            if (speatSlider.value >= speatSlider.maxValue)
            {
                EndPlay();
            }
            else if (speatSlider.value >= 10 && speatSlider.value <= pimpSlider.value + 1) DataController.instance.ChangeMap(DataController.instance.mapCode);
        }
    }

    #region 런게임 세팅
    float speatAccelator = 0;
    float pimpAccelator = 0;
    IEnumerator StartRungame()
    {
        CharacterManager mainChar = DataController.instance.GetCharacter(Character.Speat_Adult);
        mainChar.jumpForce = 7;
        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        JoystickController.instance.InitializeJoyStick(false);
        JoystickController.instance.inputDegree = 1;
        JoystickController.instance.inputDirection.x = 1;
        
        DataController.instance.currentMap.isJoystickInputUse = false;

        while (speatSlider.value < speatSlider.maxValue)
        {
            // 스핏 달리기(장애물에 막히지 않았을 때만)
            if (startPosition.position.x < mainChar.transform.position.x)
            {
                speatSlider.value += 0.8f * Time.fixedDeltaTime + speatAccelator;
                speatAccelator += 0.0009f * Time.fixedDeltaTime;
            }
            else
            {
                speatAccelator *= 0.97f;
            }

            // 포주 달리기
            if (pimpSlider.handleRect.gameObject.activeSelf)
            {
                pimpSlider.value += 0.85f * Time.fixedDeltaTime + pimpAccelator;
                pimpAccelator += 0.0005f * Time.fixedDeltaTime;
            }

            if (startPosition.position.x < mainChar.transform.position.x)
            {
                for (int i = 1; i < trailer.Length; i++)
                {
                    // 트레일러 이동
                    trailer[i].position = trailer[0].position.x - trailer[i].position.x >= 18f * 2 ?
                        trailer[0].position + 18.5f * 2 * Vector3.right: // 앞 위치로 이동
                        trailer[i].position - Vector3.right * (runSpeed + speatAccelator) * Time.fixedDeltaTime; // 뒤로 밀기
                    // 제거 및 생성
                    if (trailer[0].position.x - trailer[i].position.x >= 18f * 2)
                    {
                        Destroy(trailer[i].GetChild(0).gameObject); // 현재 장애물 패턴 제거

                        if (speatSlider.value < speatSlider.maxValue * 0.8f)
                        {
                            int index = speatDistance > 66 ? 1 : speatDistance > 33 ? 2 : 3;
                            Instantiate(patterns[index][Random.Range(0, patterns[index].Length)], trailer[i]).SetActive(true); // 장애물 패턴 랜덤 생성
                        }
                        else
                        {
                            Instantiate(patterns[0][1], trailer[i]).SetActive(true); // 장애물 없는 길 생성
                        }
                    }
                }
            }
            yield return waitForFixedUpdate;
        }
        mainChar.jumpForce = 4;
        mainChar.UseJoystickCharacter();
    }
    
    //private GameObject GetPattern(int index, string tag)
    //{
    //    GameObject pattern = patterns[index].Find(x => (x.name.Equals(tag) && !x.activeSelf));
    //    if(!pattern)
    //    {
    //        pattern = Instantiate(patterns[index].Find(x => x.name.Equals(tag)));
    //        patterns[index].Add(pattern);
    //    }
    //    return pattern;
    //}
    
    #endregion

    #region 액션 버튼
    // 점프 버튼
    public void JumpBtn(){
        if (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount == 0)
        {
            JoystickController.instance.inputJump = true;
            StartCoroutine("JumpCooldown");
        }
    }
    
    IEnumerator JumpCooldown()
    {
        jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount = 1;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.002f);
        while (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount > 0)
        {
            jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount -= 0.04f;
            if (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount < 0.9f)
            {
                JoystickController.instance.inputJump = false;
            }
            yield return waitForSeconds;
        }
    }
    
    // 능력 사용 버튼
    public void AbilityBtn()
    {
        if (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount == 0)
        {
            DataController.instance.GetCharacter(Character.Main).gameObject.layer = 9;
            StartCoroutine("AbilityCooldown");
        }
    }
    IEnumerator AbilityCooldown()
    {
        DataController.instance.GetCharacter(Character.Main).gameObject.layer = 9;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount < 1)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount += 0.02f;
            yield return waitForSeconds;
        }
        waitForSeconds = new WaitForSeconds(0.002f);
        DataController.instance.GetCharacter(Character.Main).gameObject.layer = 0;
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount > 0)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount -= 0.02f;
            yield return waitForSeconds;
        }
    }
    #endregion
}
