using System;
using System.Collections;
using System.Collections.Generic;
using Move;
using Play;
using UnityEngine;
using static Data.CustomEnum;

public class PimpGameManager : MonoBehaviour, IPlayable
{

    public bool isTalking = false;
    public CanvasControl canvasCtrl;
    public CharacterManager speat; // 스핏

    public PimpGuestMoving[] pimpGuestMoving;

    public readonly int speedHash = Animator.StringToHash("Speed");

    void Start()
    {
        foreach (var t in pimpGuestMoving)
        {
            t.pimpGameManager = this;
        }

        canvasCtrl = CanvasControl.Instance;
        speat = DataController.instance.GetCharacter(Character.Main);
    }

    void Update()
    {
        foreach (var t in pimpGuestMoving)
        {
            t.Move(IsPlay);
        }
    }

    public bool IsPlay { get; set; }
    public void Play()
    {
        IsPlay = true;
        foreach (var t in pimpGuestMoving)
        {
            t.Think();
        }
    }

    public void EndPlay()
    {
        
    }
}
