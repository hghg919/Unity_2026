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
    public Image[] hpImages; // 인스펙터에서 구급상자 이미지 3개 등록

    [Header("타격감 연출 (URP 무결점 업그레이드)")]
    // 뼈대 속 모자까지 포함한 모든 세부 머티리얼과 원본 색상을 동적으로 추적합니다.
    private List<Material> allMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private Coroutine flashCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth; // 시작할 때 체력 풀피로 설정

        // ⭐⭐⭐ [구조 무력화 세팅] 뼈대(Hips_int) 속에 숨은 모자까지 Renderer를 전부 전수조사합니다.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                // r.materials를 호출하면 해당 부위가 가진 1개 또는 여러 개의 머티리얼 인스턴스를 모두 가져옵니다.
                foreach (Material mat in r.materials)
                {
                    if (mat != null)
                    {
                        allMaterials.Add(mat);

                        // URP 쉐이더 특성을 고려하여 _BaseColor가 있으면 그것을, 없으면 기본 color를 원본으로 기억합니다.
                        Color origColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                        originalColors.Add(origColor);
                    }
                }
            }
        }

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
        if (InGameStageManager.Instance == null) return;
        List<GameObject> enemies = InGameStageManager.Instance.EnemiesInRoom;

        GameObject closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue; // 안전장치

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

            // ⭐⭐⭐ [오타 수정] lookDir => 를 지우고 targetDir만 깔끔하게 넣어줍니다.
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

        // 피격 시 붉은색 플래시 코루틴을 켭니다.
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(PlayerFlashRoutine());

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

    // 현재 체력 수치에 맞춰 구급상자 이미지를 켜고 끄는 함수
    void UpdateHpUI()
    {
        if (hpImages == null || hpImages.Length == 0) return;

        for (int i = 0; i < hpImages.Length; i++)
            hpImages[i].gameObject.SetActive(true);

        for (int i = 0; i < hpImages.Length; i++)
        {
            if (i < currentHealth)
            {
                hpImages[i].color = Color.white;
            }
            else
            {
                hpImages[i].color = new Color(0.25f, 0.25f, 0.25f, 0.3f);
            }
        }
    }

    // ⭐⭐⭐ [핵심 수정] 수집된 모든 하위 머티리얼 슬롯의 URP 프로퍼티를 동시에 변환하는 코루틴
    IEnumerator PlayerFlashRoutine()
    {
        Color damageColor = new Color(1f, 0.3f, 0.3f, 1f);

        // 1단계: 수집된 모든 부위의 모든 머티리얼을 타격 컬러로 변경 (URP 대응 포함)
        for (int i = 0; i < allMaterials.Count; i++)
        {
            if (allMaterials[i] != null)
            {
                if (allMaterials[i].HasProperty("_BaseColor"))
                    allMaterials[i].SetColor("_BaseColor", damageColor);
                else
                    allMaterials[i].color = damageColor;
            }
        }

        // 0.1초 대기
        yield return new WaitForSecondsRealtime(0.1f);

        // 2단계: 수집된 모든 머티리얼을 각자의 원래 색상으로 안전 복구
        for (int i = 0; i < allMaterials.Count; i++)
        {
            if (allMaterials[i] != null)
            {
                if (allMaterials[i].HasProperty("_BaseColor"))
                    allMaterials[i].SetColor("_BaseColor", originalColors[i]);
                else
                    allMaterials[i].color = originalColors[i];
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
            case "Heal":
                currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
                UpdateHpUI();
                break;
        }
    }
}