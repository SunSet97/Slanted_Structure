using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// npc에 어사인

[System.Serializable]
public class StorySetting
{
    public Transform wayPointParent; // 웨이포인트 부모
    public List<Transform> wayPoints; // 웨이포인트들
    public List<Vector3> npcRotationVectors; // 회전벡터

}



public class NPCWayPoint : MonoBehaviour
{
    public CharacterManager npcCharacter;
    private Animator npcAnimator;
    int storyIndex = 0;
    bool completeSetStory = false;
    
    public List<StorySetting> storySettings; // 웨이포인트 세팅들, storySettings 개수 == 서로다른 웨이포인트s 개수


    // Start is called before the first frame update
    void Start()
    {
        if(storySettings.Count != 0) SetStory();
    }

    // Update is called once per frame
    void Update()
    {
        if (!completeSetStory && storySettings.Count != 0) SetStory();

        if (npcCharacter) {
            if (!npcAnimator) npcAnimator = npcCharacter.GetComponent<Animator>();



        }

        

           

        
    }

    void SetStory()
    {
        for (int i = 0; i < storySettings.Count; i++)
        {
            if (storySettings[i].wayPointParent)
            {
                SetWayPoints(i);
            }
            else
            {
                print("storySetting[${i}]의 wayPointParent가 할당되지 않음.");
                break;
            }
        }

    }

    void SetWayPoints(int i)
    {
        Transform tmp = storySettings[i].wayPointParent;
        int theNumberOfWayPoints = tmp.childCount; // 웨이포인트 종류 개수
        for (int j = 0; j < theNumberOfWayPoints; j++)
        {
            storySettings[i].wayPoints.Add(tmp.GetChild(j));
        }
        int theNumberOfPoints = storySettings[i].wayPoints.Count;
        int theNumberOfVectors = storySettings[i].npcRotationVectors.Count;
        for (int k = 0; k < theNumberOfPoints - theNumberOfWayPoints - theNumberOfVectors ; k++)
        {
            storySettings[i].npcRotationVectors.Add(new Vector3(0, 0, 0));
        }

    }
    

    IEnumerator Moving() {

        // 애니메이터 ㄱㄱ
        yield return new WaitForSeconds(0.5f);

    }


    #region 기즈모
    private void OnDrawGizmos() {

    }

    #endregion
}
