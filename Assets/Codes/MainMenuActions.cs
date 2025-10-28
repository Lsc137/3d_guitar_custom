using UnityEngine;

public class MainMenuButtonActions : MonoBehaviour
{
    // 버튼이 Inspector에서 호출할 함수
    // (어떤 기타를 선택했는지 문자열로 받음)
    public void SelectGuitar(string guitarID)
    {
        // GameManager 싱글톤 인스턴스를 찾아서 함수 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectGuitarAndLoad(guitarID);
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다!");
        }
    }
}