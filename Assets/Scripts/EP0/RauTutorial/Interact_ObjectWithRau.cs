using System.Collections;
using UnityEngine;
using static Data.CustomEnum;

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
    public bool isInteracting = false; // 외부 스크립트에서 조절하기
    [Header("#Mark setting")]
    public GameObject mark;
    public Vector2 markOffset = Vector2.zero;
    public bool isIn = false;

    void Update()
    {
        // Outline이 없으면 추가 및 초기화
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();  // 추가
            outline.OutlineMode = mode;                         // 모드 설정
            outline.OutlineColor = outlineColor;                // 색 설정
            outline.OutlineWidth = 8f;                          // 아웃라인 두께 설정
            outline.enabled = false;                            // Outline 끄기
            if (gameObject.GetComponent<BoxCollider>() == null) 
                gameObject.AddComponent<BoxCollider>().isTrigger = true;  // 콜라이더 없으면 자동 추가
        }
        // Outline 있을 시
        else if (gameObject.activeSelf)
        {
            isIn = CheckAroundCharacter();
            //if (mark != null &&!isInteracting)
            if (!isInteracting)
            {
                outline.enabled = isIn; // Outline 활성화
                if (mark != null)
                {
                    mark.gameObject.SetActive(isIn); // 마크 활성화
                    mark.transform.position = (Vector3)markOffset + DataController.instance.cam.WorldToScreenPoint(transform.position); // 마크 위치 설정
                }
            }
            else if (isIn && isInteracting)
            {
                outline.enabled = false; // 범위 내에 있으면서 인터랙션중일 때 Outline 비활성화
                if (mark) mark.gameObject.SetActive(false); // 범위 내에 있으면서 인터랙션중일 때 마크 비활성화
            }

            // if (CanvasControl.instance.isPossibleCnvs && isIn) GetObjectTouch();
        }
    }

    // 주변에 캐릭터 있으면 true, 아니면 false
    private bool CheckAroundCharacter()
    {
        RaycastHit[] hits = Physics.SphereCastAll(gameObject.transform.position + offset, radius, Vector3.up, 0f);
        CharacterManager currentChar = DataController.instance.GetCharacter(Character.Main);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.name.Equals(currentChar.name))
                return true;
        }
        return false;
    }

    // 오브젝트 터치 판정
    // 여기서 받는 hit을 NPCInterator로 보내기
    public void GetObjectTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = DataController.instance.cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 5f);
            int layerMask = 1 << 13; // 13번 레이어마스크가 ClickObject임. 해당 레이어 마스크만 터치로 받아들인다. 터치될 오브젝트 or 캐릭터에 무조건!! 13번레이어 ClickObject 설정해줘야 한다.
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (outline.enabled)
                    {
                        isTouched = true;
                        isInteracting = true;
                        outline.enabled = false; // 아웃라인 끄기
                        StartCoroutine(ChangeIsInteractingToFalse(5));
                        if (mark != null)
                            mark.gameObject.SetActive(false); // 마크 끄기
                    }
                    if (hit.collider.transform.parent.name == "NPCManager" && CanvasControl.instance.isPossibleCnvs)
                    {
                        if (hit.collider.transform.parent.TryGetComponent(out NPCInteractor npcInteractor))
                        {
                            npcInteractor.FindInteractableNPC(hit, radius);
                            //이걸 써??? 에반데  이딴게 코드?
                        }
                        else
                        {
                            //NPCInteractor.instance_NPCInteractor.FindInteractableNPC(hit, radius);
                            Debug.LogError("오류 발생했음");
                        }
                    }
                    break;
                }
            }
            StartCoroutine(ChangeIsTouchedToFalse()); // 한 프레임 이후 isTouched = false로 바뀜.
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = outlineColor - Color.black * 0.7f;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }

    IEnumerator ChangeIsInteractingToFalse(float time)
    {
        yield return new WaitForSeconds(time); // 일정 시간 후 isInteracting 상태 변경
        isInteracting = false;
    }

    IEnumerator ChangeIsTouchedToFalse()
    {
        yield return null; // 한 프레임 끝난 후 isTouched = false로 바꾸기 (이렇게 해야지 외부 스크립트에서 오브젝트의 터치 유무를 알 수 있음!)
        isTouched = false;
    }

}