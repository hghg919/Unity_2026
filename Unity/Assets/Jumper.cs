using UnityEngine;

public class Jumper : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody myRigidbody;
    void Start()
    {
        myRigidbody.AddForce(0, 500, 0);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
