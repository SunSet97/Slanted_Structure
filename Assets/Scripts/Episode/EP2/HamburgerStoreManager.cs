using System.Collections;
using UnityEngine;
using Utility.System;
using static Data.CustomEnum;

public class HamburgerStoreManager : MonoBehaviour
{
    public TextAsset jsonFile;
    [SerializeField]
    private NpcWayPoint speat;

    [Range(0, 1f)] public float rauAniSpeed;

    public InteractionObj_stroke partTimeJob;
    public InteractionObj_stroke sofa;

    void Start()
    {
        InitialSetting();
    }

    private void InitialSetting()
    {
        partTimeJob.SetDialogueEndEvent(() => { sofa.enabled = true; });

        sofa.SetDialogueStartEvent(() =>
        {
            CharacterManager character = DataController.instance.GetCharacter(Character.Main);
            character.PickUpCharacter();
            character.transform.position = sofa.transform.GetChild(0).position;
            character.transform.rotation = sofa.transform.GetChild(0).rotation;
            character.anim.SetBool("Seat", true);
        });

        sofa.SetDialogueEndEvent(() =>
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

