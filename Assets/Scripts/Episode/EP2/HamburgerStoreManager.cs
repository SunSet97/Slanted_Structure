using System.Collections;
using UnityEngine;
using Utility.Core;
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

    void Start()
    {
        InitialSetting();
    }

    private void InitialSetting()
    {
        partTimeJob.SetInteractionEndEvent(() => { sofa.enabled = true; });

        sofa.SetInteractionStartEvent(() =>
        {
            CharacterManager character = DataController.instance.GetCharacter(Character.Main);
            character.PickUpCharacter();
            character.transform.position = sofa.transform.GetChild(0).position;
            character.transform.rotation = sofa.transform.GetChild(0).rotation;
            character.anim.SetBool("Seat", true);
        });

        sofa.SetInteractionEndEvent(() =>
        {
            JoystickController.instance.InitializeJoyStick(false);
            speat.StartMoving();
        });

        speat.SetPointEvent(() =>
        {
            DialogueController.instance.SetDialougueEndAction(() =>
            {
                speat.StartMoving();
                StartCoroutine(StartRauMoving());
            });
            DialogueController.instance.StartConversation(jsonFile.text);
        }, 3);
    }

    private IEnumerator StartRauMoving()
    {
        CharacterManager character = DataController.instance.GetCharacter(Character.Main);
        character.PickUpCharacter();
        int index = speat.pointIndex;
        PointData desPoint = speat.point[index];

        character.anim.SetBool("Seat", false);
        yield return new WaitForSeconds(2f);
        character.anim.SetFloat("Speed", rauAniSpeed);

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

