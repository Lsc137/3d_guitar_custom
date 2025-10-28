using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CustomizationManager : MonoBehaviour
{
    // --- 1. [확장] 4개 모델의 셋업 프리팹 ---
    [Header("Guitar Setup Prefabs")]
    public GameObject jazzBassSetupPrefab;
    public GameObject precisionBassSetupPrefab;
    public GameObject stratSetupPrefab;
    public GameObject teleSetupPrefab;

    // --- 2. [확장] 4개 모델의 카테고리 리스트 ---
    [Header("UI & Data")]
    public List<CustomCategory> allCategories_JazzBass;
    public List<CustomCategory> allCategories_PrecisionBass;
    public List<CustomCategory> allCategories_Strat;
    public List<CustomCategory> allCategories_Tele;
    
    [Header("UI Components")]
    public Transform menuContentParent;
    public GameObject categoryHeaderPrefab;
    public GameObject optionButtonPrefab;

    // --- 3. [변경] 슬롯 변수는 private (Start에서 채워짐) ---
    private Transform bodySlot;
    private Transform neckSlot;
    private Transform fretboardSlot;
    private Transform inlaySlot;
    private Transform pickguardSlot;
    // (모델마다 슬롯이 더 필요하면 여기에 private 변수만 추가하고 Start()에서 찾아주세요)


    // --- 4. [수정] Start 함수 ---
    void Start()
    {
        // GameManager에서 선택한 기타 ID 가져오기
        // (GameManager가 없으면 "JazzBass"를 기본값으로 테스트)
        string selectedID = "JazzBass"; 
        if (GameManager.Instance != null)
        {
            selectedID = GameManager.Instance.SelectedGuitarID;
        }

        // ID에 맞는 프리팹과 카테고리 리스트 선택
        GameObject prefabToLoad = null;
        List<CustomCategory> categoriesToLoad = null;

        // [확장] 4개 모델에 대한 분기 처리
        switch (selectedID)
        {
            case "JazzBass":
                prefabToLoad = jazzBassSetupPrefab;
                categoriesToLoad = allCategories_JazzBass;
                break;
            case "PrecisionBass":
                prefabToLoad = precisionBassSetupPrefab;
                categoriesToLoad = allCategories_PrecisionBass;
                break;
            case "Strat":
                prefabToLoad = stratSetupPrefab;
                categoriesToLoad = allCategories_Strat;
                break;
            case "Tele":
                prefabToLoad = teleSetupPrefab;
                categoriesToLoad = allCategories_Tele;
                break;
            default:
                Debug.LogError("알 수 없는 기타 ID입니다: " + selectedID + ". 재즈 베이스를 기본으로 로드합니다.");
                prefabToLoad = jazzBassSetupPrefab;
                categoriesToLoad = allCategories_JazzBass;
                break;
        }

        
        if (prefabToLoad != null)
        {
            // 선택된 기타 프리팹을 씬에 생성 (이것이 'Guitar_Turntable'이 됨)
            GameObject guitarInstance = Instantiate(prefabToLoad, Vector3.zero, Quaternion.identity);

            // [중요] 생성된 프리팹 내부에서 슬롯들을 찾아 private 변수에 할당
            // 이 경로는 모든 기타 프리팹에서 동일해야 합니다! (예: "Guitar_Root/BodySlot")
            bodySlot = guitarInstance.transform.Find("Guitar_Root/BodySlot");
            neckSlot = guitarInstance.transform.Find("Guitar_Root/NeckSlot");
            fretboardSlot = guitarInstance.transform.Find("Guitar_Root/FretboardSlot");
            inlaySlot = guitarInstance.transform.Find("Guitar_Root/InlaySlot");
            pickguardSlot = guitarInstance.transform.Find("Guitar_Root/PickguardSlot");

            // [중요] 카메라 뷰어 스크립트에 회전할 대상을 알려줌
            // (GuitarViewer가 Main Camera에 있다고 가정)
            var viewer = FindObjectOfType<GuitarViewer>();
            if (viewer != null)
            {
                viewer.targetGuitar = guitarInstance.transform;
            }
            else
            {
                Debug.LogError("씬에서 GuitarViewer 스크립트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("'" + selectedID + "'에 해당하는 프리팹이 CustomManager에 할당되지 않았습니다!");
        }

        // 선택된 기타에 맞는 UI 메뉴 빌드
        BuildMenu(categoriesToLoad);
    }

    // --- 5. [수정] BuildMenu 함수 ---
    // (List<CustomCategory> categories를 파라미터로 받도록 수정됨)
    void BuildMenu(List<CustomCategory> categories)
    {
        // (기존 UI 삭제 로직)
        foreach (Transform child in menuContentParent)
        {
            Destroy(child.gameObject);
        }

        if (categories == null)
        {
            Debug.LogWarning("로드할 카테고리가 없습니다!");
            return;
        }
        
        // [수정] 파라미터 'categories'를 사용
        foreach (CustomCategory category in categories)
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

                // --- 컴포넌트 찾기 (프리팹 구조에 맞게 Find 경로를 설정) ---
                RawImage previewImage = optionObj.transform.Find("PreviewGroup/PreviewFill").GetComponent<RawImage>();
                TextMeshProUGUI nameText = optionObj.transform.Find("PreviewGroup/NameText").GetComponent<TextMeshProUGUI>();
                Toggle thisToggle = optionObj.transform.Find("Checkbox").GetComponent<Toggle>();
                Button thisButton = optionObj.GetComponent<Button>();

                // --- 머티리얼에서 텍스처/색상 추출 ---
                Color displayColor = Color.grey;
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
                
                // --- 글자 설정 (텍스트 내용 및 색상) ---
                if (nameText != null)
                {
                    nameText.text = option.optionName;
                    
                    // 텍스처가 있으면 무조건 흰색, 없으면 밝기 분석
                    if (mat != null && mat.mainTexture != null)
                    {
                        nameText.color = Color.white;
                    }
                    else
                    {
                        nameText.color = IsColorDark(displayColor) ? Color.white : Color.black;
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
    
    // --- 6. 하이브리드 ApplyOption 함수 (변경 없음) ---
    void ApplyOption(CustomCategory category, CustomOption option)
    {
        Transform targetSlot = GetSlotFromPart(category.partToChange);
        if (targetSlot == null)
        {
            Debug.LogError("타겟 슬롯이 없습니다: " + category.partToChange);
            return;
        }

        switch (category.actionType)
        {
            case CustomizationType.PrefabSwap:
                if (option.partPrefab == null) return;
                
                foreach (Transform child in targetSlot)
                {
                    Destroy(child.gameObject);
                }
                
                GameObject newPart = Instantiate(option.partPrefab, targetSlot);
                newPart.transform.localPosition = Vector3.zero;
                newPart.transform.localRotation = Quaternion.identity;
                break;

            // --- B. 재질만 교체 ---
            case CustomizationType.MaterialOnly:
                if (option.materialToApply == null)
                {
                    Debug.LogError("적용할 재질이 없습니다: " + option.optionName);
                    return;
                }

                // [수정] 슬롯에 자식이 있는지 먼저 확인
                if (targetSlot.childCount == 0)
                {
                    Debug.LogWarning("재질을 적용할 파츠가 슬롯에 없습니다: " + targetSlot.name);
                    return;
                }

                // [수정] 슬롯 자체(GetComponent)가 아니라,
                // 슬롯의 "첫 번째 자식"에서 MeshRenderer를 찾습니다.
                MeshRenderer targetRenderer = targetSlot.GetChild(0).GetComponentInChildren<MeshRenderer>();
                // (GetComponentInChildren는 자식의 자식에 있어도 찾아줍니다. 가장 안전합니다.)

                if (targetRenderer != null)
                {
                    // 재질 교체!
                    targetRenderer.material = option.materialToApply;
                }
                else
                {
                    // [수정] 오류 메시지 변경
                    Debug.LogError("슬롯의 자식 오브젝트에서 MeshRenderer를 찾을 수 없습니다: " + targetSlot.GetChild(0).name);
                }
                break;
        }
    }

    // --- 7. 슬롯 반환 헬퍼 함수 (변경 없음) ---
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

    // --- 8. 색상 밝기 판별 함수 (변경 없음) ---
    private bool IsColorDark(Color c)
    {
        Color linearColor = c.linear;
        float luminance = (0.2126f * linearColor.r) + (0.7152f * linearColor.g) + (0.0722f * linearColor.b);
        // 임계값 (0.6f 미만이면 '어두운 색'으로 간주)
        return luminance < 0.6f; 
    }
}