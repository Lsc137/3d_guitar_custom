using UnityEngine;
using UnityEngine.InputSystem; // Input System 사용
using UnityEngine.EventSystems; // UI 감지용

public class GuitarViewer : MonoBehaviour
{
    // [Inspector에서 조절할 속도 값]
    // 중요: Time.deltaTime을 안 쓰므로 0.1 ~ 0.5 사이의 작은 값을 사용합니다.
    [SerializeField] private float rotationSpeed = 0.2f; 
    [SerializeField] private float panSpeed = 0.01f;
    [SerializeField] private float zoomSpeed = 0.001f;

    // 카메라의 Transform을 저장할 변수 (이 스크립트가 붙어있는 카메라 자신)
    private Transform cameraTransform;
    private Transform targetGuitar; 
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialCameraPosition; // 카메라 위치도 초기화
    private Quaternion initialCameraRotation; // 카메라 회전도 초기화

    void Start()
    {
        // 'transform'은 이 스크립트가 붙어있는 GameObject(Main Camera)의 Transform입니다.
        cameraTransform = this.transform;
        initialCameraPosition = cameraTransform.position;
        initialCameraRotation = cameraTransform.rotation;
    }

    public void SetTargetAndInitialize(Transform target)
    {
        targetGuitar = target;
        if (targetGuitar != null)
        {
            initialPosition = targetGuitar.position;
            initialRotation = targetGuitar.rotation;
        }
    }

    public void ResetView()
    {
        if (targetGuitar != null)
        {
            targetGuitar.position = initialPosition;
            targetGuitar.rotation = initialRotation;
        }
        // 카메라 위치/회전도 초기화
        cameraTransform.position = initialCameraPosition;
        cameraTransform.rotation = initialCameraRotation;
    }

    void Update()
    {
        // 1. 마우스가 UI 위에 있는지 확인 (EventSystem 확인 필수)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // UI 위에 있으면 아래 모든 로직을 무시
        }

        // 마우스가 연결되어 있는지 확인
        if (Mouse.current == null) return;

        // --- 3. 마우스 휠: 확대 / 축소 (Zoom) ---
        // 마우스 휠 스크롤 값을 읽어옵니다. (보통 +120 / -120)
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
        {
            // 카메라의 "앞" 방향(cameraTransform.forward)으로 이동
            // Space.World: 카메라가 어디를 보든 그 "앞" 방향으로 이동
            cameraTransform.Translate(cameraTransform.forward * scroll * zoomSpeed, Space.World);
        }

        if (Mouse.current.leftButton.isPressed)
        {
            // 마우스의 X, Y 이동량 (픽셀)
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * rotationSpeed;
            float mouseY = mouseDelta.y * rotationSpeed;

            // [수정] RotateAround 대신 Rotate를 사용합니다.
            //       카메라 축(cameraTransform.up) 대신 월드 축(Vector3.up)을 사용합니다.
            
            // 1. 좌우 드래그
            targetGuitar.Rotate(Vector3.right, mouseX, Space.World);

            // 2. 상하 드래그
            targetGuitar.Rotate(Vector3.forward, mouseY, Space.World);
        }
        // --- 2. 우클릭 드래그: 평행 이동 (Pan) ---
        if (Mouse.current.rightButton.isPressed)
        {
            // 마우스의 X, Y 이동량 (픽셀)
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Time.deltaTime을 곱하지 않습니다!
            // 카메라의 "오른쪽"과 "위쪽" 방향으로 이동할 벡터 계산
            // (마우스를 오른쪽으로 밀면, 물체는 왼쪽으로 움직여야 '잡고 끄는' 느낌이 납니다)
            Vector3 panDirection = (mouseDelta.x * cameraTransform.right) + (mouseDelta.y * cameraTransform.up);

            // 기타 오브젝트를 해당 방향으로 이동 (Space.World 기준)
            targetGuitar.Translate(panDirection * panSpeed, Space.World);
        }
        
        if (targetGuitar == null) return;
    }
}