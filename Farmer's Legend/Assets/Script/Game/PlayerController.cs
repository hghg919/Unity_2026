using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 제어하기 위해 필요합니다.
using System.Collections; // IEnumerator 사용을 위해 필요합니다.
using System.Collections.Generic; // List 사용을 위해 필요합니다.

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject projectilePrefab; // 발사할 투사체 프리팹
    public Transform firePoint;          // 투사체가 나갈 총구 위치

    private Rigidbody rb;
    private Vector3 moveVelocity;
    private bool isMoving = false;

    // 공격 주기 관리
    public float attackRate = 0.5f;
    private float nextAttackTime = 0f;

    [Header("체력 설정")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI 연동")]
    public Image[] hpImages; // 인스펙터에서 구급상자 이미지 최대치(5개 권장) 등록

    [Header("타격감 연출 (URP 무결점 업그레이드)")]
    private List<Material> allMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private Coroutine flashCoroutine;

    // 🏅 [보상 데이터 관리 시스템]
    private int extraBounces = 0;  // BounceUp 누적 스택
    private int multiShotLevel = 0; // ⭐ [추가] 다중 발사(MultiShot) 누적 레벨 (0~2)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth; // 시작할 때 체력 풀피로 설정

        // 뼈대 속 모자까지 Renderer 전수조사
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                foreach (Material mat in r.materials)
                {
                    if (mat != null)
                    {
                        allMaterials.Add(mat);
                        Color origColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                        originalColors.Add(origColor);
                    }
                }
            }
        }

        // 게임 시작 시 체력 UI 초기화
        UpdateHpUI();
    }

    void Update()
    {
        if (isDead) return;

        // 1. 키보드 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(moveX, 0f, moveZ).normalized;
        moveVelocity = moveInput * moveSpeed;

        isMoving = moveInput.magnitude > 0.1f;

        // 2. 멈춰있을 때만 공격 트리거
        if (!isMoving && Time.time >= nextAttackTime)
        {
            AttackClosestEnemy();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (isMoving)
        {
            Quaternion newRotation = Quaternion.LookRotation(moveVelocity);
            rb.MoveRotation(newRotation);
        }
    }

    void AttackClosestEnemy()
    {
        if (InGameStageManager.Instance == null) return;
        List<GameObject> enemies = InGameStageManager.Instance.EnemiesInRoom;

        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            Vector3 targetDir = (closestEnemy.transform.position - transform.position).normalized;
            targetDir.y = 0;

            transform.rotation = Quaternion.LookRotation(targetDir);

            // ⭐⭐⭐ [효과음 추가 - 요소 0번: PlayerShoot]
            // 아까 나눴던 대화처럼 다중 발사(MultiShot) 시 소리가 겹쳐서 시끄러워지는 것을 방지하기 위해,
            // 화살 개별 개수가 아닌 "한 번 공격 액션을 취할 때 딱 한 번 깔끔하게" 발사음이 터지도록 설계했습니다.
            if (StageManager.Instance != null)
            {
                StageManager.Instance.PlaySFX(StageManager.SFXType.PlayerShoot);
            }

            // 기본 상태 (Level 0): 전방 발사 1개
            SpawnProjectile(targetDir);

            if (multiShotLevel == 1)
            {
                // 첫 번째 획득 (Level 1): 전방 1개 + 우측 대각선 1개 (총 2발)
                Vector3 rightDiag = Quaternion.Euler(0, 25f, 0) * targetDir; // Y축 기준 우측으로 25도 회전
                SpawnProjectile(rightDiag);
            }
            else if (multiShotLevel >= 2)
            {
                // 두 번째 획득 (Level 2): 전방 1개 + 우측 대각선 1개 + 좌측 대각선 1개 (총 3발)
                Vector3 rightDiag = Quaternion.Euler(0, 25f, 0) * targetDir;
                Vector3 leftDiag = Quaternion.Euler(0, -25f, 0) * targetDir; // Y축 기준 좌측으로 25도 회전
                SpawnProjectile(rightDiag);
                SpawnProjectile(leftDiag);
            }

            nextAttackTime = Time.time + attackRate;
        }
    }

    // 투사체 생성 및 반사 횟수 주입을 처리하는 안전 공용 함수
    void SpawnProjectile(Vector3 direction)
    {
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        Projectile projScript = projObj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.maxBounces += extraBounces; // 기존 먹어둔 BounceUp 스택 실시간 가산
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        // ⭐⭐⭐ [효과음 추가 - 요소 1번: PlayerHit]
        // 플레이어가 피격되어 하트가 깎이고 몸이 빨갛게 깜빡이는 타이밍에 신음/피격음을 즉시 재생합니다.
        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.PlayerHit);
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(PlayerFlashRoutine());

        UpdateHpUI();

        if (currentHealth <= 0) PlayerDie();
    }

    void PlayerDie()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        moveVelocity = Vector3.zero;

        // ⭐⭐⭐ [효과음 추가 - 요소 2번: PlayerDeath]
        // 사망 플래그가 서고 인게임 매니저에게 소식을 알리기 직전, 처절한 사망 효과음을 재생합니다.
        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.PlayerDeath);
        }

        if (InGameStageManager.Instance != null) InGameStageManager.Instance.PlayerDied();
    }

    void UpdateHpUI()
    {
        if (hpImages == null || hpImages.Length == 0) return;

        for (int i = 0; i < hpImages.Length; i++)
        {
            if (hpImages[i] == null) continue;

            if (i < maxHealth)
            {
                hpImages[i].gameObject.SetActive(true);
                if (i < currentHealth) hpImages[i].color = Color.white;
                else hpImages[i].color = new Color(0.25f, 0.25f, 0.25f, 0.3f);
            }
            else
            {
                hpImages[i].gameObject.SetActive(false);
            }
        }
    }

    IEnumerator PlayerFlashRoutine()
    {
        Color damageColor = new Color(1f, 0.3f, 0.3f, 1f);
        for (int i = 0; i < allMaterials.Count; i++)
        {
            if (allMaterials[i] != null)
            {
                if (allMaterials[i].HasProperty("_BaseColor")) allMaterials[i].SetColor("_BaseColor", damageColor);
                else allMaterials[i].color = damageColor;
            }
        }
        yield return new WaitForSecondsRealtime(0.1f);
        for (int i = 0; i < allMaterials.Count; i++)
        {
            if (allMaterials[i] != null)
            {
                if (allMaterials[i].HasProperty("_BaseColor")) allMaterials[i].SetColor("_BaseColor", originalColors[i]);
                else allMaterials[i].color = originalColors[i];
            }
        }
    }

    // ⭐⭐⭐ [핵심 수정] 룰렛 카드 보상 적용 함수 내부 개조
    public void ApplyReward(string rewardType)
    {
        switch (rewardType)
        {
            case "FireRateUp":
                attackRate = Mathf.Max(0.1f, attackRate - 0.05f);
                break;

            case "MoveSpeedUp":
                moveSpeed += 1.0f;
                break;

            case "BounceUp":
                extraBounces++;
                break;

            // ⭐ [기획 병합] 스마트 힐 시스템
            case "HealOrMaxHP":
                if (currentHealth < maxHealth)
                {
                    // 1) 피가 깎여있다면 1칸 단순 치유
                    currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                    Debug.Log($"📚 [스마트 보상] 체력 1 회복 완료! 현재 체력: {currentHealth}/{maxHealth}");
                }
                else
                {
                    // 2) 피가 만땅이라면 하트 슬롯 최대치 확장 (+ 보너스로 한 칸 채워줌)
                    if (maxHealth < hpImages.Length)
                    {
                        maxHealth++;
                        currentHealth++;
                        Debug.Log($"❤️ [스마트 보상] 체력이 가득 차 있어 최대 체력 확장! 현재 최대 체력: {maxHealth}칸");
                    }
                    else
                    {
                        Debug.Log("❤️ 이미 하트 소지 상한선에 도달하여 확장이 불가능합니다.");
                    }
                }
                UpdateHpUI(); // 변경된 하트 상태 즉시 갱신
                break;

            // ⭐ [기획 추가] 다중 발사 시스템
            case "MultiShot":
                multiShotLevel = Mathf.Min(2, multiShotLevel + 1); // 최대 레벨 2까지 제한 중첩
                Debug.Log($"🏹 [다중 발사] 업그레이드 완료! 현재 발사 레벨: {multiShotLevel} (추가 화살 발사)");
                break;
        }
    }
}