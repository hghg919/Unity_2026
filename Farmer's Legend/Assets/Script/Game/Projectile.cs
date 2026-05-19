using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;

    [Header("반사 설정")]
    public int maxBounces = 3;
    private int currentBounces = 0;

    private float lastBounceTime = 0f;
    private const float bounceCooldown = 0.05f;

    void Start()
    {
        // 맵 밖으로 완전히 탈출했을 때를 대비한 최소한의 안전장치
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Projectile 스크립트의 OnTriggerEnter 내부 수정
        if (other.CompareTag("Enemy"))
        {
            // 부딪힌 대상에게서 Enemy 스크립트를 가져옴
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(1); // 1의 데미지를 줌
            }

            Destroy(gameObject); // 적과 부딪혔으므로 병은 파괴됨
        }
        else if (other.CompareTag("Wall"))
        {
            if (Time.time - lastBounceTime < bounceCooldown) return;

            if (currentBounces < maxBounces)
            {
                BounceSimple(other); // 새로운 무결점 반사 함수 호출
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    // 레이저를 쓰지 않는 가장 확실한 직사각형 맵 반사 로직
    void BounceSimple(Collider wallCollider)
    {
        lastBounceTime = Time.time;

        // 현재 투사체가 날아가던 방향 벡터 구하기
        Vector3 currentDir = transform.forward;

        // 부딪힌 투명 벽의 이름을 체크하여 반사 방향을 결정
        string wallName = wallCollider.name;

        if (wallName.Contains("North") || wallName.Contains("South"))
        {
            // 위아래(북/남) 벽에 부딪히면 앞뒤 방향(Z축)을 뒤집음
            currentDir.z = -currentDir.z;
        }
        else if (wallName.Contains("East") || wallName.Contains("West"))
        {
            // 좌우(동/서) 벽에 부딪히면 좌우 방향(X축)을 뒤집음
            currentDir.x = -currentDir.x;
        }

        // 높이 변화 방지
        currentDir.y = 0;

        // 투사체의 방향을 새로 계산된 반사 방향으로 돌려줌
        transform.rotation = Quaternion.LookRotation(currentDir);

        currentBounces++;
    }
}