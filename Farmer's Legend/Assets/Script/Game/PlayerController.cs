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
        // 1. 키보드 입력 받기 (마우스 꾹 누르기는 차후 확장)
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
        // 물리 이동 및 회전
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z); // 유니티 6에서는 velocity 대신 linearVelocity 사용 가능

        if (isMoving)
        {
            // 이동 방향 바라보기
            Quaternion newRotation = Quaternion.LookRotation(moveVelocity);
            rb.MoveRotation(newRotation);
        }
    }

    void AttackClosestEnemy()
    {
        // [과제 부연] 주변에 있는 'Enemy' 태그를 가진 적 중 가장 가까운 적을 찾음
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
            // 적을 바라보게 회전
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
        moveVelocity = Vector3.zero; // 이동 멈춤
        Debug.Log("💀 게임 오버! 플레이어가 사망했습니다.");

        // TODO: 나중에 여기에 "게임 오버 팝업창" 띄우는 코드가 들어갈 예정입니다.
    }
}