using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 2.0f;      // 이동 속도
    public float startX = 30.0f;   // 시작 지점 (오른쪽 밖)
    public float endX = -30.0f;    // 끝 지점 (왼쪽 밖)

    [Header("Y축(높이) 범위")]
    public float minY = 4.0f;
    public float maxY = 8.0f;

    [Header("Z축(깊이) 범위")]
    public float minZ = 2.0f;      // 카메라에서 가까운 최소 거리
    public float maxZ = 8.0f;     // 카메라에서 먼 최대 거리

    void Update()
    {
        // 1. 좌측으로 이동
        transform.position += Vector3.left * speed * Time.deltaTime;

        // 2. 화면 왼쪽 경계를 넘어갔는지 확인
        if (transform.position.x <= endX)
        {
            // 3. 우측으로 재배치하면서 Y와 Z를 랜덤하게 설정
            float randomY = Random.Range(minY, maxY);
            float randomZ = Random.Range(minZ, maxZ);

            transform.position = new Vector3(startX, randomY, randomZ);
        }
    }
}