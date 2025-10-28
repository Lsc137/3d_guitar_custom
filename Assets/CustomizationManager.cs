using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CustomizationManager : MonoBehaviour
{
    // --- 1. '슬롯' 참조 (프리팹 교체 방식과 동일) ---
    [Header("Part Slots")]
    public Transform bodySlot;
    public Transform neckSlot;
    public Transform fretboardSlot;
    public Transform inlaySlot;
    public Transform pickguardSlot;

    // --- 2. UI & 데이터 (기존과 동일) ---
    [Header("UI & Data")]
    public List<CustomCategory> allCategories;
    public Transform menuContentParent;
    public GameObject categoryHeaderPrefab;
    public GameObject optionButtonPrefab;

    void Start()
    {
        BuildMenu();
    }

    private bool IsColorDark(Color c)
    {
        // 색상을 선형 공간으로 변환 (정확한 밝기 계산을 위해)
        Color linearColor = c.linear;

        // 표준 휘도(Luminance) 계산식
        // Y = 0.2126 * R + 0.7152 * G + 0.0722 * B
        float luminance = (0.2126f * linearColor.r) + (0.7152f * linearColor.g) + (0.0722f * linearColor.b);

        // 휘도 임계값 (0.4f 미만이면 '어두운 색'으로 간주)
        // 이 값은 0.3 ~ 0.5 사이에서 조절하며 테스트해볼 수 있습니다.
        return luminance < 0.4f; 
    }

// --- 4. 아코디언 메뉴 생성 함수 (완전판) ---
    void BuildMenu()
{
    // (기존 UI 삭제)
    foreach (Transform child in menuContentParent)
    {
        Destroy(child.gameObject);
    }

    // 모든 카테고리 데이터를 순회
    foreach (CustomCategory category in allCategories)
    {
        // A. 카테고리 헤더 생성
        GameObject headerObj = Instantiate(categoryHeaderPrefab, menuContentParent);
        headerObj.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;

        // 이 카테고리에 속한 '옵션 버튼'과 '토글'을 기억할 리스트
        List<GameObject> optionObjectsInThisCategory = new List<GameObject>();
        List<Toggle> togglesInThisCategory = new List<Toggle>();

        // B. 옵션 버튼들 미리 생성
        foreach (CustomOption option in category.options)
        {
            GameObject optionObj = Instantiate(optionButtonPrefab, menuContentParent);

            // --- 컴포넌트 찾기 (★ 경로 수정됨) ---
            // 프리팹 구조에 맞게 Find 경로를 설정합니다.
            RawImage previewImage = optionObj.transform.Find("PreviewGroup/PreviewFill").GetComponent<RawImage>();
            TextMeshProUGUI nameText = optionObj.transform.Find("PreviewGroup/NameText").GetComponent<TextMeshProUGUI>();
            Toggle thisToggle = optionObj.transform.Find("Checkbox").GetComponent<Toggle>();
            Button thisButton = optionObj.GetComponent<Button>();

            // --- 머티리얼에서 텍스처/색상 추출 ---
            Color displayColor = Color.grey; // 기본색 (오류 방지)
            Material mat = option.materialToApply;
            if (mat != null)
            {
                if (mat.mainTexture != null)
                {
                    previewImage.texture = mat.mainTexture;
                    previewImage.color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
                }
                else
                {
                    previewImage.texture = null; 
                    if (mat.HasProperty("_BaseColor")) previewImage.color = mat.GetColor("_BaseColor");
                    else if (mat.HasProperty("_Color")) previewImage.color = mat.GetColor("_Color");
                }
                displayColor = previewImage.color;
            }

            // --- ★ 글자 설정 (새로 추가된 핵심 로직) ★ ---
            if (nameText != null)
                {
                    // 1. 텍스트 내용 설정
                    nameText.text = option.optionName;

                    // 2. 텍스트 색상 결정
                    if (mat != null && mat.mainTexture != null)
                    {
                        // 텍스처가 있으면 (나무 무늬 등)
                        // 텍스처가 어둡다는 가정 하에 무조건 흰색 글씨
                        nameText.color = Color.white;
                    }
                    else
                    {
                        // 텍스처가 없으면 (단색)
                        // 단색의 밝기를 분석해서 결정
                        if (IsColorDark(displayColor))
                        {
                            nameText.color = Color.white; // 배경이 어두우면 흰 글씨
                        }
                        else
                        {
                            nameText.color = Color.black; // 배경이 밝으면 검은 글씨
                        }
                    }
                }

            // --- 체크박스 로직 설정 ---
            togglesInThisCategory.Add(thisToggle);
            thisToggle.interactable = false; 

            // --- 버튼 클릭 이벤트 연결 ---
            CustomCategory currentCategory = category;
            CustomOption currentOption = option;

            thisButton.onClick.AddListener(() =>
            {
                ApplyOption(currentCategory, currentOption);
                foreach (Toggle t in togglesInThisCategory)
                {
                    t.isOn = (t == thisToggle);
                }
            });

            // 생성 직후 숨기고, 버튼 리스트에 추가
            optionObj.SetActive(false);
            optionObjectsInThisCategory.Add(optionObj);

        } // (옵션 루프 끝)

        // --- D. 카테고리 헤더 버튼에 '접기/펴기' 기능 연결 ---
        headerObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            foreach (GameObject btnObj in optionObjectsInThisCategory)
            {
                btnObj.SetActive(!btnObj.activeSelf);
            }
        });

    } // (카테고리 루프 끝)
}
// --- 5. [핵심] 하이브리드 ApplyOption 함수 ---
    void ApplyOption(CustomCategory category, CustomOption option)
    {
        // 1. 어느 슬롯을 타겟으로 할지 찾기
        Transform targetSlot = GetSlotFromPart(category.partToChange);
        if (targetSlot == null)
        {
            Debug.LogError("타겟 슬롯이 없습니다: " + category.partToChange);
            return;
        }

        // 2. 카테고리의 'actionType'에 따라 다른 작업 수행
        switch (category.actionType)
        {
            // --- A. 프리팹 교체 (이 로직은 기존과 동일) ---
            case CustomizationType.PrefabSwap:
                if (option.partPrefab == null)
                {
                    Debug.LogError("교체할 프리팹이 없습니다: " + option.optionName);
                    return;
                }
                
                // 기존 파츠 삭제
                if (targetSlot.childCount > 0)
                {
                    // [참고] 이 로직은 bonenut 같은 자식들도 모두 삭제합니다.
                    // 만약 자식을 남겨두고 싶다면 로직이 더 복잡해집니다.
                    foreach (Transform child in targetSlot)
                    {
                        Destroy(child.gameObject);
                    }
                }
                // 새 파츠 생성
                GameObject newPart = Instantiate(option.partPrefab, targetSlot);
                newPart.transform.localPosition = Vector3.zero;
                newPart.transform.localRotation = Quaternion.identity;
                break;

            // --- B. 재질만 교체 (★ 여기가 수정되었습니다 ★) ---
            case CustomizationType.MaterialOnly:
                if (option.materialToApply == null)
                {
                    Debug.LogError("적용할 재질이 없습니다: " + option.optionName);
                    return;
                }
                
                // [수정] 자식(GetChild(0))이 아닌, 슬롯 자체(targetSlot)에서 렌더러를 찾습니다.
                MeshRenderer targetRenderer = targetSlot.GetComponent<MeshRenderer>();

                // [수정] 만약 슬롯 자체에 렌더러가 없고,
                // "특정" 자식이 렌더러를 가졌다면 GetComponentInChildren을 쓸 수 있지만,
                // 지금은 슬롯 자체에 렌더러가 있다고 가정합니다.
                
                if (targetRenderer != null)
                {
                    // 재질 교체!
                    targetRenderer.material = option.materialToApply;
                }
                else
                {
                    // [수정] 오류 메시지 변경
                    Debug.LogError("슬롯 오브젝트 자체에서 MeshRenderer를 찾을 수 없습니다: " + targetSlot.name);
                }
                break;
        }
    }

    // --- 6. 파츠 타입에 맞는 슬롯을 반환하는 헬퍼 함수 ---
    Transform GetSlotFromPart(TargetPart part)
    {
        switch (part)
        {
            case TargetPart.Body: return bodySlot;
            case TargetPart.Neck: return neckSlot;
            case TargetPart.Fretboard: return fretboardSlot;
            case TargetPart.Inlay: return inlaySlot;
            case TargetPart.Pickguard: return pickguardSlot;
            default: return null;
        }
    }
}