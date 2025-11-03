using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// '카테고리 헤더' 프리팹에 붙어서,
/// 자신의 '열림/닫힘' 상태와 '자식 노드'들을 관리하는
/// 재귀 트리 노드 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIMenuNode : MonoBehaviour
{
    // --- 1. 상태 (State) ---
    private bool isExpanded = false;
    private bool is_option;

    // --- 2. 외형 (Appearance) ---
    private Button myButton;
    private Color collapsedColor;
    private Color expandedColor;

    // --- 3. 트리 구조 (Tree Structure) ---
    private List<UIMenuNode> directChildren;
}


public class UIMenuTree : MonoBehaviour
{
    public void Initialize(Color cColor, Color eColor, bool type, List<UIMenuNode> children)
    {
        // 1. 참조 설정
        myButton = GetComponent<Button>();
        is_option = type;

        // 2. 색상 저장
        collapsedColor = cColor;
        expandedColor = eColor;
        
        // 3. 자식 노드 리스트 저장
        foreach (CustomCategory child in children){
            if (child.nodeType == CategoryNodeType.SubCategoryList)
            {
                directChildren.Add(new UIMenuNode(cColor, eColor, false, child))
            }
            else
            {
                directChildren.Add(new UIMenuNode(null, null, true, null))
            }
        }
        
        
        // 4. 초기 상태 설정 ('닫힘')
        isExpanded = false;
        SetButtonColor(collapsedColor); // 닫힘 색상으로 시작
        
        // 5. 클릭 이벤트 연결
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
        // 1. 내 색상을 '열림'으로 변경 (카테고리일시만)
        if (!is_option) SetButtonColor(expandedColor);
        
        // 2. 내 '직접 자식'들만 켠다
        foreach (UIMenuNode child in directChildren)
        {
            child.Button.SetActive(true);
        }
    }



    /// <summary>
    /// '닫기' (CollapseOperation)
    /// (슈도코드의 CollapseOperation - subtree 순회 부분)
    /// </summary>
    private void CollapseOperation()
    {
        // 1. 내 색상을 '닫힘'으로 변경
        SetButtonColor(collapsedColor);

        // 2. 내 '직접 자식'들을 끈다 (재귀적 닫기 시작)
        foreach (UIMenuNode child in directChildren)
        {
            CollapseChildRecursively(child);
        }
    }

    /// <summary>
    /// 자식 오브젝트를 재귀적으로 닫습니다.
    /// </summary>
    private void CollapseChildRecursively(UIMenuNode targetObject)
    {
        if (targetObject.is_option)
        {
            SetActive(false);
            return;
        }

        if (!targetObject.is_option && childNode.isExpanded)
        {
            foreach (UIMenuNode child : targetObject.directChildren)
            {
                childNode.CollapseChildRecursively();
            }
            
            targetObject.SetActive(false);
        }
        
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