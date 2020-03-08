using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DialogueData
{
    //public string charName;
    public string[] dialogue;
    public string[] choice;

    public bool[] isSceneChange = { false };
    public string[] nextScene;
    public int[] storyParam = { 0 }; // 스토리 큰 단위 매개변수, 1부터시작
    public int[] branchParam = { 0 }; // 스토리 분기 매개변수, 1부터 시작
    public int[] intimacyPram_speat = { 0 };
    public int[] intimacyPram_oun = { 0 };
    public int[] selfEstmPram = { 0 }; 
    public int[] intimacyCrt_speat = { 0 };
    public int[] intimacyCrt_oun = { 0 };
    public int[] selfEstmCrt;

    //// 선택지 매개변수
    //public bool isSceneChange0 = false; // 씬 전환 여부
    //public string nextScene0;
    //public int storyParam0 = 0;
    //public int intimacyPram0 = 0;
    //public int selfEstmPram0 = 0;
    //public int intimacyCrt0 = 0;
    //public int selfEstmCrt0 = 0;


    //public bool isSceneChange1 = false; // 씬 전환 여부
    //public string nextScene1;
    //public int storyParam1 = 0;
    //public int intimacyPram1 = 0;
    //public int selfEstmPram1 = 0;
    //public int intimacyCrt1 = 0;
    //public int selfEstmCrt1 = 0;

    //public bool isSceneChange2 = false; // 씬 전환 여부
    //public string nextScene2;
    //public int storyParam2 = 0;
    //public int intimacyPram2 = 0;
    //public int selfEstmPram2 = 0;
    //public int intimacyCrt2 = 0;
    //public int selfEstmCrt2= 0;

    //public bool isSceneChange3 = false; // 씬 전환 여부
    //public string nextScene3;
    //public int storyParam3 = 0;
    //public int intimacyPram3 = 0;
    //public int selfEstmPram3 = 0;
    //public int intimacyCrt3 = 0;
    //public int selfEstmCrt3 = 0;
}
