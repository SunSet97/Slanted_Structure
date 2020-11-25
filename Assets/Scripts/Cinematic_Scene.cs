using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinematic_Scene : MonoBehaviour
{
   
        Animation_Controller acSpeat;
        Animation_Controller acRau;
        Animation_Controller acOun;
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

        // Start is called before the first frame update
        void Start()
        {
            Animation_Controller acSpeat = DataController.instance_DataController.speat.Anim_Controller;
            Animation_Controller acRau = DataController.instance_DataController.rau.Anim_Controller;
            Animation_Controller acOun = DataController.instance_DataController.oun.Anim_Controller;
        }

        // Update is called once per frame 
        void Update()
        {

        }

        //씨네마틱에서 선택된 캐릭터 감정 입력받는 곳
        public void Emotion_input(int emotion, string Main_char)//선택한 캐릭터의 감정
        {
            if (Main_char == "Speat")
            {
                acSpeat.emotion_present = (Animation_Controller.Emotion)emotion;
            }
            if (Main_char == "Rau")
            {
                acRau.emotion_present = (Animation_Controller.Emotion)emotion;
            }
            if (Main_char == "Oun")
            {
                acOun.emotion_present = (Animation_Controller.Emotion)emotion;
            }

        }



    
}
