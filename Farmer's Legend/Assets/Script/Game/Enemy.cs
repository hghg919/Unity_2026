using UnityEngine;
using UnityEngine.AI; // NavMesh 사용을 위해 필수

public class Enemy : MonoBehaviour
{
    // 1. 기본 능력치 설정
    public int maxHealth = 1;
    private int currentHealth;

    // 2. AI 유형 설정 (인스펙터 창에서 고를 수 있음)
    public enum EnemyType { Melee, Ranged }
    [Header("AI 유형 결정")]
    public EnemyType enemyType = EnemyType.Melee;

    [Header("원거리 옵션 (Ranged Only)")]
    public float attackRange = 7f;       // 원거리 몹이 멈춰서 공격할 사정거리
    public GameObject enemyProjectile;  // 적이 발사할 똥/화살 프리팹
    public Transform firePoint;          // 적의 총구 위치
    public float attackRate = 1.5f;      // 공격 주기 (초)
    private float nextAttackTime = 0f;

    // 내비게이션 및 플레이어 참조
    private NavMeshAgent agent;
    private Transform playerTransform;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.RegisterEnemy(this.gameObject);
    }

    void Update()
    {
        if (playerTransform == null || agent == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (enemyType == EnemyType.Melee)
        {
            // [근접형] 장애물 피해 무조건 끝까지 쫓아감
            agent.SetDestination(playerTransform.position);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            // ⭐⭐⭐ [인공지능 대폭 고도화] 
            // 사정거리 안이면서 '동시에 벽에 가려지지 않고 눈에 보일 때만' 제자리에 서서 공격합니다!
            if (distanceToPlayer <= attackRange && HasLineOfSight())
            {
                agent.ResetPath(); // 조건이 다 맞을 때만 멈추기

                // 플레이어 바라보기
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);

                // 공격 타이밍 체크
                if (Time.time >= nextAttackTime)
                {
                    RangedAttack();
                }
            }
            else
            {
                // 💡 [핵심] 사거리보다 멀거나, 혹은 사거리 안이더라도 벽에 가려져서 안 보이면?
                // 플레이어가 눈에 보일 때까지 네비메쉬 길을 따라 벽을 돌아서 계속 추격합니다!
                agent.SetDestination(playerTransform.position);
            }
        }
    }

    // 적과 플레이어 사이에 벽(Wall)이 가로막고 있는지 실시간 레이저 검사
    bool HasLineOfSight()
    {
        if (firePoint == null || playerTransform == null) return false;

        // 플레이어의 중심점을 조준하도록 살짝 높이 보정 (피벗이 발바닥일 경우 레이저가 바닥에 닿는 것 방지)
        Vector3 targetCenter = playerTransform.position + Vector3.up * 0.5f;
        Vector3 targetDir = (targetCenter - firePoint.position).normalized;
        float distance = Vector3.Distance(firePoint.position, targetCenter);

        RaycastHit hit;
        // 총구 위치에서 플레이어 방향으로 레이저를 쏩니다.
        if (Physics.Raycast(firePoint.position, targetDir, out hit, distance))
        {
            // 레이저가 플레이어에게 닿기 전에 "Wall" 태그 오브젝트에 부딪혔다면 시야 차단으로 판단
            if (hit.collider.CompareTag("Wall"))
            {
                return false;
            }
        }

        return true;
    }

    void RangedAttack()
    {
        nextAttackTime = Time.time + attackRate;

        if (enemyProjectile != null && firePoint != null)
        {
            Vector3 targetDir = (playerTransform.position - firePoint.position).normalized;
            targetDir.y = 0;
            Instantiate(enemyProjectile, firePoint.position, Quaternion.LookRotation(targetDir));
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.EnemyDied(this.gameObject);

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enemyType == EnemyType.Melee && collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
        }
    }
}