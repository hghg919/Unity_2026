using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 제어하기 위해 필요합니다.

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
    public Image[] hpImages; // 인스펙터에서 구급상자 이미지 3개 등록

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth; // 시작할 때 체력 풀피로 설정

        // 게임 시작 시 체력 UI를 풀피 상태로 초기화합니다.
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

        // 이동 중인지 체크
        isMoving = moveInput.magnitude > 0.1f;

        // 2. 궁수의 전설 핵심 로직: 멈춰있을 때만 공격 트리거
        if (!isMoving && Time.time >= nextAttackTime)
        {
            AttackClosestEnemy();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 물리 이동 및 회전
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (isMoving)
        {
            // 이동 방향 바라보기
            Quaternion newRotation = Quaternion.LookRotation(moveVelocity);
            rb.MoveRotation(newRotation);
        }
    }

    void AttackClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        // 적이 있다면 조준하고 발사
        if (closestEnemy != null)
        {
            Vector3 targetDir = (closestEnemy.transform.position - transform.position).normalized;
            targetDir.y = 0; // 높이 고정
            transform.rotation = Quaternion.LookRotation(targetDir);

            // 투사체 생성
            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(targetDir));

            // 다음 공격 쿨타임 지정
            nextAttackTime = Time.time + attackRate;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("💥 플레이어 피격! 남은 체력: " + currentHealth);

        // 피격 시 체력 UI를 실시간으로 갱신합니다.
        UpdateHpUI();

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        moveVelocity = Vector3.zero;
        Debug.Log("💀 게임 오버! 플레이어가 사망했습니다.");

        if (InGameStageManager.Instance != null)
        {
            InGameStageManager.Instance.PlayerDied();
        }
    }

    // ⭐⭐⭐ [핵심 수정] 구급상자를 끄지 않고, 반투명한 검은색/흑백조로 변경하는 함수
    void UpdateHpUI()
    {
        if (hpImages == null || hpImages.Length == 0) return;

        for (int i = 0; i < hpImages.Length; i++)
        {
            if (hpImages[i] == null) continue;

            // 이미지가 무조건 화면에 켜져 있도록 강제하되, 색상만 변경합니다.
            hpImages[i].gameObject.SetActive(true);

            if (i < currentHealth)
            {
                // 1. 체력이 남아있는 칸: 원본 컬러 그대로 선명하게 표시 (RGB: 1, 1, 1 / Alpha: 1)
                hpImages[i].color = Color.white;
            }
            else
            {
                // 2. 체력이 깎인 칸: 어두운 회색조(0.25f)로 다운시키고 + 투명도를 30%(0.3f)로 낮춰 반투명 흑백 효과 연출
                hpImages[i].color = new Color(0.25f, 0.25f, 0.25f, 0.3f);
            }
        }
    }

    public void ApplyReward(string rewardType)
    {
        switch (rewardType)
        {
            case "FireRateUp":
                attackRate = Mathf.Max(0.1f, attackRate - 0.05f);
                Debug.Log($"🏹 공속 증가! 현재 공격 속도 주기: {attackRate}초");
                break;

            case "MoveSpeedUp":
                moveSpeed += 1.0f;
                Debug.Log($"👟 이속 증가! 현재 이동 속도: {moveSpeed}");
                break;

            case "Heal":
                currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                Debug.Log($"📚 체력 1 회복! 현재 체력: {currentHealth}/{maxHealth}");

                // 보상방에서 구급상자를 먹어 치유될 때도 UI를 실시간 갱신합니다.
                UpdateHpUI();
                break;
        }
    }
}