using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody playerRigidbody;
    public float speed = 8f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) == true)
        {
            playerRigidbody.AddForce(0f,0f,speed);
        }
        if (Input.GetKey(KeyCode.DownArrow) == true)
        {
            playerRigidbody.AddForce(0f, 0f, -speed);
        }
        if (Input.GetKey(KeyCode.RightArrow) == true)
        {
            playerRigidbody.AddForce(speed, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.LeftArrow) == true)
        {
            playerRigidbody.AddForce(-speed, 0f, 0f);
        }
    }
}
