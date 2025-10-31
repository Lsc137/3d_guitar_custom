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
            GameObject guitarInstance = Instantiate(prefabToLoad, guitarSpawnPoint, Quaternion.Euler(guitarSpawnRotation));

            // 2. 씬에서 GuitarViewer 찾기
            GuitarViewer viewer = FindObjectOfType<GuitarViewer>();

            // 3. _CameraTarget 찾기
            Transform cameraTargetTransform = guitarInstance.transform.Find("_CameraTarget");

            if (viewer != null)
            {
                // [★핵심★] 롤백된 SetTargetAndInitialize 호출
                // (이 함수는 targetGuitar도 함께 설정해 줌)
                viewer.SetTargetAndInitialize(guitarInstance.transform, cameraTargetTransform);
            }
            else
            {
                Debug.LogError("씬에서 GuitarViewer 스크립트를 찾을 수 없습니다.");
            }
            
            // 5. 슬롯 찾기 (기존 코드)
            BodySlot = guitarInstance.transform.Find("BodySlot");
            NeckSlot = guitarInstance.transform.Find("NeckSlot");
            InlaySlot = guitarInstance.transform.Find("InlaySlot");
            PickguardSlot = guitarInstance.transform.Find("PickguardSlot");
            FretboardSlot = guitarInstance.transform.Find("FretboardSlot");
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
                if (option.thumbnailIcon != null)
                {
                    Sprite sprite = option.thumbnailIcon;
                    Texture2D tex = sprite.texture;

                    // 썸네일 텍스처 설정
                    previewImage.texture = tex;
                    
                    // 스프라이트의 픽셀 Rect를 전체 텍스처 크기로 나눠서 uvRect 계산
                    Rect normalizedRect = new Rect(
                        sprite.textureRect.x / tex.width,
                        sprite.textureRect.y / tex.height,
                        sprite.textureRect.width / tex.width,
                        sprite.textureRect.height / tex.height
                    );
                    previewImage.uvRect = normalizedRect;
                    
                    // Sprite를 위한 기본 틴트는 흰색(원본 색상)
                    previewImage.color = Color.white; 
                }
                // 2. (비상용) 썸네일이 실수로 비어있는 경우
                else
                {
                    previewImage.texture = null;
                    previewImage.uvRect = new Rect(0, 0, 1, 1); 
                    previewImage.color = Color.magenta; // '오류'를 의미하는 자홍색
                }

                // --- 글자 색상 결정 로직 ---
                if (nameText != null)
                {
                    nameText.text = option.optionName;
                    
                    // 모든 썸네일이 (가정상) Sprite(이미지)이므로,
                    // '흰색' 글씨가 가장 잘 보일 것이라고 가정
                    nameText.color = Color.white;
                    // (팁: TextMeshPro의 'Outline'이나 'Drop Shadow'를 켜면 더 좋습니다)
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
                Transform parentPartTransform = mainTargetSlot.GetChild(1);

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

                Transform mainChild = targetSlot.GetChild(0);
                
                if (mainChild != null)
                {
                    MeshRenderer targetRenderer = mainChild.GetComponent<MeshRenderer>();
                    if (targetRenderer != null) {
                        targetRenderer.material = option.materialToApply;
                    } else {
                        Debug.LogError("메인 자식에서 MeshRenderer를 찾지 못했습니다");
                    }
                }
                else
                {
                    Debug.LogError("메인 자식 파츠를 찾지 못했습니다");
                    Debug.Log("Current path: " + targetSlot);
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
            case TargetPart.Pickguard_inner: return PickguardSlot;
            case TargetPart.Skybox: return null; 
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