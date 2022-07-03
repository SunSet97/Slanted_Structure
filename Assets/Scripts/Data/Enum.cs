using System;
using UnityEngine;
using UnityEngine.UI;

namespace Data
{
    [Serializable]
    public struct Ending
    {
        public string ending_id;
        public string ending_content;
        public Color color;

        public Image image;
        public CustomEnum.EndingType endingType;

    }
    public static class CustomEnum
    {
        public enum EndingType
        {
            Normal,
            Bad,
            Special,
            Happy
        }
        public enum Character
        {
            Main = -1,
            Speat,
            Oun,
            Rau,
            Speat_Child,
            Speat_Adult,
            Speat_Adolescene
        }
        public enum JoystickInputMethod
        {
            OneDirection,
            AllDirection,
            Other
        }
        public enum Expression
        {
            NONE = -1, IDLE = 0, LAUGH, SAD, CRY, ANGRY, SURPISE, PANIC, SUSPICION, FEAR, CURIOUS
        }
    
        public enum TaskContentType
        {
            NONE = 0, ANIMATION = 1, DIALOGUE = 2, TEMP = 3, TEMPEND = 4, TaskReset = 5, NEW = 6, THEEND = 7, Play = 8, FadeIn = 9, FadeOut = 10, Cinematic = 11, EndingChoice = 12, Clear = 13, TempDialogue = 99
        }
    
        public enum CameraViewType
        {
            FollowCharacter, FixedView         
        }
        public enum OutlineColor
        {
            red,
            magenta,
            yellow,
            green,
            blue,
            grey,
            black,
            white
        }
        public enum InteractionPlayType
        {
            None = -1,
            Potal,
            Animation,
            Dialogue,
            Task,
            Game,
            Cinematic
        }
        public enum InteractionMethod // 인터렉션 오브젝트 터치했는지 안했는지 감지 기능 필요
        {
            Touch,
            Trigger,
            No
        }
    }
}
