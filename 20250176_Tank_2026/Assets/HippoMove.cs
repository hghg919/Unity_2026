using UnityEngine;

public class HippoMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 2f;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 move = transform.forward*moveSpeed*Time.fixedDeltaTime;
        rb.MovePosition(rb.position - move);
    }
}
