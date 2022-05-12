namespace Data
{
    public static class CustomEnum
    {
        public enum Expression
        {
            IDLE = 0, LAUGH, SAD, CRY, ANGRY, SURPISE, PANIC, SUSPICION, FEAR, CURIOUS, ANIM_ONE, ANIM_TWO
        }
    
        public enum TaskContentType
        {
            NONE = 0, ANIMATION = 1, DIALOGUE = 2, TEMP = 3, TEMPEND = 4, TaskReset = 5, NEW = 6, THEEND = 7, Play = 8, FadeOut = 9, FadeIn = 10, Cinematic = 11, TempDialogue = 99
        }
    
        public enum CameraViewType
        {
            FollowCharacter, FixedView         
        }
    
        public enum InteractionPlayType
        {
            None = -1,
            Potal,
            Animation,
            Dialogue,
            CameraSetting,
            Interact,
            Task,
            Game
        }
        public enum InteractionMethod // 인터렉션 오브젝트 터치했는지 안했는지 감지 기능 필요
        {
            Touch,
            Trigger,
            No
        }
    }
}
