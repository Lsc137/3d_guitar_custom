using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CustomOption
{
    public string optionName;
    public Sprite thumbnailIcon;

    [Header("Option Data")]
    // 'MaterialOnly' 타입일 때 사용할 재질
    public Material materialToApply; 
    
    // 'PrefabSwap' 타입일 때 사용할 프리팹
    public GameObject partPrefab;    
}