using System.Collections;
using UnityEngine;
using Play;

public class DalgonaGame : MonoBehaviour, IGamePlayable
{
    public DalgonaDrager[] dalgona;
    private int index = 0;
    
    public bool IsPlay { get; set; }

    private void Start()
    {
        Play();
    }

    public void EndPlay()
    {
        if (index + 1 < dalgona.Length)
        {
            index++;
            dalgona[index].Init();
            StartCoroutine(WaitDalgonaEnd());
        }
        else
        {
            JoystickController.instance.StopSaveLoadJoyStick(false);
            IsPlay = false;
        }
    }

    public void Play()
    {
        if(index >= dalgona.Length)
        {
            Debug.LogError("dalgona index Error");
            return;
        }
        JoystickController.instance.StopSaveLoadJoyStick(true);
        IsPlay = true;
        dalgona[index].Init();
        StartCoroutine(WaitDalgonaEnd());
    }

    private IEnumerator WaitDalgonaEnd()
    {
        yield return new WaitWhile(() => dalgona[index].gameObject.activeSelf);
        EndPlay();
    }
}
