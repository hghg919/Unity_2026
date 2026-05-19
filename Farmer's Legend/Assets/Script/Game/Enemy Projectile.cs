using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;       // 적 투사체 속도 (플레이어 것보다 약간 느린 게 피하기 좋습니다)
    public float lifetime = 5f;      // 혹시 안 부딪히면 5초 뒤 자동 삭제

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 매 프레임 정면으로 전진
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. 플레이어(Player)와 부딪혔을 때
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1); // 플레이어 체력 1 깎기
            }
            Destroy(gameObject); // 플레이어 맞췄으니 총알 삭제
        }

        // 2. 투명 통짜 벽(Wall)과 부딪혔을 때
        else if (other.CompareTag("Wall"))
        {
            // [기획 참고] 잡몹 총알은 벽에 안 튕기고 바로 사라지는 게 플레이하기 덜 피곤합니다.
            Destroy(gameObject);
        }
    }
}