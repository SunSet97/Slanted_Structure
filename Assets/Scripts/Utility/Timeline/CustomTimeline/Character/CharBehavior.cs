using System;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Core;
using Utility.Dialogue;

namespace Utility.Timeline.CustomTimeline.Character
{
    [Serializable]
    public class CharBehavior : PlayableBehaviour
    {
        //[SerializeField] private CustomEnum.Character who;

#pragma warning disable 0649
        [SerializeField] private Expression expression;
#pragma warning restore 0649



        //private CustomEnum.Expression previousEmotion = CustomEnum.Expression.NONE;


        //#region 일반 캐릭터에 사용

        //public override void OnBehaviourPlay(Playable playable, FrameData info)
        //{
        //    previousEmotion = DataController.instance.GetCharacter(who).emotion;
        //    DataController.instance.GetCharacter(who).SetEmotion(expression);
        //}

        //// 첫 시작 시, 자동으로 실행됨
        //public override void OnBehaviourPause(Playable playable, FrameData info)
        //{
        //    if (previousEmotion == CustomEnum.Expression.NONE) return;
        
        //    DataController.instance.GetCharacter(who).SetEmotion(previousEmotion);
        //    previousEmotion = CustomEnum.Expression.NONE;
        //}

        //#endregion
    
        //시네마틱 캐릭터에 사용
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            var cinCharacter = ((CharacterBindingHelper)playerData).who;

            var character = DataController.Instance.GetCharacter(cinCharacter);
            character.Emotion = expression;
        }
    }
}
