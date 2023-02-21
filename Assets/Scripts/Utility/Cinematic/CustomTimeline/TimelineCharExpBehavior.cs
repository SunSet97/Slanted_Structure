using System;
using Data;
using UnityEngine;
using UnityEngine.Playables;
using Utility.Cinematic;

[Serializable]
public class TimelineCharExpBehavior : PlayableBehaviour
{
    //[SerializeField] private CustomEnum.Character who;

    [SerializeField] private CustomEnum.Expression expression;



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
    
    #region 시네마틱 캐릭터에 사용
    
    // 시네마틱 캐릭터를 사용하는 경우 사용
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);
        var cin_character = playerData as CinematicCharacter;
        cin_character.ExpressionSetting(expression);
    }
    
    #endregion
}
