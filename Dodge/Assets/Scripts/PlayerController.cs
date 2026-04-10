using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody playerRigidbody;
    public float speed = 8f;

    private float hAxis;
    private float vAxis;

    // 조이스틱에서 호출할 함수
    public void SetJoystickInput(float x, float z)
    {
        hAxis = x;
        vAxis = z;
    }

    void Start()
    {
        playerRigidbody =GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // 조이스틱 값과 키보드 값을 합쳐서 이동 처리
        float xInput = Input.GetAxis("Horizontal") + hAxis;
        float zInput = Input.GetAxis("Vertical") + vAxis;

        Vector3 movement = new Vector3(xInput, 0f, zInput).normalized;

        // 기존의 이동 및 회전 로직 실행
        Move(movement);

    }

    public void Die()
    {
        gameObject.SetActive(false);
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.EndGame();
    }

    private void Move(Vector3 direction)
    {
        Vector3 newVelocity = direction * speed;
        playerRigidbody.linearVelocity = newVelocity;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                10f * Time.deltaTime
            );
        }
    }
}
