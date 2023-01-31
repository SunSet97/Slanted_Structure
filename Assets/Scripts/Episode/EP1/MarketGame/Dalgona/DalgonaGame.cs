using System.Collections;
using UnityEngine;
using Play;

public class DalgonaGame : MonoBehaviour, IGamePlayable
{
    public GameObject dalgonaPanel;

    public DalgonaDrager[] dalgona;
    private int index = 0;

    public bool IsPlay { get; set; }

    public void Play()
    {
        if (index >= dalgona.Length)
        {
            Debug.LogError("dalgona index Error");
            return;
        }

        dalgonaPanel.SetActive(true);
        JoystickController.instance.StopSaveLoadJoyStick(true);
        IsPlay = true;
        dalgona[index].Init();
        StartCoroutine(WaitDalgonaEnd());
    }

    public void EndPlay()
    {
        JoystickController.instance.StopSaveLoadJoyStick(false);
        IsPlay = false;
        dalgonaPanel.SetActive(false);
    }

    private IEnumerator WaitDalgonaEnd()
    {
        yield return new WaitUntil(() => dalgona[index].isEnd);
        if (index + 1 < dalgona.Length)
        {
            index++;
            dalgona[index].Init();
            StartCoroutine(WaitDalgonaEnd());
        }
        else
        {
            EndPlay();
        }
    }
}
