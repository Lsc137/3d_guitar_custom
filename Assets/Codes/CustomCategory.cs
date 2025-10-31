using UnityEngine;
using System.Collections.Generic;

// 교체할 파츠 (기존과 동일)
public enum TargetPart { Body, Neck, Fretboard, Inlay, Pickguard_inner, Pickguard_outer, Knob, Skybox }

// [추가] 이 카테고리가 수행할 작업 유형
public enum CustomizationType 
{
    MaterialOnly, // 현재 파츠의 재질만 교체
    PrefabSwap    // 파츠 프리팹을 통째로 교체
}

[CreateAssetMenu(fileName = "New Category", menuName = "Guitar/Custom Category")]
public class CustomCategory : ScriptableObject
{
    public string categoryName;
    public TargetPart partToChange;      // 어느 '슬롯'을 변경할지

    [Header("Action Type")]
    public CustomizationType actionType; // 이 카테고리는 '재질만' 바꾸나요? '프리팹'을 바꾸나요?

    public List<CustomOption> options;
}