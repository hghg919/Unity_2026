using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 제어하기 위해 필요합니다.
using System.Collections; // IEnumerator 사용을 위해 필요합니다.
using System.Collections.Generic; // List 사용을 위해 필요합니다.

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject projectilePrefab; // 발사할 투사체 프리팹
    public Transform firePoint;          // 투사체가 나갈 총구 위치

    [Header("부드러운 회전 설정")]
    public float rotationSpeed = 14f;

    private Rigidbody rb;
    private Animator anim; // 에셋 애니메이터 컴포넌트 제어용
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
    private int multiShotLevel = 0; // 다중 발사(MultiShot) 누적 레벨 (0~2)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // 내 오브젝트의 Animator 가져오기

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

        // [에셋 맞춤형 애니메이션 이동 이식]
        if (anim != null)
        {
            anim.SetBool("Static_b", !isMoving); // 움직일 때 false, 멈추면 true
            anim.SetFloat("Speed_f", isMoving ? 1.0f : 0.0f); // 걷기 조건(0.25) 충족용
        }

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
            // [부드러운 구면 선형 회전 보간 적용]
            Quaternion targetRotation = Quaternion.LookRotation(moveVelocity);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
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

            // 사격 타켓팅 부드러운 회전
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed * 1.5f));

            // ⭐ [오류 해결 완료 구역] 변수 뒤에 붙어있던 불필요한 기호를 완벽히 제거했습니다!
            if (anim != null)
            {
                anim.SetInteger("Animation_int", 10);
                // 멈추지 않고 계속 던지는 버그를 막기 위해 0.15초 뒤 자동으로 0번(Idle)으로 되돌립니다.
                StartCoroutine(ResetAttackAnimation());
            }

            // 효과음 추가 - 요소 0번: PlayerShoot
            if (StageManager.Instance != null)
            {
                StageManager.Instance.PlaySFX(StageManager.SFXType.PlayerShoot);
            }

            // 기본 상태 (Level 0): 전방 발사 1개
            SpawnProjectile(targetDir);

            if (multiShotLevel == 1)
            {
                Vector3 rightDiag = Quaternion.Euler(0, 25f, 0) * targetDir;
                SpawnProjectile(rightDiag);
            }
            else if (multiShotLevel >= 2)
            {
                Vector3 rightDiag = Quaternion.Euler(0, 25f, 0) * targetDir;
                Vector3 leftDiag = Quaternion.Euler(0, -25f, 0) * targetDir;
                SpawnProjectile(rightDiag);
                SpawnProjectile(leftDiag);
            }

            nextAttackTime = Time.time + attackRate;
        }
    }

    // 공격 애니메이션 안전 리셋 코루틴
    IEnumerator ResetAttackAnimation()
    {
        // 현재 0.15초로 되어있는데, 모션 속도를 올린 후 
        // 너무 일찍 끊기면 0.2f로 늘려주고, 반대로 너무 늦게 풀리면 0.1f로 줄여주며 튜닝하면 됩니다!
        yield return new WaitForSecondsRealtime(0.2f);
        if (anim != null && !isDead)
        {
            anim.SetInteger("Animation_int", 0);
        }
    }

    void SpawnProjectile(Vector3 direction)
    {
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        Projectile projScript = projObj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.maxBounces += extraBounces;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;

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

        // [에셋 맞춤형 애니메이션 사망 이식]
        if (anim != null)
        {
            anim.SetBool("Death_b", true); // 사망 확인 시 쓰러지는 애니메이션 가동
        }

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

            case "HealOrMaxHP":
                if (currentHealth < maxHealth)
                {
                    currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                }
                else
                {
                    if (maxHealth < hpImages.Length)
                    {
                        maxHealth++;
                        currentHealth++;
                    }
                }
                UpdateHpUI();
                break;

            case "MultiShot":
                multiShotLevel = Mathf.Min(2, multiShotLevel + 1);
                break;
        }
    }
}