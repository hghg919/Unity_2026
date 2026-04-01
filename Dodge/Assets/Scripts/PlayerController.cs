using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody playerRigidbody;
    public float speed = 8f;
    void Start()
    {
        playerRigidbody =GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(xInput, 0f, zInput).normalized;

        Vector3 newVelocity = new Vector3(xInput, 0f, zInput).normalized;
        newVelocity *= speed;
        playerRigidbody.linearVelocity = newVelocity;

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(movement),
                10f * Time.deltaTime
            );
        }
    }

    public void Die()
    {
        gameObject.SetActive(false);
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.EndGame();
    }
}
