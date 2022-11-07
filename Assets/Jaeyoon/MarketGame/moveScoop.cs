using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class moveScoop : MonoBehaviour
{
    public Button IcecreamClickCheckButton;
    public Button failCheckButton;
    public Image change_sellerFace;
    public Sprite[] sellerSprites;
    public Image icecream_background;

    Vector3 positionSet;
    float standard_range = 600.0f;
    float horizontal_speed = 2.5f;
    //true일 때만 움직이고 false일 때는 안 움직이게
    bool checkMoving;
    float timer = 0;
    float horizontal_timer = 0;
    int count_success = 0;


    void Start()
    {
        positionSet = transform.position;
        IcecreamClickCheckButton.interactable = false;
        IcecreamClickCheckButton.onClick.AddListener(() =>
        {
            StopCoroutine(ScoopMoveVertical());
            StartCoroutine(ChangeSellerFace(2));
            count_success++;
            Debug.Log(count_success);
            transform.position = positionSet;
            IcecreamClickCheckButton.interactable = false;
            if (count_success == 3) { 
                StopAllCoroutines(); 
            }
        });
        failCheckButton.onClick.AddListener(() =>
        {
            int randomFace = Random.Range(0, 2);
            int facenum = randomFace * 2 + 1;
            StartCoroutine(ChangeSellerFace(facenum));
        });
        checkMoving = true;
    }

    void Update()
    {
        if (checkMoving)
        {
            ScoopMoveHorizontal();
            timer += Time.deltaTime;
            if (timer > 5f) { 
                checkMoving = false;
                StartCoroutine(ScoopMoveVertical()); 
            }
        }
    }
    IEnumerator ScoopMoveVertical()
    {
        var saveVec = transform.position;
        //Vector3 switchPosition = 2.position;
        ////switchPosition.y = Random.Range(220, 500);
        ////transform.position = switchPosition;
        //switchPosition.y = Mathf.Lerp(220, 500, 50);
        //transform.position = switchPosition;
        IcecreamClickCheckButton.interactable = true;
        
        float t = 0f;

        while(t <= (horizontal_speed/26f))
        {
            t += Time.deltaTime;
            Vector3 switchPosition = saveVec;
            switchPosition.y = Mathf.Lerp(saveVec.y, 400, t / (horizontal_speed / 26f));
            transform.position = switchPosition;
            yield return null;
        }       

        yield return new WaitForSeconds(.1f);

        t = (horizontal_speed / 12f);

        while (t >= 0)
        {
            t -= Time.deltaTime;
            Vector3 switchPosition = saveVec;
            switchPosition.y = Mathf.Lerp(saveVec.y, 400, t / (horizontal_speed / 26f));
            transform.position = switchPosition;
            yield return null;
        }

        IcecreamClickCheckButton.interactable = false;
        yield return new WaitForSeconds(.1f);
        timer = 0;
        checkMoving = true;
    }

    void ScoopMoveHorizontal()
    {
        horizontal_timer += Time.deltaTime;
        //int acceleration = Random.Range(1, 3);
        Vector3 currentPosition = positionSet;
        currentPosition.x += standard_range * Mathf.Sin(horizontal_timer * horizontal_speed * 2);
       // print(acceleration);
        transform.position = currentPosition;
    }

    IEnumerator ChangeSellerFace(int basicface_num) {
        yield return new WaitForSeconds(.1f);
        change_sellerFace.sprite = sellerSprites[basicface_num];
        yield return new WaitForSeconds(.7f);
        change_sellerFace.sprite = sellerSprites[0];
    }

    IEnumerator ReturnToDefault() {
        var speed = 0.5f;
        while(Mathf.Approximately(Vector2.Distance(transform.position, positionSet), 0))
        {
            transform.position = Vector2.Lerp(transform.position, positionSet, Time.deltaTime * speed);
            
            yield return null;
        }
    }
}