using System.Collections;
using UnityEngine;

public class JumpWall : JumpInTotal
{
    public Transform obstacleTransform;
    [Range(0, 1)]
    public float slerp_radius;
    protected override void ButtonPressed()
    {
        StartCoroutine(FramePerParameter());
        gameManager.ActiveButton(false);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //if (isActivated)
        //{
        //    return;
        //}
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        //if (isActivated)
        //{
        //    return;
        //}
        base.OnTriggerExit(other);
    }

    private IEnumerator FramePerParameter()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        CharacterManager platformer_char = DataController.instance.GetCharacter(Data.CustomEnum.Character.Main);
        platformer_char.PickUpCharacter();
        Vector3 startposition = platformer_char.transform.position;
        Vector3 middle_position = (startposition + obstacleTransform.position) / 2;

        float d = 1f;
        float t = 0f;
        while (t <= 1f)
        {
            t = ((platformer_char.transform.position - startposition).magnitude / (obstacleTransform.position - startposition).magnitude);
            Vector3 slerp_position = Vector3.Slerp(transform.position, obstacleTransform.position, t);
            var direction = Vector3.Lerp(middle_position, slerp_position, slerp_radius) - platformer_char.transform.position;

            var characterController = platformer_char.GetComponent<CharacterController>();
            characterController.Move(direction * Time.fixedDeltaTime * d);

            d += Time.fixedDeltaTime * 20f;
            yield return waitForFixedUpdate;
        }

        platformer_char.PutDownCharacter();
    }
}
