using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    public Transform mapUI;
    
    public bool isInConverstation;

    public static CanvasControl Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;   
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
