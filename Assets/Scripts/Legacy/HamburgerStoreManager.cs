using System.Collections;
using UnityEngine;
using Utility.Character;
using Utility.Core;
using Utility.Dialogue;
using Utility.Interaction;
using static Data.CustomEnum;

public class HamburgerStoreManager : MonoBehaviour
{
    public TextAsset jsonFile;
    [SerializeField]
    private NpcWayPoint speat;

    [Range(0, 1f)] public float rauAniSpeed;

    public InteractionObject partTimeJob;
    public InteractionObject sofa;

    private void Start()
    {
        InitialSetting();
    }

    private void InitialSetting()
    {
        // 전부 타임라인으로 해도 되겠는뎁쇼
        partTimeJob.SetInteractionEndEvent(() => { sofa.enabled = true; });

        sofa.SetInteractionStartEvent(() =>
        {
            var character = DataController.Instance.GetCharacter(CharacterType.Main);
            character.PickUpCharacter();
            character.transform.position = sofa.transform.GetChild(0).position;
            character.transform.rotation = sofa.transform.GetChild(0).rotation;
            character.CharacterAnimator.SetBool("Seat", true);
        });
        
        sofa.SetInteractionEndEvent(() =>
        {
            JoystickController.Instance.SetJoyStickState(false);
            speat.StartMoving();
        });

        speat.SetPointEvent(() =>
        {
            DialogueController.Instance.AddDialogueEndAction(() =>
            {
                speat.StartMoving();
                StartCoroutine(StartRauMoving());
            });
            DialogueController.Instance.StartConversation(jsonFile.text);
        }, 3);
    }

    private IEnumerator StartRauMoving()
    {
        CharacterManager character = DataController.Instance.GetCharacter(CharacterType.Main);
        character.PickUpCharacter();
        int index = speat.pointIndex;
        PointData desPoint = speat.point[index];

        character.CharacterAnimator.SetBool("Seat", false);
        yield return new WaitForSeconds(2f);
        character.CharacterAnimator.SetFloat("Speed", rauAniSpeed);

        character.transform.LookAt(desPoint.transform);

        while (index < speat.point.Length || !speat.point[index].isStop)
        {
            if (character.transform.position == speat.point[index].transform.position)
            {
                index++;
                if (index >= speat.point.Length) yield break;
                desPoint = speat.point[index];
                character.transform.LookAt(desPoint.transform);
            }
            else
            {
                character.transform.position = Vector3.MoveTowards(character.transform.position,
                    desPoint.transform.position, speat.speed * Time.deltaTime * 0.95f);
            }

            yield return null;
        }
    }
}

