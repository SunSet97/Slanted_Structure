using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Data.CustomEnum;

public class CanvasControl : MonoBehaviour
{
    [Header("저장 패널")]
    public Text[] saveText = new Text[3];
    public GameObject savePanel;

    // 추후 변경 
    [Header("친밀도/자존감")]
    public Text intimacyTextSpeat;
    public Text intimacyTextOun;
    public Text selfEstmText;

    [Header("대화 관련")]
    public bool isInConverstation;


    private bool isExistFile;

    [Header("이야기 진행")]
    public GameObject commandPanel;
    //인스턴스화
    private static CanvasControl _instance;
    public static CanvasControl instance
    {
        get => _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
    }

    // 자존감 및 친밀도 text 업데이트
    public void UpdateStats()
    {
        selfEstmText.text = "자존감: " + DataController.instance.charData.selfEstm;
        intimacyTextSpeat.text = "스핏 친밀도: " + DataController.instance.charData.intimacy_spOun;
        intimacyTextOun.text = "오운 친밀도: " + DataController.instance.charData.intimacy_ounRau;
    }

    bool isExistCmdSpr; // 지시문과 같이 나올 이미지 존재 여부
    GameObject curCmdSpr; // 지시문과 같이 나올 이미지의 parent
    public SpriteRenderer[] sprRenderers; // curCmdSpr의 렌더러
    Coroutine fadeIn;
    
    Coroutine fadeOut;
    CanvasGroup canvasGroup;
    float speed = 0.7f;
    public bool finishFadeIn = false;
    
    IEnumerator FadeIn()
    {
        canvasGroup = commandPanel.GetComponent<CanvasGroup>();

        while (canvasGroup.alpha <= 1)
        {
            if (isExistCmdSpr)
            {
                foreach(SpriteRenderer render in sprRenderers)
                {
                    Color color = render.color;
                    color.a += Time.deltaTime * speed;
                    render.color = color;
                }
            }
            canvasGroup.alpha += Time.deltaTime * speed;

            if (canvasGroup.alpha >= 1)
            {
                finishFadeIn = true;
                StopCoroutine(fadeIn);
            }
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        canvasGroup = commandPanel.GetComponent<CanvasGroup>();

        while (canvasGroup.alpha >= 0f)
        {
            if (isExistCmdSpr)
            {
                foreach (SpriteRenderer render in sprRenderers)
                {
                    Color color = render.color;
                    color.a -= Time.deltaTime * speed;
                    render.color = color;
                }
            }

            canvasGroup.alpha -= Time.deltaTime * speed;
            if (canvasGroup.alpha <= 0f)
            {
                commandPanel.SetActive(false);
                StopCoroutine(fadeOut);
            }
            yield return null;
        }
    }
}
