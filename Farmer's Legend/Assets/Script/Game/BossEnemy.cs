using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("보스 능력치")]
    public int maxHealth = 3;
    private int currentHealth;
    public float moveSpeed = 4f;

    [Header("돌진(스킬) 설정")]
    public float chargeSpeed = 18f;
    public float chargeMaxDuration = 0.6f;
    public float prepareDuration = 1.0f;
    public float stunDuration = 1.0f;

    private enum BossState { Chasing, Preparing, Charging, Stunned }
    private BossState currentState = BossState.Chasing;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform playerTransform;
    private Animator anim; // 📌 보스 말 애니메이터 제어용

    [Header("타격감, 경고 및 이펙트 연출")]
    private Renderer[] bossRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    public GameObject deathEffectPrefab;
    public GameObject chargeDustPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // 애니메이터 캐싱

        if (agent != null) agent.speed = moveSpeed;

        bossRenderers = GetComponentsInChildren<Renderer>();
        if (bossRenderers != null && bossRenderers.Length > 0)
        {
            originalColors = new Color[bossRenderers.Length];
            for (int i = 0; i < bossRenderers.Length; i++)
            {
                originalColors[i] = bossRenderers[i].material.color;
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.RegisterEnemy(this.gameObject);

        StartCoroutine(BossPatternLoop());
    }

    IEnumerator BossPatternLoop()
    {
        while (currentState != BossState.Stunned || currentHealth > 0)
        {
            // 1단계: 추격
            currentState = BossState.Chasing;
            if (agent != null) agent.enabled = true;

            // ⭐ [수정 구역 1] "isCharging_b" 대신 실제 생성한 파라미터 이름인 "Run"으로 연동합니다.
            if (anim != null)
            {
                anim.SetBool("Run", false); // ◀ 변경 완료!
                anim.SetBool("Eat_b", false);
                anim.SetFloat("Speed_f", 0.5f);
            }

            yield return new WaitForSeconds(3.0f);

            if (playerTransform == null) yield return null;

            // 2단계: 기 모으기 (경고 연출)
            currentState = BossState.Preparing;
            if (agent != null) agent.enabled = false;
            rb.linearVelocity = Vector3.zero;

            Vector3 targetDir = (playerTransform.position - transform.position).normalized;
            targetDir.y = 0;
            transform.rotation = Quaternion.LookRotation(targetDir);

            // 📌 기 모으는 1초 동안 멈춰서 바닥을 쿵쿵 찧는(Eat) 경고 모션을 발동시킵니다!
            if (anim != null)
            {
                anim.SetFloat("Speed_f", 0f);
                anim.SetBool("Eat_b", true);
            }

            SetBossColor(new Color(1f, 0f, 0f, 1f));
            yield return new WaitForSeconds(prepareDuration);

            ResetBossColor();
            if (anim != null) anim.SetBool("Eat_b", false); // 돌진 전 기모으기 모션 해제

            // 3단계: 초고속 직선 돌진 발사!
            currentState = BossState.Charging;
            Vector3 chargeDirection = transform.forward;

            // ⭐ [수정 구역 2] 돌진을 가동하는 순간 "Run" 변수를 true로 켭니다.
            if (anim != null) anim.SetBool("Run", true); // ◀ 변경 완료!

            if (StageManager.Instance != null)
            {
                StageManager.Instance.PlaySFX(StageManager.SFXType.BossCharge);
            }

            GameObject dustObj = null;
            if (chargeDustPrefab != null)
            {
                dustObj = Instantiate(chargeDustPrefab, transform.position, Quaternion.identity, transform);
            }

            float chargeTimer = 0f;
            while (chargeTimer < chargeMaxDuration && currentState == BossState.Charging)
            {
                chargeTimer += Time.deltaTime;
                rb.linearVelocity = chargeDirection * chargeSpeed;
                yield return null;
            }

            if (dustObj != null)
            {
                ParticleSystem ps = dustObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    dustObj.transform.SetParent(null);
                    ps.Stop();
                    Destroy(dustObj, 2.0f);
                }
                else
                {
                    Destroy(dustObj);
                }
            }

            // 4단계: 그로기 스턴
            currentState = BossState.Stunned;
            rb.linearVelocity = Vector3.zero;

            // ⭐ [수정 구역 3] 돌진이 끝났으므로 "Run"을 끄고 완전히 멈춥니다.
            if (anim != null)
            {
                anim.SetBool("Run", false); // ◀ 변경 완료!
                anim.SetFloat("Speed_f", 0f);
            }

            SetBossColor(new Color(1f, 0.9f, 0.3f, 1f));
            yield return new WaitForSeconds(stunDuration);

            ResetBossColor();
        }
    }

    void Update()
    {
        if (currentState == BossState.Chasing && agent != null && agent.enabled && playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == BossState.Charging && collision.gameObject.CompareTag("Wall"))
        {
            currentState = BossState.Stunned;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }

            if (currentState == BossState.Charging)
            {
                currentState = BossState.Stunned;
            }
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
        StopAllCoroutines();

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (StageManager.Instance != null)
            PreloadSFXIfNecessary();

        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.EnemyDied(this.gameObject);

        Destroy(gameObject);
    }

    private void PreloadSFXIfNecessary()
    {
        if (StageManager.Instance != null) StageManager.Instance.PlaySFX(StageManager.SFXType.EnemyDeath);
    }

    void SetBossColor(Color color)
    {
        if (bossRenderers == null) return;
        foreach (var r in bossRenderers)
        {
            if (r != null && r.material != null)
            {
                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", color);
                else r.material.color = color;
            }
        }
    }

    void ResetBossColor()
    {
        if (bossRenderers == null) return;
        for (int i = 0; i < bossRenderers.Length; i++)
        {
            if (bossRenderers[i] != null && bossRenderers[i].material != null)
            {
                if (bossRenderers[i].material.HasProperty("_BaseColor")) bossRenderers[i].material.SetColor("_BaseColor", originalColors[i]);
                else bossRenderers[i].material.color = originalColors[i];
            }
        }
    }

    IEnumerator DamageFlashRoutine()
    {
        SetBossColor(Color.red);
        yield return new WaitForSecondsRealtime(0.1f);
        ResetBossColor();
    }
}