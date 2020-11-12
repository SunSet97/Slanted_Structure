using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public GameObject merchant;
    CharacterManager character;

    void Start()
    {
        DataController.instance_DataController.isMapChanged = true;

        CharacterManager[] temp = new CharacterManager[3];
        temp[0] = DataController.instance_DataController.speat;
        temp[1] = DataController.instance_DataController.oun;
        temp[2] = DataController.instance_DataController.rau;

        foreach (CharacterManager cm in temp)
            if (cm.name == DataController.instance_DataController.currentChar.name) character = cm;

    }

    void Update()
    {
        
    }
}
