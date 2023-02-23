using System.Collections;
using UnityEngine;
using Utility.Core;

public class jumpObstacle : JumpInTotal
{
    public Transform obstacleTransform;
    public float speed;
    public AnimationCurve jumpCurve;
    public float sec;
    protected override void ButtonPressed()
    {
        StartCoroutine(FramePerParameter());
        gameManager.ActiveButton(false);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    private IEnumerator FramePerParameter()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        CharacterManager platformer_char = DataController.Instance.GetCharacter(Data.CustomEnum.Character.Main);
        platformer_char.PickUpCharacter();

        platformer_char.RotateCharacter2D(-1f);

        platformer_char.anim.SetBool("Jump", true);
        Vector3 startposition = platformer_char.transform.position;

        float t = 0f;
        while (t <= 1f)
        {
            t += Time.fixedDeltaTime / sec;

            float curvepercent = jumpCurve.Evaluate(t);
            var dest_position = Vector3.LerpUnclamped(startposition, obstacleTransform.position, t);
            dest_position.y = Mathf.LerpUnclamped(startposition.y, obstacleTransform.position.y, curvepercent);

            platformer_char.transform.position = dest_position;

            yield return waitForFixedUpdate;
        }

        platformer_char.anim.SetBool("Jump", false);
        platformer_char.PutDownCharacter();

    }
}