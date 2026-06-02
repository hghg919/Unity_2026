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
    public float chargeSpeed = 18f;       // 돌진 속도 (정말 빨라야 피하는 맛이 있습니다)
    public float chargeMaxDuration = 0.6f; // 벽에 안 부딪혔을 때 최대 돌진 시간
    public float prepareDuration = 1.0f;   // 돌진 전 기 모으는 시간 (경고 연출)
    public float stunDuration = 1.0f;      // 벽에 박은 뒤 그로기 상태 시간

    // 보스 상태 관리 제어용
    private enum BossState { Chasing, Preparing, Charging, Stunned }
    private BossState currentState = BossState.Chasing;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform playerTransform;

    [Header("타격감 및 경고 연출")]
    private Renderer[] bossRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent != null) agent.speed = moveSpeed;

        // 뼈대 속에 숨은 모자나 마구까지 렌더러 전수 조사 (플레이어 방식 재활용)
        bossRenderers = GetComponentsInChildren<Renderer>();
        if (bossRenderers != null && bossRenderers.Length > 0)
        {
            originalColors = new Color[bossRenderers.Length];
            for (int i = 0; i < bossRenderers.Length; i++)
            {
                originalColors[i] = bossRenderers[i].material.color;
            }
        }

        // 플레이어 타겟 추적 및 방 등록
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.RegisterEnemy(this.gameObject);

        // 🔥 보스의 독자적인 AI 패턴 루프 가동!
        StartCoroutine(BossPatternLoop());
    }

    // ⭐⭐⭐ [핵심 인공지능] 코루틴을 이용한 상태 기반 패턴 제어 시스템
    IEnumerator BossPatternLoop()
    {
        while (currentState != BossState.Stunned || currentHealth > 0)
        {
            // 1단계: 3초 동안 플레이어를 추격합니다.
            currentState = BossState.Chasing;
            if (agent != null) agent.enabled = true;
            yield return new WaitForSeconds(3.0f);

            if (playerTransform == null) yield return null;

            // 2단계: 돌진 전 기 모으기 (1초 동안 멈춰서 플레이어 조준 + 몸이 빨개짐)
            currentState = BossState.Preparing;
            if (agent != null) agent.enabled = false; // 네비게이션 정지
            rb.linearVelocity = Vector3.zero;

            // 플레이어의 마지막 위치 조준
            Vector3 targetDir = (playerTransform.position - transform.position).normalized;
            targetDir.y = 0;
            transform.rotation = Quaternion.LookRotation(targetDir);

            // 경고 연출: 몸을 붉게 물들임
            SetBossColor(new Color(1f, 0f, 0f, 1f));
            yield return new WaitForSeconds(prepareDuration);

            // 원래 색으로 임시 복구 후 돌진 시작
            ResetBossColor();

            // 3단계: 초고속 직선 돌진 발사!
            currentState = BossState.Charging;
            Vector3 chargeDirection = transform.forward; // 기 모으기가 끝난 시점의 정면 방향

            // ⭐⭐⭐ [효과음 추가 - 요소 5번: BossCharge]
            // 기 모으기 경고가 끝나고 물리적으로 몸이 튀어나가는 이 기막힌 타이밍에 보스 돌진음 가동!
            if (StageManager.Instance != null)
            {
                StageManager.Instance.PlaySFX(StageManager.SFXType.BossCharge);
            }

            float chargeTimer = 0f;
            while (chargeTimer < chargeMaxDuration && currentState == BossState.Charging)
            {
                chargeTimer += Time.deltaTime;
                // 유니티 6 물리 엔진에 맞춰 직선 속도 강제 주입
                rb.linearVelocity = chargeDirection * chargeSpeed;
                yield return null;
            }

            // 4단계: 돌진이 끝났거나 무언가에 부딪힌 후 '그로기 스턴' (1초 휴식)
            currentState = BossState.Stunned;
            rb.linearVelocity = Vector3.zero;

            // 그로기 연출: 몸이 노랗게 변함
            SetBossColor(new Color(1f, 0.9f, 0.3f, 1f));
            yield return new WaitForSeconds(stunDuration);

            ResetBossColor();
        }
    }

    void Update()
    {
        // 추격 상태일 때만 네비메쉬 인공지능으로 플레이어를 쫓아갑니다.
        if (currentState == BossState.Chasing && agent != null && agent.enabled && playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    // ⭐⭐⭐ [핵심 충돌 판정] 돌진 중 장애물(Wall)에 박으면 즉시 멈추는 물리 처리
    void OnCollisionEnter(Collision collision)
    {
        // 1. 돌진 중에 "Wall" 태그를 가진 내부 벽/외곽 벽에 부딪혔다면?
        if (currentState == BossState.Charging && collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("💥 보스가 벽에 정면 충돌하여 돌진을 멈춥니다!");
            currentState = BossState.Stunned; // 루프 안의 돌진 while문을 파괴하고 스턴 단계로 강제 전환
        }

        // 2. 플레이어와 부딪혔을 때 데미지 주기
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }

            // 플레이어 받쳐 버린 후에도 충격으로 돌진이 끊기게 설계
            if (currentState == BossState.Charging)
            {
                currentState = BossState.Stunned;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // ⭐⭐⭐ [효과음 추가 - 요소 3번: EnemyHit]
        // 보스가 플레이어의 반사 화살을 맞았을 때도 동일하게 찰진 피격음을 들려줍니다.
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

        // ⭐⭐⭐ [효과음 추가 - 요소 4번: EnemyDeath]
        // 최종 스테이지 보스가 격파되어 처단되는 통쾌함을 사망음으로 마무리합니다.
        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.EnemyDeath);
        }

        if (InGameStageManager.Instance != null)
            InGameStageManager.Instance.EnemyDied(this.gameObject);

        Destroy(gameObject);
    }

    // 머티리얼 제어 함수들 (URP 무결점 버전)
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