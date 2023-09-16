namespace Data
{
    public static class CustomEnum
    {
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
            Other,
            Waypoint
        }

        public enum Expression
        {
            None = -1,
            Idle = 0,
            Laugh,
            Sad,
            Cry,
            Angry,
            Surprise,
            Panic,
            Suspicion,
            Fear,
            Curious
        }

        public enum TaskContentType
        {
            None = 0,
            Animation = 1,
            Dialogue = 2,
            Temp = 3,
            TempEnd = 4,
            TaskReset = 5,
            New = 6,
            TheEnd = 7,
            Play = 8,
            FadeIn = 9,
            FadeOut = 10,
            Cinematic = 11,
            ImmediatelyTemp = 12,
            Clear = 13,
            TempDialogue = 99
        }
        
        public enum OutlineColor
        {
            Red,
            Magenta,
            Yellow,
            Green,
            Blue,
            Grey,
            Black,
            White
        }

        public enum InteractionPlayType
        {
            None = -1,
            Portal,
            Animation,
            Dialogue,
            Task,
            Game,
            Cinematic,
            // FadeOut
        }

        public enum InteractionMethod
        {
            Touch,
            Trigger,
            No,
            OnChangeMap
        }

        public enum JoystickAreaType
        {
            Default,
            None,
            Full
        }

        public enum Swipe
        {
            Left,
            Right,
            Down,
            None
        }
        
        public enum TimelineClipType
        {
            None,
            Action,
            Emotion,
            Both
        }
    }
}
