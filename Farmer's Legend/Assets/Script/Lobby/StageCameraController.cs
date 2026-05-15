using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // TextMeshPro를 사용하기 위해 꼭 필요합니다.

public class StageCameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    public Vector3[] stagePositions; // 3개의 스테이지 좌표 (농장, 강, 바다 순서)
    public float moveSpeed = 5f;
    public float dragThreshold = 50f;

    [Header("UI 설정")]
    public TextMeshProUGUI stageTitleText; // 간판 위의 TMP 텍스트 컴포넌트
    public string[] stageNames; // 각 스테이지에 표시될 이름들

    public int currentStageIndex = 0;
    private Vector2 touchStartPos;

    void Update()
    {
        HandleMouseInput();
        MoveToStage();
        UpdateStageUI(); // 매 프레임 UI 상태 확인
    }

    void HandleMouseInput()
    {
        // 마우스 왼쪽 버튼 클릭 시작
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchStartPos = Mouse.current.position.ReadValue();
        }

        // 마우스 왼쪽 버튼 클릭 해제
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 touchEndPos = Mouse.current.position.ReadValue();
            float swipeDistance = touchEndPos.x - touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > dragThreshold)
            {
                if (swipeDistance < 0 && currentStageIndex < stagePositions.Length - 1)
                {
                    currentStageIndex++; // 다음 스테이지
                }
                else if (swipeDistance > 0 && currentStageIndex > 0)
                {
                    currentStageIndex--; // 이전 스테이지
                }
            }
        }
    }

    void MoveToStage()
    {
        // 목표 좌표로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, stagePositions[currentStageIndex], Time.deltaTime * moveSpeed);
    }

    void UpdateStageUI()
    {
        // 현재 인덱스에 맞는 텍스트로 변경
        if (stageTitleText != null && stageNames.Length > currentStageIndex)
        {
            stageTitleText.text = stageNames[currentStageIndex];
        }
    }
}