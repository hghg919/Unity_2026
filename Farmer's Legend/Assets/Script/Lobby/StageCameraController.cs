using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // ⭐ [필수 추가] 유니티 UI 클릭 감지용 네임스페이스
using TMPro;

public class StageCameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    public Vector3[] stagePositions; // 3개의 스테이지 좌표 (농장, 강, 바다 순서)
    public float moveSpeed = 5f;
    public float dragThreshold = 50f;

    [Header("UI 설정")]
    public TextMeshProUGUI stageTitleText; // 간판 위의 TMP 텍스트 컴포넌트
    public string[] stageNames; // 각 스테이지에 표시될 이름들

    [Header("🚫 드래그를 차단할 UI 패널들 (Hierarchy에서 드래그 앤 드롭)")]
    public GameObject stageDetailPanel;  // 세부 정보 창 (StageDetail)
    public GameObject settingsPanel;     // 일시정지/설정 창 (SettingsPanel)

    public int currentStageIndex = 0;
    private Vector2 touchStartPos;

    // ⭐ [추가] UI 관통 드래그를 방지하기 위한 상태 제어 플래그
    private bool isDragging = false;

    private LobbyManager cachedLobby; // 캐싱용 변수 추가

    void Start()
    {
        cachedLobby = FindFirstObjectByType<LobbyManager>();
    }

    void Update()
    {
        // 팝업 UI 창이 하나라도 활성화되어 있다면 드래그 처리를 건너뜁니다.
        if (!IsPopupOpen())
        {
            HandleMouseInput();
        }
        else
        {
            // 팝업창이 뜨는 순간 혹시나 진행 중이던 드래그 연산도 안전하게 강제 초기화합니다.
            isDragging = false;
        }

        MoveToStage();
        UpdateStageUI();
    }

    // 인스펙터에 등록된 UI 창 또는 그 자식들이 활성화되어 있는지 삼중으로 정밀 체크합니다.
    bool IsPopupOpen()
    {
        // 1. 상세창 활성화 체크
        if (stageDetailPanel != null && stageDetailPanel.activeInHierarchy) return true;

        // 2. 세팅 패널 활성화 체크
        if (settingsPanel != null)
        {
            // 부모(SettingsPanel) 자체가 켜져 있는 경우
            if (settingsPanel.activeInHierarchy) return true;

            // 구조상 부모는 켜져 있고 내부의 자식 'Panel'만 토글되는 경우를 대비한 방어 코드
            Transform childPanel = settingsPanel.transform.Find("Panel");
            if (childPanel != null && childPanel.gameObject.activeInHierarchy) return true;
        }

        return false;
    }

    void HandleMouseInput()
    {
        // 1. 마우스 왼쪽 버튼 클릭 시작
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // ⭐ [핵심 추가] 클릭한 순간 마우스 포인터가 유니티 UI(버튼, 패널 등) 위에 있다면 드래그를 시작하지 않습니다.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = false;
                return;
            }

            touchStartPos = Mouse.current.position.ReadValue();
            isDragging = true; // UI가 아닌 순수 배경을 눌렀을 때만 드래그 시작 인정
        }

        // 2. 마우스 왼쪽 버튼 클릭 해제 (정상적으로 배경에서 시작된 드래그인 경우에만 작동)
        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            Vector2 touchEndPos = Mouse.current.position.ReadValue();
            float swipeDistance = touchEndPos.x - touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > dragThreshold)
            {
                if (swipeDistance < 0 && currentStageIndex < stagePositions.Length - 1)
                {
                    currentStageIndex++;
                }
                else if (swipeDistance > 0 && currentStageIndex > 0)
                {
                    currentStageIndex--;
                }
            }
        }
    }

    void MoveToStage()
    {
        float dt = Time.timeScale == 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, stagePositions[currentStageIndex], dt * moveSpeed);
    }

    void UpdateStageUI()
    {
        if (stageTitleText != null && stageNames.Length > currentStageIndex)
        {
            stageTitleText.text = stageNames[currentStageIndex];
        }

        // Find 명령을 지우고, 스타트 때 찾아둔 변수를 재사용합니다.
        if (cachedLobby != null)
        {
            cachedLobby.UpdateThemePanel(currentStageIndex);
        }
    }
}