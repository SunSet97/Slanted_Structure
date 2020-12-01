using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinematic_Scene : MonoBehaviour
{
    #region 싱글톤
    //인스턴스화 추후 비주얼 노벨에서 접근 가능하도록
    private static Cinematic_Scene instance = null;

    public static Cinematic_Scene instance_Cinematic_Scene
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    //씨네마틱에서 선택된 캐릭터 감정 입력받는 곳
    public void Emotion_input(int emotion, string Main_char)//선택한 캐릭터의 감정
    {

        if (Main_char == "Speat")
        {
            DataController.instance_DataController.speat.emotion = (CharacterManager.Emotion)emotion;
        }
        if (Main_char == "Rau")
        {
            DataController.instance_DataController.rau.emotion = (CharacterManager.Emotion)emotion;
        }
        if (Main_char == "Oun")
        {
            DataController.instance_DataController.oun.emotion = (CharacterManager.Emotion)emotion;
        }

    }
}