using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    // 1. 재질을 바꿀 대상을 Inspector에서 연결할 변수
    public MeshRenderer bodyRenderer;
    public MeshRenderer neckRenderer;

    // 2. UI 버튼이 호출할 공개(public) 함수들
    //    (Inspector에서 이 함수로 'Material' 에셋을 전달받습니다)

    public void ChangeBodyMaterial(Material newMaterial)
    {
        if (bodyRenderer != null)
        {
            // bodyRenderer의 재질을 newMaterial로 교체!
            bodyRenderer.material = newMaterial;
        }
    }

    public void ChangeNeckMaterial(Material newMaterial)
    {
        if (neckRenderer != null)
        {
            // neckRenderer의 재질을 newMaterial로 교체!
            neckRenderer.material = newMaterial;
        }
    }
}