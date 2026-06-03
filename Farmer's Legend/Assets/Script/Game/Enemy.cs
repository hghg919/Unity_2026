using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;

    [Header("이동 능력치")]
    public float moveSpeed = 3.5f;

    public enum EnemyType { Melee, Ranged }
    [Header("AI 유형 결정")]
    public EnemyType enemyType = EnemyType.Melee;

    [Header("원거리 옵션 (Ranged Only)")]
    public float attackRange = 7f;
    public GameObject enemyProjectile;
    public Transform firePoint;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Animator anim; // 📌 동물 애니메이터 컴포넌트 제어용

    [Header("타격감 및 이펙트 연출")]
    private Renderer enemyRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    public GameObject deathEffectPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>(); // 애니메이터 캐싱

        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

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

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (enemyType == EnemyType.Melee)
        {
            agent.SetDestination(playerTransform.position);

            // 📌 [애니메이션] 근접 몹은 항상 움직이므로 기본 걷기 속도(0.5) 주입
            if (anim != null) anim.SetFloat("Speed_f", 0.5f);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            if (distanceToPlayer <= attackRange && HasLineOfSight())
            {
                agent.ResetPath();

                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);

                if (Time.time >= nextAttackTime)
                {
                    RangedAttack();
                }
            }
            else
            {
                agent.SetDestination(playerTransform.position);

                // 📌 [애니메이션] 추격 중일 때는 걷기 모션 가동
                if (anim != null && !anim.GetBool("Eat_b"))
                {
                    anim.SetFloat("Speed_f", 0.5f);
                }
            }
        }
    }

    bool HasLineOfSight()
    {
        if (firePoint == null || playerTransform == null) return false;

        Vector3 targetCenter = playerTransform.position + Vector3.up * 0.5f;
        Vector3 targetDir = (targetCenter - firePoint.position).normalized;
        float distance = Vector3.Distance(firePoint.position, targetCenter);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, targetDir, out hit, distance))
        {
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

        // 📌 [애니메이션 연동] 에셋 규칙에 맞게 속도를 0으로 내리고 Eat_b를 켭니다!
        if (anim != null)
        {
            anim.SetFloat("Speed_f", 0f);
            anim.SetBool("Eat_b", true);
            StartCoroutine(ResetAttackAnimation());
        }

        if (enemyProjectile != null && firePoint != null)
        {
            Vector3 targetDir = (playerTransform.position - firePoint.position).normalized;
            targetDir.y = 0;
            Instantiate(enemyProjectile, firePoint.position, Quaternion.LookRotation(targetDir));
        }
    }

    // 공격 후 다시 걷기 상태로 돌려놓는 안전 코루틴
    IEnumerator ResetAttackAnimation()
    {
        // 기존 0.4초에서 0.15초 ~ 0.2초 정도로 줄여줍니다.
        yield return new WaitForSeconds(0.18f);
        if (anim != null)
        {
            anim.SetBool("Eat_b", false);
            anim.SetFloat("Speed_f", 0.5f); // 다시 팍팍 걸어오도록 복구
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.EnemyHit);
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.EnemyDeath);
        }

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

                // 근접 몹도 부딪히는 순간 한 번 까딱 공격 연출
                if (anim != null)
                {
                    anim.SetFloat("Speed_f", 0f);
                    anim.SetBool("Eat_b", true);
                    StartCoroutine(ResetAttackAnimation());
                }
            }
        }
    }

    IEnumerator DamageFlashRoutine()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
            enemyRenderer.material.color = originalColor;
        }
    }
}