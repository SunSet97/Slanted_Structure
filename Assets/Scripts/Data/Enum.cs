using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEnum
{
    public enum EXPRESSION
    {
        IDLE = 0, LAUGH, SAD, CRY, ANGRY, SURPISE, PANIC, SUSPICION, FEAR, CURIOUS, ANIM_ONE, ANIM_TWO
    }
    
    public enum TYPE
    {
        NONE = 0, ANIMATION = 1, DIALOGUE = 2, TEMP = 3, TEMPEND = 4, TaskReset = 5, NEW = 6, THEEND = 7, Play = 8, FadeOut = 9, FadeIn = 10, Cinematic = 11, TempDialogueEnd = 99
    }
    
    public enum CameraViewType
    {
        FollowCharacter, FixedView         
    }
    
}
