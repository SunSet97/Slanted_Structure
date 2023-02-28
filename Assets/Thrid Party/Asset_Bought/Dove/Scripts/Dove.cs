using System.Collections;
using UnityEngine;
using System;

public class Dove : MonoBehaviour
{
    public float speedTime = 0.8f;

    [SerializeField] private int randomNum;
    [SerializeField] private float speed;
    [SerializeField] private float direction;
    [SerializeField] private float stateChange = 4.0f;

    [NonSerialized] public Animator DoveAnimator;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int directionHash = Animator.StringToHash("Direction");
    private readonly int stateHash = Animator.StringToHash("State");

    private bool attackBool = false;

    private void Start()
    {
        DoveAnimator = GetComponent<Animator>();
        DoveAnimator.SetInteger(stateHash, 0);
        StartCoroutine(RandomState());
    }

    private void Update()
    {
        MoveDove();
    }

    public void MoveDove()
    {
        if (DoveAnimator.GetInteger(stateHash) != 0)
        {
            return;
        }

        speed = (Mathf.Sin(Time.time * speedTime) * .6f + 1f) / 2f;
        direction = Mathf.Cos(Time.time * speedTime);
        DoveAnimator.SetFloat(speedHash, speed);
        DoveAnimator.SetFloat(directionHash, direction);
    }

    public void Attack()
    {
        DoveAnimator.SetTrigger("Attack");
    }

    IEnumerator RandomState()
    {
        float randomTime=UnityEngine.Random.Range(1,stateChange);
        var waitForSeconds = new WaitForSeconds(randomTime);
        while (true)
        {
            randomNum = UnityEngine.Random.Range(0, 3);
            DoveAnimator.SetInteger(stateHash, randomNum);

            yield return waitForSeconds;
        }
    }
}
