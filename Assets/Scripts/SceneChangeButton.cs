using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChangeButton : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        SceneController.instance_SceneController.OpenScene(sceneName);
    }
}
