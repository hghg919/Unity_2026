using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth; // 시작할 때 체력 풀피로 설정
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

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero; // 물리 이동 즉시 멈춤
        moveVelocity = Vector3.zero;
        Debug.Log("💀 게임 오버! 플레이어가 사망했습니다.");
    }

    // ⭐ [수정 완료] 보상방 카드 클릭 시 실제 스탯을 영구 강화해 주는 함수
    public void ApplyReward(string rewardType)
    {
        switch (rewardType)
        {
            case "FireRateUp":
                // 🏹 공격 주기(간격)를 줄여서 공격 속도를 빠르게 만듭니다. (최소 0.1초 제한)
                attackRate = Mathf.Max(0.1f, attackRate - 0.05f);
                Debug.Log($"🏹 곡괭이 선택: 공속 증가! 현재 공격 속도 주기: {attackRate}초");
                break;

            case "MoveSpeedUp":
                // 👟 이동 속도 변수 자체를 누적 증가시킵니다.
                moveSpeed += 1.0f;
                Debug.Log($"👟 삽 선택: 이속 증가! 현재 이동 속도: {moveSpeed}");
                break;

            case "Heal":
                // 📚 최대 체력(maxHealth)을 넘지 않는 선에서 체력을 1 회복시킵니다.
                currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                Debug.Log($"📚 책 선택: 체력 1 회복! 현재 체력: {currentHealth}/{maxHealth}");

                // [나중에 체력 UI 연동 시 여기에 UI 갱신 코드를 넣으면 됩니다]
                break;
        }
    }
}