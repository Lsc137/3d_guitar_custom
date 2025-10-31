using UnityEngine;
using UnityEngine.InputSystem; // Input System 사용
using UnityEngine.EventSystems; // UI 감지용

public class GuitarViewer : MonoBehaviour
{
    // [Inspector에서 연결]
    // CustomizationManager가 Start()에서 자동으로 연결해 줄 것입니다.
    public Transform targetGuitar; 

    // [Inspector에서 조절할 속도 값]
    [SerializeField] private float rotationSpeed = 0.2f; 
    [SerializeField] private float panSpeed = 0.01f;
    [SerializeField] private float zoomSpeed = 0.001f;

    // 카메라의 Transform을 저장할 변수 (이 스크립트가 붙어있는 카메라 자신)
    private Transform cameraTransform;

    // '초기화' 버튼을 위한 변수들
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;

    void Start()
    {
        // 카메라 자신의 Transform을 미리 찾아 변수에 저장
        cameraTransform = this.transform; 
        
        // 카메라의 초기 위치/회전값 저장
        initialCameraPosition = cameraTransform.position;
        initialCameraRotation = cameraTransform.rotation;
        
        // targetGuitar는 CustomizationManager가 설정해줄 때까지 대기
    }

    /// <summary>
    /// CustomizationManager가 기타 로드 시 호출합니다.
    /// </summary>
    public void SetTargetAndInitialize(Transform target, Transform cameraStartTransform)
    {
        // 1. 제어할 기타 타겟 설정
        targetGuitar = target;
        
        // 2. 기타의 초기 위치/회전값 저장 (초기화 버튼용)
        if (targetGuitar != null)
        {
            initialPosition = targetGuitar.position;
            initialRotation = targetGuitar.rotation;
        }

        // 3. 카메라는 _CameraTarget 위치로 이동
        cameraTransform.position = cameraStartTransform.position;
        cameraTransform.rotation = cameraStartTransform.rotation;

        // 4. 카메라의 초기값도 갱신 (씬이 로드될 때마다)
        initialCameraPosition = cameraTransform.position;
        initialCameraRotation = cameraTransform.rotation;
    }

    /// <summary>
    /// '카메라 초기화' UI 버튼이 OnClick()으로 호출하는 함수입니다.
    /// </summary>
    public void ResetView()
    {
        // 기타와 카메라를 모두 저장된 초기 상태로 복원
        if (targetGuitar != null)
        {
            targetGuitar.position = initialPosition;
            targetGuitar.rotation = initialRotation;
        }
        cameraTransform.position = initialCameraPosition;
        cameraTransform.rotation = initialCameraRotation;
    }

    /// <summary>
    /// 매 프레임마다 유니티가 자동 호출합니다.
    /// </summary>
    void Update()
    {
        // 1. 방어 코드
        if (targetGuitar == null) return; // 제어할 타겟이 없으면 중지
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Mouse.current == null) return;

        // --- 2. 마우스 휠: 확대 / 축소 (Zoom) ---
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            // 카메라의 "앞" 방향(cameraTransform.forward)으로 이동
            cameraTransform.Translate(cameraTransform.forward * scroll * zoomSpeed, Space.World);
        }

        // --- 3. 좌클릭 드래그: 회전 (Orbit) ---
        // (이 로직은 기타를 카메라 시점 기준으로 회전시킵니다)
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x * rotationSpeed;
            float mouseY = mouseDelta.y * rotationSpeed;

            // targetGuitar의 현재 위치(position)를 중심으로 회전 (RotateAround)
            targetGuitar.RotateAround(targetGuitar.position, cameraTransform.up, -mouseX);
            targetGuitar.RotateAround(targetGuitar.position, cameraTransform.right, mouseY);
        }

        // --- 4. 우클릭 드래그: 평행 이동 (Pan) ---
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // 카메라의 "오른쪽"과 "위쪽" 방향으로 이동할 벡터 계산
            // (마우스를 오른쪽으로 밀면, 물체도 오른쪽으로 움직임)
            Vector3 panDirection = (mouseDelta.x * cameraTransform.right) + (mouseDelta.y * cameraTransform.up);
            
            // 기타 오브젝트를 해당 방향으로 이동 (Space.World 기준)
            targetGuitar.Translate(panDirection * panSpeed, Space.World);
        }
    }
}