﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeatTutorialBackstreetManager : MonoBehaviour, Playable
{
    public bool isPlay { get; set; } = false;
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
    private List<GameObject[]> patterns = new List<GameObject[]>();
    public Transform patternFolder;

    [Header("#Buttons")]
    public Button jumpBtn;
    public Button abilityBtn;

    float percentage;
    private float speatDistance;
    float pimpDistance;
    public float runSpeed;
    void Start()
    {
        initRunGame();
    }
    private void initRunGame()
    {
        for (int i = 0; i < 3; i++)
        {
            patterns.Add(Resources.LoadAll<GameObject>("Run_Pattern/Pattern" + i));
        }
        StartCoroutine(StartRungame());
    }

    void Update()
    {
        if (isPlay)
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
            if (pimp.activeSelf) pimp.transform.localPosition = new Vector3(20 - pimpDistance * 2, pimp.transform.localPosition.y, pimp.transform.localPosition.z);

            // 남은 거리, 포주와의 거리 텍스트 입력
            endText.text = string.Format("{0:0}m", speatDistance); distanceText.text = string.Format("{0:0}m", pimpDistance);

            // 게임 끝
            if (speatSlider.value >= speatSlider.maxValue)
            {
                if (_jsonFile != null)
                {
                    CanvasControl.instance_CanvasControl.SetDialougueEndAction(() => { DataController.instance_DataController.currentMap.positionSets[0].clearBox.GetComponent<CheckMapClear>().Clear(); });
                    CanvasControl.instance_CanvasControl.StartConversation(_jsonFile.text);
                }
                else
                {
                    DataController.instance_DataController.currentMap.positionSets[0].clearBox.GetComponent<CheckMapClear>().Clear();
                }
            }
            else if (speatSlider.value >= 10 && speatSlider.value <= pimpSlider.value + 1) DataController.instance_DataController.ChangeMap(DataController.instance_DataController.mapCode);
        }
    }

    #region 런게임 세팅
    float speatAccelator = 0;
    float pimpAccelator = 0;
    IEnumerator StartRungame()
    {
        yield return new WaitUntil(() => { return isPlay; });
        CharacterManager speat = DataController.instance_DataController.GetCharacter(DataController.CharacterType.Speat);
        speat.jumpForce = 7;
        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        DataController.instance_DataController.InitializeJoystic(false);
        Debug.Log("지금");
        DataController.instance_DataController.currentMap.isJoystickUse = false;
        DataController.instance_DataController.inputDegree = 1;
        DataController.instance_DataController.inputDirection.x = 1;

        while (speatSlider.value < speatSlider.maxValue)
        {
            // 스핏 달리기(장애물에 막히지 않았을 때만)
            if (startPosition.position.x < speat.transform.position.x)
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

            if (startPosition.position.x < speat.transform.position.x)
            {
                GameObject.Find("Speat Tutorial backstreet");
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
                            int index = speatDistance > 66 ? 0 : speatDistance > 33 ? 1 : 2;
                            Instantiate(patterns[index][Random.Range(0, patterns[index].Length)], trailer[i]).SetActive(true); // 장애물 패턴 랜덤 생성
                        }
                        else
                        {
                            Instantiate(patterns[0][5], trailer[i]).SetActive(true); // 장애물 없는 길 생성
                        }
                    }
                }
            }
            yield return waitForFixedUpdate;
        }
        speat.jumpForce = 4;
        speat.UseJoystickCharacter();
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
            DataController.instance_DataController.inputJump = true;
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
            if (jumpBtn.GetComponentsInChildren<Image>()[1].fillAmount < 0.9f) DataController.instance_DataController.inputJump = false;
            yield return waitForSeconds;
        }
    }
    // 능력 사용 버튼
    public void AbilityBtn()
    {
        if (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount == 0)
        {
            DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main).gameObject.layer = 9;
            StartCoroutine("AbilityCooldown");
        }
    }
    IEnumerator AbilityCooldown()
    {
        DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main).gameObject.layer = 9;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.005f);
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount < 1)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount += 0.02f;
            yield return waitForSeconds;
        }
        waitForSeconds = new WaitForSeconds(0.002f);
        DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main).gameObject.layer = 0;
        while (abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount > 0)
        {
            abilityBtn.GetComponentsInChildren<Image>()[1].fillAmount -= 0.02f;
            yield return waitForSeconds;
        }
    }
    #endregion
}
