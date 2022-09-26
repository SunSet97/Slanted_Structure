using System;
using System.Linq;
using UnityEngine;

public class assetbundletestset : MonoBehaviour
{
    private AssetBundle _assetBundle;

    public string path;

    private void Start()
    {
        LoadAssetBundle(path);
    }
    
    void LoadAssetBundle(string _path)
    {
        // _assetBundle = AssetBundle.LoadFromFile(Application.dataPath + _path);
        //
        // Debug.Log(_assetBundle == null ? "Fail to load" : "Success to load");
        // var objs = _assetBundle.LoadAllAssets();
        // foreach (var t in objs)
        // {
        //     Debug.Log(t);
        // }
    }
    
    // private void Start()
    // {
    //     LoadAssetBundle(path);
    // }
    //
    // void LoadAssetBundle(string _path)
    // {
    //     _assetBundle = AssetBundle.LoadFromFile(Application.dataPath + _path);
    //     
    //     Debug.Log(_assetBundle == null ? "Fail to load" : "Success to load");
    //     var objs = _assetBundle.LoadAllAssets<GameObject>();
    //     foreach (var t in objs)
    //     {
    //         Debug.Log(t.name);
    //     }
    //     
    //     
    //     Debug.Log(_assetBundle == null ? "Fail to load" : "Success to load");
    //     var objss = _assetBundle.LoadAllAssets<GameObject>();
    //     
    //     var patterns = new GameObject[3][];
    //     for (var idx = 0; idx < patterns.Length + 1; idx++)
    //     {
    //         GameObject[] t = objss.Where(item => item.name.Substring(7, 1) == idx.ToString()).Distinct().ToArray();
    //         foreach (var VARIABLE in t)
    //         {
    //             Debug.Log(VARIABLE);
    //         }
    //
    //         Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
    //     }
    // }
}
