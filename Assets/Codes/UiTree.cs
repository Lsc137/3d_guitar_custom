using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// '카테고리 헤더' 프리팹에 붙어서,
/// 자신의 '열림/닫힘' 상태와 '자식 노드'들을 관리하는
/// 재귀 트리 노드 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIMenuNode : MonoBehaviour
{
    // --- 1. 참조 (References) ---
    private Button myButton;
    private TextMeshProUGUI myText;
    
    // --- 2. 상태 (State) ---
    private bool isExpanded = false;

    // --- 3. 외형 (Appearance) ---
    private Color collapsedColor;
    private Color expandedColor;

    // --- 4. 트리 구조 (Tree Structure) ---
    // 이 노드가 직접 켜고 끌 '자식' 오브젝트들
    private List<GameObject> directChildren = new List<GameObject>();

    /// <summary>
    /// CustomizationManager가 이 노드를 생성할 때 호출하는 초기화 함수
    /// </summary>
    public void Initialize(Color cColor, Color eColor, List<GameObject> children, string categoryName)
    {
        // 1. 참조 설정
        myButton = GetComponent<Button>();
        myText = GetComponentInChildren<TextMeshProUGUI>();
        
        // 2. 텍스트 설정
        myText.text = categoryName;

        // 3. 색상 저장
        collapsedColor = cColor;
        expandedColor = eColor;
        
        // 4. 자식 노드 리스트 저장
        directChildren = children;
        
        // 5. 초기 상태 설정 ('닫힘')
        isExpanded = false;
        SetButtonColor(collapsedColor); // 닫힘 색상으로 시작
        
        // 6. 클릭 이벤트 연결
        myButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
        myButton.onClick.AddListener(OnToggle);
    }

    /// <summary>
    /// 버튼 클릭 시 호출되는 메인 토글 함수
    /// </summary>
    private void OnToggle()
    {
        isExpanded = !isExpanded; // 상태 반전

        if (isExpanded)
        {
            OpenOperation();
        }
        else
        {
            CollapseOperation();
        }
    }

    /// <summary>
    /// '열기' (OpenOperation)
    /// </summary>
    private void OpenOperation()
    {
        // 1. 내 색상을 '열림'으로 변경
        SetButtonColor(expandedColor);
        
        // 2. 내 '직접 자식'들만 켠다
        foreach (GameObject child in directChildren)
        {
            child.SetActive(true);
        }
    }

    /// <summary>
    /// '닫기' (CollapseOperation)
    /// </summary>
    private void CollapseOperation()
    {
        // 1. 내 색상을 '닫힘'으로 변경
        SetButtonColor(collapsedColor);

        // 2. 내 '직접 자식'들을 끈다 (재귀적 닫기 시작)
        foreach (GameObject child in directChildren)
        {
            CollapseChildRecursively(child);
        }
    }

    /// <summary>
    /// 자식 오브젝트를 재귀적으로 닫습니다. (Post-order)
    /// </summary>
    private void CollapseChildRecursively(GameObject targetObject)
    {
        // 1. 자식이 UIMenuNode(다른 카테고리)인지 확인
        UIMenuNode childNode = targetObject.GetComponent<UIMenuNode>();
        
        // 2. 만약 자식이 '열려있는' 카테고리 노드였다면,
        //    그 자식의 '닫기' 함수를 먼저 호출
        if (childNode != null && childNode.isExpanded)
        {
            childNode.CollapseOperation(); 
        }
        
        // 3. 모든 자손들을 다 닫은 후, '나'를 끈다.
        targetObject.SetActive(false);
    }

    /// <summary>
    /// 버튼의 Normal Color를 설정하는 헬퍼 함수
    /// </summary>
    private void SetButtonColor(Color newColor)
    {
        if (myButton == null) return;
            
        ColorBlock colors = myButton.colors;
        colors.normalColor = newColor;
        myButton.colors = colors;
    }
}