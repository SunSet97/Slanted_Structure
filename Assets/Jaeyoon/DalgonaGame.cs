using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Play;
public class DalgonaGame : MonoBehaviour, IPlayable
{
    public dragimage[] dalgona;
    private int index;
    public bool IsPlay { get; set; }

    public void EndPlay()
    {
        IsPlay = false;
        dalgona[index].gameObject.SetActive(false);
        index++;
    }

    public void Play()
    {
        if(index >= dalgona.Length)
        {
            Debug.LogError("dalgona index Error");
        }
        IsPlay = true;
        dalgona[index].gameObject.SetActive(true);
    }


}
