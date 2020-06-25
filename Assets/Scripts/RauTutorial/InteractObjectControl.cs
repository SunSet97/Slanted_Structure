using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractObjectControl : MonoBehaviour
{
    public bool isInteractable;
    public bool playMiniGame;
    public Slider slider;
    public GameObject dustTree;
    public GameObject bridge;
    private int correctCnt;
    private CanvasControl canvasCtrl;

    // Start is called before the first frame update
    void Start()
    {
        canvasCtrl = CanvasControl.instance_CanvasControl;
        isInteractable = false;
        correctCnt = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInteractable) InteractWIthThis();
        if (playMiniGame) MoveSlider();
    }

    void InteractWIthThis()
    {
        // 상호작용 가능 할 때에만 npc와의 대화가 가능함 
        if (Input.GetMouseButtonDown(0))
        {
            Camera curCamera = null;

            if (Camera.main.CompareTag("MainCamera"))
            {
                curCamera = Camera.main.GetComponent<Camera>();
            }


            // 카메라 ~ 터치 지점으로의 ray 
            Ray ray = curCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.name == gameObject.name && canvasCtrl.isPossibleCnvs == true)
                {

                    if (hit.collider.gameObject.name == "DustTree")
                        canvasCtrl.isGoNextStep = true;
                    // 대화가 끝나기 전까지 다시 대화 불가능하도록 설정
                    canvasCtrl.isPossibleCnvs = false;

                    if (DataController.instance_DataController.currentChar.name == "Rau")
                        DataController.instance_DataController.LoadData("Interaction/RauTutorial/", gameObject.name + ".json");
                    else
                        DataController.instance_DataController.LoadData("Interaction/SpeatTutorial/", gameObject.name + ".json");

                    canvasCtrl.StartConversation();
                }

            }
        }
    }

    public float fillSpeed = 0;
    bool isSliderMax = false;
    void MoveSlider()
    {
        print ("아아아아너ㅏㅇ리하ㅜ니");
        slider.value += Time.deltaTime * fillSpeed;

        if (slider.value >= 1)
        {
            slider.value = 1;
            fillSpeed *= -1;
        }
        if (slider.value <= 0)
        {
            slider.value = 0;
            fillSpeed *= -1;
        }


        //if (isSliderMax == false)
        //{
        //    slider.value += Time.deltaTime * fillSpeed;
        //    if (slider.value >= slider.maxValue)
        //        isSliderMax = true;
        //}
        //else
        //{
        //    slider.value -= Time.deltaTime * fillSpeed;
        //    if (slider.value <= 0)
        //        isSliderMax = false;
        //}
       
    }

    public float correctMin;
    public float correctMax;
    public void StopSlider()
    {
        float curValue = slider.value;
        if (Mathf.Clamp(curValue, correctMin, correctMax) == curValue)
        {
            correctCnt++;
            if (correctCnt == 3)
            {
                playMiniGame = false;
                // 나무 쓰러뜨리는 코루틴 
                dustTree.SetActive(false);
                bridge.SetActive(true);

                gameObject.SetActive(false);
                canvasCtrl.isGoNextStep = true;
                canvasCtrl.GoNextStep();
                // 그 다음 스테이지로 가도록 할 것
            }
        }
    }

    //IEnumerator TreeRotation()
    //{
    //    while (canvasGroup.alpha <= 1)
    //    {
    //        if (isExistCmdSpr)
    //        {
    //            foreach (SpriteRenderer render in sprRenderers)
    //            {
    //                Color color = render.color;
    //                color.a += Time.deltaTime * speed;
    //                render.color = color;
    //            }
    //        }
    //        canvasGroup.alpha += Time.deltaTime * speed;

    //        if (canvasGroup.alpha >= 1)
    //        {
    //            finishFadeIn = true;
    //            StopCoroutine(fadeIn);
    //        }
    //        yield return null;
    //    }
    //}


}
