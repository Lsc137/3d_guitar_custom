using UnityEngine;

public class MainMenuButtonHelper : MonoBehaviour
{
    public void GoToMainMenu()
    {
        // GameManager 싱글톤 인스턴스를 찾아서 함수 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다!");
        }
    }
}