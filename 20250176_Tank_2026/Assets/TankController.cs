using UnityEngine;

public class TankController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int playerNum = 1;
    string mvAxisName;
    string rotAxisName;

    public float moveSpeed = 10f;
    public float rotateSpeed = 150f;
    void Start()
    {
        mvAxisName = "Vertical" + playerNum;
        rotAxisName = "Horizontal" + playerNum;
    }

    // Update is called once per frame
    void Update()
    {
        float move = Input.GetAxis(mvAxisName)*moveSpeed*Time.deltaTime;
        float rotate = Input.GetAxis(rotAxisName)*rotateSpeed*Time.deltaTime;

        transform.Translate(0, 0, move);
        transform.Rotate(0, rotate, 0);

    }
}
