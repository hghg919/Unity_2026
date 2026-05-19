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
            // [원거리형]
            if (distanceToPlayer <= attackRange)
            {
                // 사정거리 안이면 추격을 멈추고 제자리에 서서 공격
                agent.ResetPath(); // 멈추기

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
                // 사정거리보다 멀면 다시 플레이어 쫓아가기
                agent.SetDestination(playerTransform.position);
            }
        }
    }

    void RangedAttack()
    {
        nextAttackTime = Time.time + attackRate;

        if (enemyProjectile != null && firePoint != null)
        {
            // 적의 투사체 발사 (플레이어 방향으로)
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
        Destroy(gameObject);
    }
}