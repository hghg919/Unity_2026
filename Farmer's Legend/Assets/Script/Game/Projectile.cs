using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;

    [Header("반사 설정")]
    public int maxBounces = 3;
    private int currentBounces = 0;

    private float lastBounceTime = 0f;
    private const float bounceCooldown = 0.05f;

    // 📌 유니티 인스펙터에서 등록해둔 오렌지색 타격 파티클 프리팹 슬롯
    [Header("💥 타격감 이펙트 에셋")]
    public GameObject hitEffectPrefab;

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
        // 부딪힌 대상이 "Enemy" 태그를 가지고 있다면
        if (other.CompareTag("Enemy"))
        {
            // 1단계: 일반 잡몹 스크립트(Enemy)가 붙어있는지 확인
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // 1의 데미지를 줌
                SpawnHitEffect();    // ⭐ 흔들림/멈춤 없이 순수하게 이펙트만 생성!
                Destroy(gameObject); // 총알 파괴
                return; // 함수 종료
            }

            // 2단계: [보스몹 대응 추가] 보스몹 스크립트(BossEnemy)가 붙어있는지 확인
            BossEnemy boss = other.GetComponent<BossEnemy>();
            if (boss != null)
            {
                boss.TakeDamage(1);   // 보스에게 1의 데미지를 줌!
                SpawnHitEffect();    // ⭐ 보스에게 맞았을 때도 순수하게 이펙트만 생성!
                Destroy(gameObject); // 총알 파괴
                return; // 함수 종료
            }
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

    // ⭐ 다른 연출을 다 빼고 오직 깔끔한 파티클 폭발 피드백만 남겨둔 안전 함수입니다.
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    // 레이저를 쓰지 않는 가장 확실한 직사각형 맵 반사 로직
    void BounceSimple(Collider wallCollider)
    {
        lastBounceTime = Time.time;

        // 현재 투사체가 날아가던 방향 벡터
        Vector3 currentDir = transform.forward;

        Vector3 rayStart = transform.position - currentDir * 1.0f;
        Ray ray = new Ray(rayStart, currentDir);
        RaycastHit hit;

        if (wallCollider.Raycast(ray, out hit, 5.0f))
        {
            currentDir = Vector3.Reflect(currentDir, hit.normal);
        }
        else
        {
            string wallName = wallCollider.name;
            if (wallName.Contains("North") || wallName.Contains("South")) currentDir.z = -currentDir.z;
            else if (wallName.Contains("East") || wallName.Contains("West")) currentDir.x = -currentDir.x;
        }

        currentDir.y = 0;
        transform.rotation = Quaternion.LookRotation(currentDir);

        currentBounces++;
    }
}