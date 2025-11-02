using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CustomizationManager : MonoBehaviour
{

    public Vector3 guitarSpawnPoint = new Vector3(0, 0, 0);
    public Vector3 guitarSpawnRotation = new Vector3(0, 0, 0);
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

    [Tooltip("가장 상위 카테고리(레벨 1)의 기본 색상입니다.")]
    public Color baseCategoryColor = new Color(0.9f, 0.9f, 0.9f, 1f); // 옅은 회색  
    [Tooltip("한 단계 내려갈 때마다 얼마나 밝아질지 (0.0 ~ 1.0)")]
    public float brightnessStep = 0.2f; // 30%씩 밝아짐

    // --- 3. [변경] 슬롯 변수는 private (Start에서 채워짐) ---

    [Header("Lighting Rigs")]
    public List<GameObject> allLightingRigs;
    private Transform BodySlot;
    private Transform NeckSlot;
    private Transform FretboardSlot;
    private Transform InlaySlot;
    private Transform PickguardSlot;

    private MeshRenderer headRenderer;    
    private Material currentSkyboxMaterial;

    // (모델마다 슬롯이 더 필요하면 여기에 private 변수만 추가하고 Start()에서 찾아주세요)

    // --- 4. [수정] Start 함수 ---
    [System.Obsolete]
    void Start()
    {
        // 1. GameManager에서 ID 가져오기 (기존과 동일)
        string selectedID = "JazzBass"; 
        if (GameManager.Instance != null)
        {
            selectedID = GameManager.Instance.SelectedGuitarID;
        }

        // 2. 프리팹과 카테고리 리스트 선택 (기존과 동일)
        GameObject prefabToLoad = null;
        List<CustomCategory> categoriesToLoad = null;

        switch (selectedID)
        {
            case "JazzBass":
                prefabToLoad = jazzBassSetupPrefab;
                categoriesToLoad = allCategories_JazzBass;
                break;
            // ... (PrecisionBass, Strat, Tele 등 나머지 case는 그대로) ...
            default:
                prefabToLoad = jazzBassSetupPrefab;
                categoriesToLoad = allCategories_JazzBass;
                break;
        }

        // 3. 기타 프리팹 생성 및 초기화 (기존과 동일)
        if (prefabToLoad != null)
        {
            GameObject guitarInstance = Instantiate(prefabToLoad, guitarSpawnPoint, Quaternion.Euler(guitarSpawnRotation));
            GuitarViewer viewer = FindObjectOfType<GuitarViewer>();
            Transform cameraTargetTransform = guitarInstance.transform.Find("_CameraTarget");

            if (viewer != null && cameraTargetTransform != null)
            {
                viewer.SetTargetAndInitialize(guitarInstance.transform, cameraTargetTransform);
            }
            else
            {
                Debug.LogError("_CameraTarget을 찾을 수 없거나 GuitarViewer가 씬에 없습니다!");
            }
            

            BodySlot = guitarInstance.transform.Find("BodySlot");
            NeckSlot = guitarInstance.transform.Find("NeckSlot");
            PickguardSlot = guitarInstance.transform.Find("PickguardSlot");
            InlaySlot = guitarInstance.transform.Find("InlaySlot");
            FretboardSlot = guitarInstance.transform.Find("FretboardSlot");

        }

        // 4. [★핵심 수정★] 아코디언 메뉴 빌드
        // (기존 currentCategories = ... 및 DisplayCategories(...) 호출 삭제)
        
        // 4A. 메뉴 초기화
        foreach (Transform child in menuContentParent)
        {
            Destroy(child.gameObject);
        }
        
        // 4B. 아코디언 메뉴 생성 시작
        BuildFoldingMenu(categoriesToLoad, menuContentParent, baseCategoryColor);
    }
/// <summary>
    /// 아코디언 메뉴를 재귀적으로 빌드합니다. (최대 3레벨: Cat -> SubCat -> Option)
    /// </summary>
    /// <param name="categories">빌드할 카테고리 리스트</param>
    /// <param name="parent">이 버튼들이 생성될 부모 Transform</param>
    void BuildFoldingMenu(List<CustomCategory> categories, Transform parent, Color currentColor)
    {
        if (categories == null) return;

        foreach (CustomCategory category in categories)
        {
            // --- 1. '카테고리 헤더' 버튼 생성 (예: "Inlay" 또는 "Shape") ---
            GameObject headerObj = Instantiate(categoryHeaderPrefab, parent);
            headerObj.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;

            //닫힘 색상
            Color collapsedColor = currentColor;
            Color expandedColor, nextLevelColor;
            

            Color.RGBToHSV(currentColor, out float H, out float S, out float V);
            V += brightnessStep / 2;
            expandedColor = Color.HSVToRGB(H, S, V);
            V += brightnessStep / 2;
            nextLevelColor = Color.HSVToRGB(H, S, V);

            // --- 3. 버튼 컴포넌트 가져오기 및 초기 색상 설정 ---
            Button headerButton = headerObj.GetComponent<Button>();
            Image headerImage = headerObj.GetComponentInChildren<Image>(); // (Target Graphic 확인 필수)

            if (headerButton != null)
            {
                ColorBlock colors = headerButton.colors;
                colors.normalColor = collapsedColor;
                headerButton.colors = colors;
            }

            // 이 헤더가 토글(접고 펴기)할 자식 UI 요소들 리스트
            List<GameObject> childrenToToggle = new List<GameObject>();

            // --- 2. 이 카테고리의 자식들 생성 ---
            
            // A. 만약 자식이 '하위 카테고리'라면 (예: "Inlay" -> "Shape", "Color")
            if (category.nodeType == CategoryNodeType.SubCategoryList)
            {
                // 재귀 호출을 사용하지 않고, 2단계까지만 지원
                foreach (CustomCategory subCategory in category.subCategories)
                {
                    // "Shape" 헤더 생성
                    GameObject subHeaderObj = Instantiate(categoryHeaderPrefab, parent);
                    subHeaderObj.GetComponentInChildren<TextMeshProUGUI>().text = "└ " + subCategory.categoryName; // (간단한 들여쓰기)

                    Color subCollapsedColor = nextLevelColor;
                    Color subExpandedColor;
                    Color.RGBToHSV(nextLevelColor, out float H2, out float S2, out float V2);
                    V2 += brightnessStep/2;
                    subExpandedColor = Color.HSVToRGB(H2, S2, V2);

                    Button subHeaderButton = subHeaderObj.GetComponent<Button>();
                    Image subHeaderImage = subHeaderObj.GetComponentInChildren<Image>();
                    if (subHeaderButton != null)
                    {
                        ColorBlock colors = subHeaderButton.colors;
                        colors.normalColor = nextLevelColor; // 2단계 색상 적용
                        subHeaderButton.colors = colors;
                    }

                    // "Shape" 헤더가 토글할 '옵션'들을 담을 리스트
                    List<GameObject> subOptionsToToggle = new List<GameObject>();
                    List<Toggle> togglesInThisCategory = new List<Toggle>();

                    // "Dots", "Blocks" 옵션 생성
                    foreach (CustomOption option in subCategory.options)
                    {
                        GameObject optionObj = Instantiate(optionButtonPrefab, parent);
                        SetupOptionButton(optionObj, subCategory, option, togglesInThisCategory);

                        optionObj.SetActive(false); // 옵션은 기본적으로 숨김
                        subOptionsToToggle.Add(optionObj);
                    }
                    
                    bool isSubCategoryExpanded = false; // '닫힘' 상태로 시작
                    subHeaderButton.onClick.AddListener(() =>
                    {
                        // 1. 상태 뒤집기
                        isSubCategoryExpanded = !isSubCategoryExpanded;
                        
                        // 2. 색상 변경
                        if (subHeaderButton != null)
                        {
                            ColorBlock colors = subHeaderButton.colors;
                            colors.normalColor = isSubCategoryExpanded ? subExpandedColor : subCollapsedColor;
                            subHeaderButton.colors = colors;
                        }
                        
                        // 3. 자식(옵션) 토글
                        foreach (GameObject opt in subOptionsToToggle)
                        {
                            opt.SetActive(!opt.activeSelf);
                        }
                    });

                    // "Shape" 헤더와 그 옵션들을 "Inlay" 헤더의 토글 리스트에 추가
                    subHeaderObj.SetActive(false); // 하위 카테고리도 기본적으로 숨김
                    childrenToToggle.Add(subHeaderObj);
                    childrenToToggle.AddRange(subOptionsToToggle);
                }
            }
            // B. 만약 자식이 '옵션'이라면 (예: "Body" -> "Red", "Blue")
            else if (category.nodeType == CategoryNodeType.OptionsList)
            {
                List<Toggle> togglesInThisCategory = new List<Toggle>();
                foreach (CustomOption option in category.options)
                {
                    GameObject optionObj = Instantiate(optionButtonPrefab, parent);
                    SetupOptionButton(optionObj, category, option, togglesInThisCategory);
                    

                    optionObj.SetActive(false); // 옵션은 기본적으로 숨김
                    childrenToToggle.Add(optionObj);
                }
            }

            bool isCategoryExpanded = false; // '닫힘' 상태로 시작
            headerButton.onClick.AddListener(() =>
            {
                // 1. 상태 뒤집기
                isCategoryExpanded = !isCategoryExpanded;
                
                // 2. 색상 변경
                if (headerButton != null)
                {
                    ColorBlock colors = headerButton.colors;
                    colors.normalColor = isCategoryExpanded ? expandedColor : collapsedColor;
                    headerButton.colors = colors;
                }

                // 3. 자식 토글
                foreach (GameObject child in childrenToToggle)
                {
                    child.SetActive(!child.activeSelf);
                }
            });
        }
    }

    /// <summary>
    /// 옵션 버튼(Prefab)의 썸네일, 텍스트, 클릭 이벤트를 설정하는 헬퍼 함수
    /// (기존 DisplayOptions 함수의 로직을 재활용)
    /// </summary>
    void SetupOptionButton(GameObject optionObj, CustomCategory category, CustomOption option, List<Toggle> toggleList)
    {
        // --- 컴포넌트 찾기 ---
        RawImage previewImage = optionObj.transform.Find("PreviewGroup/PreviewFill").GetComponent<RawImage>();
        TextMeshProUGUI nameText = optionObj.transform.Find("PreviewGroup/NameText").GetComponent<TextMeshProUGUI>();
        Toggle thisToggle = optionObj.transform.Find("Checkbox").GetComponent<Toggle>(); // "Checkbox" 이름 확인!
        Button thisButton = optionObj.GetComponent<Button>();

        // --- 썸네일 로직 ---
        if (option.thumbnailIcon != null)
        {
            Sprite sprite = option.thumbnailIcon;
            Texture2D tex = sprite.texture;
            previewImage.texture = tex;
            Rect normalizedRect = new Rect(
                sprite.textureRect.x / tex.width,
                sprite.textureRect.y / tex.height,
                sprite.textureRect.width / tex.width,
                sprite.textureRect.height / tex.height
            );
            previewImage.uvRect = normalizedRect;
            previewImage.color = Color.white;
        }
        else
        {
            previewImage.texture = null;
            previewImage.uvRect = new Rect(0, 0, 1, 1);
            previewImage.color = Color.magenta; // 썸네일 없으면 오류 표시
        }

        // --- 글자 색상 결정 로직 ---
        if (nameText != null)
        {
            nameText.text = option.optionName;
            nameText.color = Color.white;
        }

        // --- 체크박스 로직 설정 ---
        toggleList.Add(thisToggle);
        thisToggle.interactable = false; 

        // --- 버튼 클릭 이벤트 연결 ---
        CustomCategory currentCategory = category;
        CustomOption currentOption = option;
        
        thisButton.onClick.AddListener(() =>
        {
            // 1. 메인 로직 실행
            ApplyOption(currentCategory, currentOption);

            // 2. [★수정★] 파라미터로 받은 리스트를 사용해 토글
            foreach (Toggle t in toggleList)
            {
                t.isOn = false;
            }
            thisToggle.isOn = true;
        });
    }


/// <summary>
    /// UI 버튼이 호출하는 '메인' 함수. 메인 변경과 종속 변경을 모두 지시합니다.
    /// </summary>
    public void ApplyOption(CustomCategory category, CustomOption option)
    {
        // 1. 메인 변경 실행 (예: 픽가드 교체)
        // (이 함수는 targetSlot을 찾아서 프리팹/재질을 교체합니다)
        Transform mainTargetSlot = ExecuteMainChange(category.partToChange, category.actionType, option);

        // 2. 종속 변경 리스트를 순회하며 모두 실행
        if (option.dependentChanges != null && option.dependentChanges.Count > 0)
        {
            // 종속 변경은 '메인 슬롯'(예: PickguardSlot) 내부에서 일어납니다.
            if (mainTargetSlot != null && mainTargetSlot.childCount > 0)
            {
                // 부모 파츠의 Transform (예: 'pickguard_default_prefab' 인스턴스)
                Transform parentPartTransform = mainTargetSlot.GetChild(0);

                foreach (DependentChange change in option.dependentChanges)
                {
                    // 부모 파츠 내부에서 "Dependent/Pickguard_outer" 같은 자식 Transform을 찾습니다.
                    Transform childToChange = parentPartTransform.Find(change.childPath);
                    
                    if (childToChange != null)
                    {
                        // 찾은 자식(예: "Pickguard_outer")에 변경 사항을 적용합니다.
                        ExecuteDependentChange(childToChange, change);
                    }
                    else
                    {
                        Debug.LogWarning("종속 파츠를 찾지 못했습니다: " + change.childPath + " (in " + parentPartTransform.name + ")");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 메인 변경을 실행하고, 변경된 슬롯을 반환합니다. (기존 ApplyOption 로직)
    /// </summary>
    private Transform ExecuteMainChange(TargetPart part, CustomizationType type, CustomOption option)
    {
        // [Skybox 재질 교체]
        if (part == TargetPart.Skybox)
        {
            if (option.materialToApply != null)
            {
                RenderSettings.skybox = option.materialToApply;
                currentSkyboxMaterial = option.materialToApply; 
                DynamicGI.UpdateEnvironment();
            }
            return null; // Skybox는 슬롯이 없음
        }

        // [일반 파츠 교체]
        Transform targetSlot = GetSlotFromPart(part);
        if (targetSlot == null)
        {
            Debug.LogError("타겟 슬롯이 없습니다: " + part);
            return null;
        }
        
        switch (type)
        {
            case CustomizationType.PrefabSwap:
                if (option.partPrefab == null) return targetSlot;
                if (targetSlot.childCount > 0)
                {
                    Destroy(targetSlot.GetChild(0).gameObject);
                }
                GameObject newPart = Instantiate(option.partPrefab, targetSlot);
                newPart.transform.localPosition = Vector3.zero;
                newPart.transform.localRotation = Quaternion.identity;
                break;

            case CustomizationType.MaterialOnly:
                if (option.materialToApply == null) return targetSlot;
                if (targetSlot.childCount == 0) return targetSlot; // 슬롯에 자식이 없음
                
                // 슬롯의 첫 자식 (예: 'pickguard_default_prefab' 인스턴스)
                Transform parentPart = targetSlot.GetChild(0);
                
                MeshRenderer targetRenderer = null;

                // 1. mainChildPath가 비어있다면, '부품 프리팹' 자체의 렌더러를 찾습니다.
                if (string.IsNullOrEmpty(option.mainChildPath))
                {
                    targetRenderer = parentPart.GetComponent<MeshRenderer>();
                }
                // 2. mainChildPath가 있다면, '부품 프리팹'의 자식 중에서 찾습니다.
                else
                {
                    Transform mainChild = parentPart.Find(option.mainChildPath);
                    if (mainChild != null)
                    {
                        targetRenderer = mainChild.GetComponent<MeshRenderer>();
                    }
                    else
                    {
                        Debug.LogError("메인 자식 파츠를 찾지 못했습니다: " + option.mainChildPath);
                    }
                }

                // 3. 렌더러를 찾았다면 재질 적용
                if (targetRenderer != null)
                {
                    targetRenderer.material = option.materialToApply;
                }
                else
                {
                    Debug.LogError("적용할 MeshRenderer를 찾지 못했습니다 (Path: " + option.mainChildPath + ")");
                }
                
                // Head/Neck 연동 로직
                if (part == TargetPart.Neck && headRenderer != null)
                {
                    headRenderer.material = option.materialToApply;
                }
                break;
        }
        return targetSlot; // 변경이 일어난 메인 슬롯을 반환
    }
    
    /// <summary>
    /// 종속 파츠(자식)의 변경을 실행합니다.
    /// </summary>
    private void ExecuteDependentChange(Transform childTransform, DependentChange change)
    {
        switch (change.actionType)
        {
            case CustomizationType.MaterialOnly:
                if (change.materialToApply == null) return;
                
                // 종속 파츠는 자식의 자식일 수 있으므로 GetComponentInChildren 사용
                MeshRenderer targetRenderer = childTransform.GetComponentInChildren<MeshRenderer>();
                if (targetRenderer != null)
                {
                    targetRenderer.material = change.materialToApply;
                }
                else
                {
                    Debug.LogError("종속 자식에서 MeshRenderer를 찾지 못했습니다: " + childTransform.name);
                }
                break;

            case CustomizationType.PrefabSwap:
                // (자식의 프리팹을 교체하는 것은 더 복잡하므로,
                //  일단 MaterialOnly만 구현합니다.)
                break;
        }
    }

    // --- 6. GetSlotFromPart 헬퍼 함수 ---
    Transform GetSlotFromPart(TargetPart part)
    {
        switch (part)
        {
            case TargetPart.Body: return BodySlot;
            case TargetPart.Neck: return NeckSlot;
            case TargetPart.Fretboard: return FretboardSlot;
            case TargetPart.Inlay: return InlaySlot;
            case TargetPart.Pickguard: return PickguardSlot; // 'Pickguard'로 수정
            case TargetPart.Skybox: return null; 
            default: return null;
        }
    }

}