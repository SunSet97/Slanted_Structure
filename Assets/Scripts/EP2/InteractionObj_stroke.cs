using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 인터렉션하는 오브젝트에 컴포넌트로 추가!!
public class InteractionObj_stroke : MonoBehaviour
{
    public enum OutlineColor // 일단 내비둬
    {
        red,
        magenta,
        yellow,
        green,
        blue,
        grey,
        black,
        white

    }

    public OutlineColor color;
    public bool onOutline = false; // 아웃라인 켜져있는 안켜져 있는지
    public bool isCharacterInRange = false; // obj_interaction 오브젝트 기준으로 일정 범위 내에 캐릭터가 있는지 확인
    public int radius = 5;
    public Text exclamationMark;
    [Header("느낌표 사용할 때 체크")]
    public bool useExclamationMark = false;
    public float y; // 느낌표 위치(y좌표) 조절
    private bool isInteractionObj = false;
    private Outline outline;
    private CharacterManager character;
    Camera cam;


    void Start()
    {
        if (gameObject.tag == "obj_interaction")
        {
            isInteractionObj = true;

            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineWidth = 8f; // 아웃라인 두께 설정
            outline.enabled = false; // 우선 outline 끄기

            cam = DataController.instance_DataController.cam;

            exclamationMark.gameObject.SetActive(false); // 느낌표 끄기

            character = DataController.instance_DataController.currentChar;


            // 아웃라인 색깔 설정
            if (color == OutlineColor.red) outline.OutlineColor = Color.red;
            else if (color == OutlineColor.magenta) outline.OutlineColor = Color.magenta;
            else if (color == OutlineColor.yellow) outline.OutlineColor = Color.yellow;
            else if (color == OutlineColor.green) outline.OutlineColor = Color.green;
            else if (color == OutlineColor.blue) outline.OutlineColor = Color.blue;
            else if (color == OutlineColor.grey) outline.OutlineColor = Color.grey;
            else if (color == OutlineColor.black) outline.OutlineColor = Color.black;
            else if (color == OutlineColor.white) outline.OutlineColor = Color.white;

        }

    }

    void Update()
    {
        if (!character)
        {
            character = DataController.instance_DataController.currentChar;
        }

        if (!isInteractionObj)
        {
            Debug.Log("tag를 obj_interaction으로 설정하세요");
        }
        else
        {
            CheckAroundCharacter(); // 일정 범위 안에 선택된 캐릭터 있는지 확인
            if (isCharacterInRange && !onOutline) // 범위 내로 들어옴
            {
                // 아웃라인 켜기
                onOutline = true;
                outline.enabled = true;

                if (useExclamationMark)
                {
                    // 느낌표 보이게
                    exclamationMark.gameObject.SetActive(true);
                }

            }
            else if (!isCharacterInRange && onOutline) // 범위 밖
            {
                // 아웃라인 끄기
                onOutline = false;
                outline.enabled = false;

                if (useExclamationMark)
                {
                    // 느낌표 안보이게
                    exclamationMark.gameObject.SetActive(false);
                }

            }

        }

        Vector3 myScreenPos = cam.WorldToScreenPoint(transform.position);
        exclamationMark.transform.position = myScreenPos + new Vector3(0, y, 0);

    }

    void CheckAroundCharacter()
    {
        RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position, radius, Vector3.up, 0f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == character.gameObject)
            {
                isCharacterInRange = true;
            }
            else
            {
                isCharacterInRange = false;
            }

        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }

}