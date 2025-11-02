using UnityEngine;
using System.Collections.Generic;

// --- (기존 Enum 선언들) ---
public enum TargetPart { Body, Neck, Fretboard, Inlay, Pickguard, Skybox, SkyboxRotation, Head }
public enum CustomizationType { MaterialOnly, PrefabSwap, FloatValue }

// ▼▼▼ [핵심 추가] 이 카테고리가 어떤 종류인지 정의 ▼▼▼
public enum CategoryNodeType
{
    OptionsList,     // 최종 옵션("도트", "블럭")을 보여주는 카테고리
    SubCategoryList  // 다른 카테고리("Shape", "Color")를 보여주는 카테고리
}

[CreateAssetMenu(fileName = "New Category", menuName = "Guitar/Custom Category")]
public class CustomCategory : ScriptableObject
{
    public string categoryName;      // UI에 보일 이름 (예: "Inlay", "Shape")

    // ▼▼▼ [핵심 수정] 카테고리 타입 설정 ▼▼▼
    [Tooltip("이 카테고리가 옵션을 보여줄지, 하위 카테고리를 보여줄지 선택")]
    public CategoryNodeType nodeType = CategoryNodeType.OptionsList;

    // --- 'OptionsList' 타입일 때만 사용 ---
    [Header("Options List Settings")]
    [Tooltip("이 카테고리가 'OptionsList'일 경우, 변경할 파츠")]
    public TargetPart partToChange; 
    [Tooltip("이 카테고리가 'OptionsList'일 경우, 변경할 방식")]
    public CustomizationType actionType;
    [Tooltip("이 카테고리가 'OptionsList'일 경우, 실제 옵션들")]
    public List<CustomOption> options;

    // --- 'SubCategoryList' 타입일 때만 사용 ---
    [Header("Sub-Category List Settings")]
    [Tooltip("이 카테고리가 'SubCategoryList'일 경우, 포함할 하위 카테고리들")]
    public List<CustomCategory> subCategories; 
}