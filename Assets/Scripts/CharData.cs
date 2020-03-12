using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CharData
{
    public GameObject respawnLocation;

    //public int[] data = new int[3]; // 데이터 파일 이름을 저장 
    public int[] story;
    public List<string> item = new List<string>();

    public int date = 0;
    public int pencilCnt = 0;
    public int endingCnt = 0;

    public Vector3 endPosition; // 게임 종료 시 캐릭터 위치

    public string currentScene; // 씬 바뀔 때마다 바꿔줌

}
