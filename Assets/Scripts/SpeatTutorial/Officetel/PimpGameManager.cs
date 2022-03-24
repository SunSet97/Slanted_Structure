using System;
using System.Collections;
using System.Collections.Generic;
using Move;
using Play;
using UnityEngine;

public class PimpGameManager : MonoBehaviour, IMovable
{
    public bool IsMove { get; set; }

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

        canvasCtrl = CanvasControl.instance_CanvasControl;
        speat = DataController.instance_DataController.GetCharacter(DataController.CharacterType.Main);
    }

    void Update()
    {
        foreach (var t in pimpGuestMoving)
        {
            t.Move(IsMove);
        }
    }
}
