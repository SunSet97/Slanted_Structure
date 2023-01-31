using System.Collections;
using UnityEngine;

public class JumpWallClimbing : JumpInTotal
{
    public Transform obstacleTransform;
    [Range(0, 1)]
    public float slerp_radius;
    protected override void ButtonPressed()
    {
        StartCoroutine(FramePerParameter());
        gameManager.ActiveButton(false);
        isActivated = true;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isActivated)
        {
            return;
        }
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (isActivated)
        {
            return;
        }
        base.OnTriggerExit(other);
    }

    private IEnumerator FramePerParameter()
    {
        CharacterManager platformer_char = DataController.instance.GetCharacter(Data.CustomEnum.Character.Main);
        platformer_char.PickUpCharacter();
        Vector3 startposition = platformer_char.transform.position;

        float t = 0;
        while (t <= 1)
        {
            //platformer_char.transform.position = Vector3.Lerp(startposition, , lerp_Route);

            t += Time.deltaTime;
            yield return null;
        }

        platformer_char.PutDownCharacter();
    }
}
