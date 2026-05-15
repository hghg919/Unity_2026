using UnityEngine;
using UnityEngine.InputSystem; // 이 줄을 추가해야 합니다.

public class StageCameraController : MonoBehaviour
{
    public Vector3[] stagePositions;
    public int currentStageIndex = 0;
    public float moveSpeed = 5f;
    public float dragThreshold = 50f;

    private Vector2 touchStartPos;

    void Update()
    {
        HandleMouseInput();
        MoveToStage();
    }

    void HandleMouseInput()
    {
        // 마우스 왼쪽 버튼을 눌렀을 때
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchStartPos = Mouse.current.position.ReadValue();
        }

        // 마우스 왼쪽 버튼을 뗐을 때
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 touchEndPos = Mouse.current.position.ReadValue();
            float swipeDistance = touchEndPos.x - touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > dragThreshold)
            {
                if (swipeDistance < 0 && currentStageIndex < stagePositions.Length - 1)
                    currentStageIndex++;
                else if (swipeDistance > 0 && currentStageIndex > 0)
                    currentStageIndex--;
            }
        }
    }

    void MoveToStage()
    {
        transform.position = Vector3.Lerp(transform.position, stagePositions[currentStageIndex], Time.deltaTime * moveSpeed);
    }
}