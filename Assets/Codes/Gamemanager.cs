using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 인스턴스

    // 다음 씬으로 전달할 데이터
    // (예: "JazzBass", "Strat")
    public string SelectedGuitarID { get; private set; } 

    void Awake()
    {
        // --- 싱글톤 패턴 ---
        // 씬에 GameManager가 이미 있는지 확인
        if (Instance == null)
        {
            // 없다면, 이것을 인스턴스로 지정
            Instance = this;
            // 씬이 바뀌어도 이 오브젝트를 파괴하지 말라고 명령
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 있다면, 이 새 오브젝트는 파괴 (중복 방지)
            Destroy(gameObject);
        }
    }

    // --- 메인 메뉴의 UI 버튼이 이 함수를 호출할 것입니다 ---
    public void SelectGuitarAndLoad(string guitarID)
    {
        SelectedGuitarID = guitarID;

        // "CustomizerScene" 씬을 로드합니다.
        // (빌드 설정에서 Index 1이었던 씬)
        SceneManager.LoadScene("CustomizerScene");
        // 또는 SceneManager.LoadScene(1);
    }
}