using UnityEngine;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    public Rigidbody ballRigidbody;
    public float speed = 10f;
    private bool isLaunched = false;

    // 정지 감지용 변수
    private float stopThreshold = 0.1f; // 이 속도보다 낮으면 멈춘 것으로 간주
    private float stopTimer = 0f;
    private float waitTime = 2f; // 2초 동안 멈춰있으면 다음 라운드

    // 초기 위치 저장용
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. 발사 로직
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isLaunched)
        {
            ballRigidbody.AddForce(0f, 0f, speed, ForceMode.Impulse);
            isLaunched = true;
        }

        // 2. 멈춤 감지 로직
        if (isLaunched)
        {
            // 리지드바디의 속도(magnitude)가 임계값보다 낮은지 확인
            if (ballRigidbody.linearVelocity.magnitude < stopThreshold)
            {
                stopTimer += Time.deltaTime;
                if (stopTimer >= waitTime)
                {
                    GameManager.instance.NextRound();
                    ResetBall(); // 공 상태 초기화
                }
            }
            else
            {
                stopTimer = 0f; // 다시 움직이면 타이머 초기화
            }
        }
    }

    public void ResetBall()
    {
        isLaunched = false;
        stopTimer = 0f;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }
}