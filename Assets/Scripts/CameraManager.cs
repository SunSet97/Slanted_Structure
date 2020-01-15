using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public CharMovement charmov;
    public Toggle[] toggle;
    public Transform[] xAxis;
    public Transform[] zAxis;
    public Transform[] inQut;
    public Transform[] outQut;
    private bool isQut = false;
    
    // Update is called once per frame
    void Update()
    {
        OnOffnArray(xAxis, 0);
        OnOffnArray(zAxis, 1);
        OnOffnArray(inQut, 2);
        OnOffnArray(outQut, 3);

        charmov.isPlatformView = toggle[0].isOn || toggle[1].isOn;
        charmov.isOutsideQuarterView = toggle[3].isOn;
        charmov.isInsideQuarterView = toggle[2].isOn;
        if (!toggle[3].isOn)
            Waypoint.FindObjectOfType<Waypoint>().checkedWaypoint = null;
        
    }

    void OnOffnArray(Transform[] array,int index)
    {
        foreach(Transform element in array)
        {
            element.gameObject.SetActive(toggle[index].isOn);
        }
        if (toggle[index].isOn)
            charmov .cam= array[1].GetComponent<Camera>();
    }
}
