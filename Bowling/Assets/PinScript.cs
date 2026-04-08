using UnityEngine;

public class PinScript : MonoBehaviour
{
    public bool isDown = false;

    void Update()
    {
        // 핀의 위쪽 방향(Up)이 하늘(Vector3.up)과 멀어지면 쓰러진 것으로 간주
        // 0.6f는 약 50~60도 정도 기울어졌을 때를 의미합니다.
        if (!isDown && Vector3.Dot(transform.up, Vector3.up) < 0.6f)
        {
            isDown = true;
            GameManager.instance.AddScore(); // 점수 추가 호출
        }
    }
}