using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    public Transform mapUI;
    
    public bool isInConverstation;

    public static CanvasControl instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;   
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
    }
}
