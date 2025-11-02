using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DependentChange
{
    [Tooltip("부모 프리팹 내부의 자식 경로 (예: Dependent/Pickguard_outer)")]
    public string childPath;
    public CustomizationType actionType; // 어떻게 바꿀지 (예: MaterialOnly)
    public Material materialToApply;     // 적용할 재질
    //public GameObject prefabToApply;     // 적용할 프리ab
}

[System.Serializable]
public class CustomOption
{
    public string optionName;
    public Sprite thumbnailIcon;

    [Header("Prefab Swap")]
    [Tooltip("이 옵션이 프리팹을 통째로 교체하는 경우 (예: 픽가드 모양 변경)")]
    public GameObject partPrefab;

    [Header("Main Child Path")]
    [Tooltip("메인 자식의 경로")]
    public string mainChildPath = "";

    [Header("Material Only Change")]
    [Tooltip("메인 자식에게 적용할 재질 (예: 톨토이스)")]
    public Material materialToApply;

    [Header("Dependent Changes")]
    [Tooltip("이 옵션 적용 시 함께 변경될 다른 자식 파츠들")]
    public List<DependentChange> dependentChanges;
}