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
        // 부딪힌 대상이 "Enemy" 태그를 가지고 있다면
        if (other.CompareTag("Enemy"))
        {
            // 1단계: 일반 잡몹 스크립트(Enemy)가 붙어있는지 확인
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // 1의 데미지를 줌
                Destroy(gameObject); // 총알 파괴
                return; // 함수 종료
            }

            // ⭐⭐⭐ 2단계: [보스몹 대응 추가] 보스몹 스크립트(BossEnemy)가 붙어있는지 확인
            BossEnemy boss = other.GetComponent<BossEnemy>();
            if (boss != null)
            {
                boss.TakeDamage(1); // 보스에게 1의 데미지를 줌!
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

    // 레이저를 쓰지 않는 가장 확실한 직사각형 맵 반사 로직
    // --- Projectile.cs 내부의 BounceSimple 함수 교체 ---
    void BounceSimple(Collider wallCollider)
    {
        lastBounceTime = Time.time;

        // 현재 투사체가 날아가던 방향 벡터
        Vector3 currentDir = transform.forward;

        // 💡 [핵심 개선] 이름 매칭 방식 타파! 
        // 총알 위치보다 살짝 뒤에서 진행 방향으로 레이저(Ray)를 쏘아 부딪힌 벽의 표면 각도(법선 벡터)를 구합니다.
        Vector3 rayStart = transform.position - currentDir * 1.0f;
        Ray ray = new Ray(rayStart, currentDir);
        RaycastHit hit;

        // 충돌한 콜라이더 표면의 튕겨나가는 정방향(hit.normal)을 획득합니다.
        if (wallCollider.Raycast(ray, out hit, 5.0f))
        {
            // 유니티 물리 엔진 공식: Vector3.Reflect(입사각, 반사표면방향)
            currentDir = Vector3.Reflect(currentDir, hit.normal);
        }
        else
        {
            // [백업 안전장치] 레이캐스트가 안 잡힐 경우 예전 이름 기반 코드로 작동
            string wallName = wallCollider.name;
            if (wallName.Contains("North") || wallName.Contains("South")) currentDir.z = -currentDir.z;
            else if (wallName.Contains("East") || wallName.Contains("West")) currentDir.x = -currentDir.x;
        }

        // 높이 변화 고정 및 회전값 재연산
        currentDir.y = 0;
        transform.rotation = Quaternion.LookRotation(currentDir);

        currentBounces++;
    }
}