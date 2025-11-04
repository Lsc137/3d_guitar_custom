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
    public float brightnessStep = 0.16f; // 16%씩 밝아짐

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
        string selectedID = "Tele"; 
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
                // prefabToLoad = teleSetupPrefab;
                // categoriesToLoad = allCategories_Tele;

                // prefabToLoad = stratSetupPrefab;
                // categoriesToLoad = allCategories_Strat;

                prefabToLoad = precisionBassSetupPrefab;
                categoriesToLoad = allCategories_PrecisionBass;

                // prefabToLoad = jazzBassSetupPrefab;
                // categoriesToLoad = allCategories_JazzBass;
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

        // 4A. 메뉴 초기화
        foreach (Transform child in menuContentParent)
        {
            Destroy(child.gameObject);
        }
        
        
        CreateCategoryButtons(categoriesToLoad, menuContentParent, baseCategoryColor);
    }

/// <summary>
    /// [새 재귀 함수 1] (슈도코드의 CreateCategoryButton)
    /// 카테고리 버튼 리스트를 생성하고, UIMenuNode를 붙여 초기화합니다.
    /// </summary>
    private List<GameObject> CreateCategoryButtons(List<CustomCategory> categories, Transform parent, Color currentColor)
    {
        // 이 함수 호출(현재 레벨)에서 생성된 '직접 자식' 오브젝트들
        List<GameObject> createdNodeObjects = new List<GameObject>();
        if (categories == null) return createdNodeObjects;

        foreach (CustomCategory category in categories)
        {
            // --- 1. '카테고리 헤더' 생성 ---
            GameObject headerObj = Instantiate(categoryHeaderPrefab, parent);
            createdNodeObjects.Add(headerObj); // 반환 리스트에 '나'를 추가

            // --- 2. 색상 계산 ---
            Color collapsedColor = currentColor;
            Color expandedColor;
            Color childColor;
            Color.RGBToHSV(currentColor, out float H, out float S, out float V);
            V += brightnessStep / 2;
            expandedColor = Color.HSVToRGB(H, S, V);
            V += brightnessStep / 2;
            childColor = Color.HSVToRGB(H, S, V);
            
            // --- 3. 자식 오브젝트들 생성 (재귀) ---
            List<GameObject> childrenList = new List<GameObject>();
            
            if (category.nodeType == CategoryNodeType.SubCategoryList)
            {
                childrenList = CreateCategoryButtons(category.subCategories, parent, childColor);
            }
            else if (category.nodeType == CategoryNodeType.OptionsList)
            {
                childrenList = CreateOptionButtons(category, parent);
            }
            
            // --- 4. 자식들 숨기기 ---
            foreach (GameObject child in childrenList)
            {
                child.SetActive(false);
            }

            // --- 5. '노드' 컴포넌트 가져와서 초기화 ---
            // (주의: categoryHeaderPrefab에 UIMenuNode.cs가 미리 붙어있어야 함)
            UIMenuNode node = headerObj.GetComponent<UIMenuNode>();
            if (node != null)
            {
                // UIMenuNode에게 텍스트, 색상, 자식 리스트를 주입
                node.Initialize(collapsedColor, expandedColor, childrenList, category.categoryName);
            }
            else
            {
                Debug.LogError("CategoryHeaderPrefab에 UIMenuNode 스크립트가 없습니다!");
            }
        }
        return createdNodeObjects; // 생성된 노드 리스트 반환
    }

    /// <summary>
    /// [새 헬퍼 함수 2] (슈도코드의 CreateOptionButton)
    /// '옵션' 버튼들만 생성합니다. (UIMenuNode 없음)
    /// </summary>
    private List<GameObject> CreateOptionButtons(CustomCategory category, Transform parent)
    {
        List<GameObject> createdOptions = new List<GameObject>();
        List<Toggle> togglesForThisList = new List<Toggle>(); // 이 카테고리만의 토글 리스트

        foreach (CustomOption option in category.options)
        {
            GameObject optionObj = Instantiate(optionButtonPrefab, parent);
            SetupOptionButton(optionObj, category, option, togglesForThisList); 
            createdOptions.Add(optionObj);
        }
        return createdOptions;
    }
    
    /// <summary>
    /// [새 헬퍼 함수 3]
    /// 옵션 버튼(Prefab)의 썸네일, 텍스트, 클릭 이벤트를 설정합니다. (체크박스 버그 수정 포함)
    /// </summary>
    void SetupOptionButton(GameObject optionObj, CustomCategory category, CustomOption option, List<Toggle> toggleList)
    {
        // --- 컴포넌트 찾기 ---
        RawImage previewImage = optionObj.transform.Find("PreviewGroup/PreviewFill").GetComponent<RawImage>();
        TextMeshProUGUI nameText = optionObj.transform.Find("PreviewGroup/NameText").GetComponent<TextMeshProUGUI>();
        Toggle thisToggle = optionObj.transform.Find("Checkbox").GetComponent<Toggle>(); 
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
            previewImage.color = Color.magenta;
        }

        // --- 글자 색상 결정 로직 ---
        if (nameText != null)
        {
            nameText.text = option.optionName;
            nameText.color = Color.white;
        }

        // --- 체크박스 로직 설정 ---
        toggleList.Add(thisToggle); // 이 리스트는 CreateOptionButtons에서 새로 생성됨
        thisToggle.interactable = false; 

        // --- 버튼 클릭 이벤트 연결 ---
        thisButton.onClick.AddListener(() =>
        {
            ApplyOption(category, option);
            
            // 이 카테고리 리스트의 모든 토글을 끄고
            foreach (Toggle t in toggleList)
            {
                t.isOn = false;
            }
            // 지금 누른 것만 켠다
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
                Transform parentPartTransform = mainTargetSlot.Find("Dependent");

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
                    Destroy(targetSlot.Find("Main").GetChild(0).gameObject);
                }
                Instantiate(option.partPrefab, targetSlot.Find("Main"));
                break;

            case CustomizationType.MaterialOnly:
                if (option.materialToApply == null) return targetSlot;
                if (targetSlot.childCount == 0) return targetSlot; // 슬롯에 자식이 없음

                // 슬롯의 첫 자식 (예: 'pickguard_default_prefab' 인스턴스)
                Transform parentPart = targetSlot.Find(option.mainChildPath);
                
                MeshRenderer targetRenderer = null;

                // 1. mainChildPath가 비어있다면, '부품 프리팹' 자체의 렌더러를 찾습니다.
                if (string.IsNullOrEmpty(option.mainChildPath))
                {
                    targetRenderer = parentPart.GetComponent<MeshRenderer>();
                }
                // 2. mainChildPath가 있다면, '부품 프리팹'의 자식 중에서 찾습니다.
                else
                {
                    Transform mainChild = parentPart.GetChild(0);
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