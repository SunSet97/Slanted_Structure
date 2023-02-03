using Play;
using UnityEngine;
using Utility.Core;

public class PimpGameManager : MonoBehaviour, IGamePlayable
{
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
        IsPlay = false;
        DataController.instance.currentMap.ui.gameObject.SetActive(false);
        foreach (var t in pimpGuestMoving)
        {
            var animator = t.GetComponent<Animator>();
            animator.SetFloat("Speed", 0.0f);
        }
    }
}
