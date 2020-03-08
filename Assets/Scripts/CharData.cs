using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CharData
{
    public GameObject respawnLocation;

    //public int[] data = new int[3]; // 데이터 파일 이름을 저장 
    public int story = 1;
    public int story_branch = 1;
    public int dialogue_index = 1;
    public List<string> item = new List<string>();

    public int date = 0;
    public int pencilCnt = 0;
    public int endingCnt = 0; 
    public int selfEstm = 0; // 자존감 
    public int intimacy_speat = 0; // 스핏 친밀도
    public int intimacy_oun = 0; // 오운 친밀도

    public Vector3 endPosition; // 게임 종료 시 캐릭터 위치

    public string currentScene; // 씬 바뀔 때마다 바꿔줌

}
