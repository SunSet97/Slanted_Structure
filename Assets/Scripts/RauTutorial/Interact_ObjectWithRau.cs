using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact_ObjectWithRau : MonoBehaviour
{
    [Header("#Outline setting")]
    public bool isTouched = false;
    [Header("#Outline setting")]
    public Color outlineColor = Color.red;
    public Vector3 offset = Vector3.zero;
    public float radius = 3;
    public Outline.Mode mode = Outline.Mode.OutlineAll;
    public Outline outline;
    [Header("#Mark setting")]
    public GameObject mark;
    public Vector2 markOffset = Vector2.zero;
    public bool isIn = false;
    void Update()
    {
        if (DataController.instance_DataController != null)
        {
            // Outline이 없으면 추가 및 초기화
            if (this.gameObject.GetComponent<Outline>() == null)
            {
                outline = this.gameObject.AddComponent<Outline>();  // 추가
                outline.OutlineMode = mode;                         // 모드 설정
                outline.OutlineColor = outlineColor;                // 색 설정
                outline.OutlineWidth = 8f;                          // 아웃라인 두께 설정
                outline.enabled = false;                            // Outline 끄기
                if(this.gameObject.GetComponent<BoxCollider>() == null) this.gameObject.AddComponent<BoxCollider>().isTrigger = true;  // 콜라이더 없으면 자동 추가
            }
            // Outline 있을 시
            else if(this.gameObject.activeSelf)
            {
                //print("@@@@@@@@@@@@@@@");
                isIn = CheckAroundCharacter();
                //print("근처?: " + isIn);
                //outline.enabled = CheckAroundCharacter(); // Outline 활성화
                outline.enabled = isIn;
                //print("아웃라인 활성화됨?: " + outline.enabled);
                if (mark != null)
                {
                    mark.gameObject.SetActive(CheckAroundCharacter()); // 마크 활성화
                    mark.transform.position = (Vector3)markOffset + DataController.instance_DataController.cam.WorldToScreenPoint(transform.position); // 마크 위치 설정
                }

                GetObjectTouch();
            }
        }
    }

    // 주변에 캐릭터 있으면 true, 아니면 false
    private bool CheckAroundCharacter()
    {
        RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position + offset, radius, Vector3.up, 0f);
        bool temp = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != null && DataController.instance_DataController.currentChar != null)
                temp = hit.collider.name == DataController.instance_DataController.currentChar.name ? true : false;
            else
                temp = false;
            //print("hit.collider.name :  " + hit.collider.name + " /DataController.instance_DataController.currentChar.name: " + DataController.instance_DataController.currentChar.name + "/ temp: " + temp);
        }
        //if (temp) print("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");

        return temp;
    }

    // 오브젝트 터치 판정
    private void GetObjectTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = DataController.instance_DataController.cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != null)
                    isTouched = hit.collider.name == this.gameObject.name ? true : false;
                else
                    isTouched = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = outlineColor - Color.black * 0.7f;
        Gizmos.DrawWireSphere(gameObject.transform.position + offset, radius);
        Gizmos.DrawSphere(gameObject.transform.position + offset, radius);
    }
}