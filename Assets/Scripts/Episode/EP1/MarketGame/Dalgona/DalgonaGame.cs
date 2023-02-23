using System;
using System.Collections;
using UnityEngine;
using Play;
using Utility.Core;

public class DalgonaGame : MonoBehaviour, IGamePlayable
{
    public GameObject dalgonaPanel;

    public DalgonaDrager[] dalgona;
    private int index = 0;

    public bool IsPlay { get; set; }
    public Action ONEndPlay { get; set; }

    public void Play()
    {
        if (index >= dalgona.Length)
        {
            Debug.LogError("dalgona index Error");
            return;
        }

        dalgonaPanel.SetActive(true);
        JoystickController.Instance.StopSaveLoadJoyStick(true);
        IsPlay = true;
        dalgona[index].Init();
        StartCoroutine(WaitDalgonaEnd());
    }

    public void EndPlay()
    {
        JoystickController.Instance.StopSaveLoadJoyStick(false);
        IsPlay = false;
        ONEndPlay?.Invoke();
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
