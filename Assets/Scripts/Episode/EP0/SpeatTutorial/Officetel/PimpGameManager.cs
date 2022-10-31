using Play;
using UnityEngine;

public class PimpGameManager : MonoBehaviour, IGamePlayable
{

    public bool isTalking;

    public PimpGuestMoving[] pimpGuestMoving;
    void Start()
    {
        foreach (var t in pimpGuestMoving)
        {
            t.pimpGameManager = this;
        }
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
