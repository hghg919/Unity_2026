using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class InGameStageManager : MonoBehaviour
{
    public static InGameStageManager Instance;

    [System.Serializable]
    public struct RewardData
    {
        public string rewardName;
        public Sprite rewardIcon;
        public string rewardType;
    }

    [System.Serializable]
    public struct MainStageData
    {
        public string stageDebugName;
        public GameObject[] subStageRooms;
    }

    [Header("전체 보상 데이터 풀")]
    public RewardData[] allRewards;

    [Header("보상 UI 슬롯 (통짜 카드 3개 세팅)")]
    public GameObject rewardPanel;
    public Button[] slotButtons;
    public Image[] slotImages;
    public TextMeshProUGUI[] slotTexts;
    public TextMeshProUGUI stageText;

    [Header("슬롯 페이드 효과 설정")]
    public CanvasGroup[] slotCanvasGroups;
    public float inactiveAlpha = 0.3f;

    [Header("룰렛 속도 및 연출 설정")]
    public float individualShuffleDuration = 0.6f;
    public float shuffleSpeed = 0.1f;
    public float punchScale = 1.3f;
    public float punchDuration = 0.25f;

    [Header("진짜 맵 구조 세팅")]
    public GameObject[] themeEnvironments;
    public MainStageData[] allMainStages;

    [Header("🏆 [통합] 결과창 UI 세팅 (성공/사망 공용)")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI replayButtonText;
    public Button replayButton;
    public Button backToLobbyButton;
    public Image[] resultSubStageIcons;

    // ⭐⭐⭐ [추가] 결과창의 1-1, 1-2, 1-3 텍스트들을 자동으로 바꾸기 위해 인스펙터에서 등록할 슬롯
    [Header("📝 결과창 서브 스테이지 번호 텍스트들 (3개 순서대로 등록)")]
    public TextMeshProUGUI[] resultSubStageTexts;

    private List<GameObject> enemiesInRoom = new List<GameObject>();

    private int localMainStage = 1;
    private int localSubStage = 1;

    void Awake()
    {
        Instance = this;
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Start()
    {
        UpdateStageUI();
        SpawnNextWave();
    }

    public void RegisterEnemy(GameObject enemy)
    {
        enemiesInRoom.Add(enemy);
    }

    public void EnemyDied(GameObject enemy)
    {
        enemiesInRoom.Remove(enemy);
        if (enemiesInRoom.Count <= 0)
        {
            int currentSub = StageManager.Instance != null ? StageManager.Instance.CurrentSubStage : localSubStage;

            if (currentSub == 3)
            {
                StartCoroutine(ShowEndGamePanel(true));
            }
            else
            {
                StartCoroutine(ShowRewardsRoutine());
            }
        }
    }

    public void PlayerDied()
    {
        StopAllCoroutines();
        if (rewardPanel != null) rewardPanel.SetActive(false);
        StartCoroutine(ShowEndGamePanel(false));
    }

    IEnumerator ShowRewardsRoutine()
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].interactable = false;
            slotImages[i].transform.localScale = Vector3.one;
            if (slotCanvasGroups != null && slotCanvasGroups.Length > i && slotCanvasGroups[i] != null)
            {
                slotCanvasGroups[i].alpha = inactiveAlpha;
            }
        }

        List<int> finalRewardIndices = GetRandomIndices(slotButtons.Length, allRewards.Length);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotCanvasGroups != null && slotCanvasGroups.Length > i && slotCanvasGroups[i] != null)
            {
                slotCanvasGroups[i].alpha = 1.0f;
            }

            float timer = 0f;
            while (timer < individualShuffleDuration)
            {
                int randomVisualIndex = Random.Range(0, allRewards.Length);
                slotImages[i].sprite = allRewards[randomVisualIndex].rewardIcon;
                slotTexts[i].text = allRewards[randomVisualIndex].rewardName;

                timer += shuffleSpeed;
                yield return new WaitForSecondsRealtime(shuffleSpeed);
            }

            int finalIndex = finalRewardIndices[i];
            RewardData selectedReward = allRewards[finalIndex];

            slotImages[i].sprite = selectedReward.rewardIcon;
            slotTexts[i].text = selectedReward.rewardName;

            yield return StartCoroutine(PunchScaleRoutine(slotImages[i].transform, punchScale, punchDuration));
            yield return new WaitForSecondsRealtime(0.1f);
        }

        Time.timeScale = 0f;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int finalIndex = finalRewardIndices[i];
            RewardData selectedReward = allRewards[finalIndex];

            slotButtons[i].interactable = true;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => SelectReward(selectedReward.rewardType));
        }
    }

    IEnumerator PunchScaleRoutine(Transform targetTransform, float targetScale, float duration)
    {
        Vector3 originScale = Vector3.one;
        Vector3 maxScale = Vector3.one * targetScale;
        float elapsed = 0f;
        float halfDuration = duration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            targetTransform.localScale = Vector3.Lerp(originScale, maxScale, elapsed / halfDuration);
            yield return null;
        }
        targetTransform.localScale = maxScale;

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            targetTransform.localScale = Vector3.Lerp(maxScale, originScale, elapsed / halfDuration);
            yield return null;
        }
        targetTransform.localScale = originScale;
    }

    void SelectReward(string rewardType)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player != null) player.ApplyReward(rewardType);
        }

        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1f;

        NextSubStage();
    }

    void NextSubStage()
    {
        if (StageManager.Instance != null)
        {
            int mainStage = StageManager.Instance.SelectedMainStage;
            int subStage = StageManager.Instance.CurrentSubStage;

            StageManager.Instance.ClearSubStage(mainStage, subStage);
            StageManager.Instance.CurrentSubStage++;
        }
        else
        {
            localSubStage++;
        }

        UpdateStageUI();
        SpawnNextWave();
    }

    // InGameStageManager.cs 내부의 ShowEndGamePanel 함수를 이 코드로 통째로 바꾸시면 됩니다.
    IEnumerator ShowEndGamePanel(bool isWin)
    {
        int mainStage = StageManager.Instance != null ? StageManager.Instance.SelectedMainStage : localMainStage;
        int currentSub = StageManager.Instance != null ? StageManager.Instance.CurrentSubStage : localSubStage;

        if (isWin && StageManager.Instance != null)
        {
            StageManager.Instance.ClearSubStage(mainStage, 3);
        }

        // 1. [기존 구현] 결과창 서브 스테이지 텍스트 자동 치환 (1-1, 1-2, 1-3 등으로 글자 변경)
        if (resultSubStageTexts != null)
        {
            for (int i = 0; i < resultSubStageTexts.Length; i++)
            {
                if (resultSubStageTexts[i] != null)
                {
                    resultSubStageTexts[i].text = $"{mainStage}-{i + 1}";
                }
            }
        }

        Time.timeScale = 0f;
        if (resultPanel != null) resultPanel.SetActive(true);
        if (replayButton != null) replayButton.interactable = false;
        if (backToLobbyButton != null) backToLobbyButton.interactable = false;

        if (resultTitleText != null)
        {
            resultTitleText.text = isWin ? "STAGE CLEAR" : "DEFEATED";
            resultTitleText.color = isWin ? Color.green : Color.red;
        }
        if (replayButtonText != null)
        {
            replayButtonText.text = isWin ? "RePlay" : "ReStart";
        }

        // 2. 일단 모든 메달 아이콘을 기본 회색 상태로 초기화합니다.
        for (int i = 0; i < resultSubStageIcons.Length; i++)
        {
            if (resultSubStageIcons[i] != null)
            {
                resultSubStageIcons[i].color = new Color(0.25f, 0.25f, 0.25f, 1f);
                resultSubStageIcons[i].transform.localScale = Vector3.one;
            }
        }
        yield return new WaitForSecondsRealtime(0.2f);

        // ⭐⭐⭐ [해결책 1 핵심 적용] 하드디스크 세이브 데이터에서 기존 최고 클리어 기록을 조회합니다.
        int maxClearedSub = StageManager.Instance != null ? StageManager.Instance.GetMaxClearedSubStage(mainStage) : 0;

        if (isWin)
        {
            // 스테이지를 이겼을 때는 순차적으로 커지면서 전부 불이 들어오는 연출을 실행합니다.
            for (int i = 0; i < resultSubStageIcons.Length; i++)
            {
                if (resultSubStageIcons[i] == null) continue;
                resultSubStageIcons[i].color = Color.white;
                yield return StartCoroutine(PunchScaleRoutine(resultSubStageIcons[i].transform, punchScale, punchDuration));
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }
        else
        {
            // ⭐ [수정] 플레이어가 죽었을 때: 
            // '기존 최고 기록(maxClearedSub)' 이하이거나, '이번 판에 깨고 넘어왔던 방(roomNum < currentSub)'이라면 불을 켜둡니다!
            for (int i = 0; i < resultSubStageIcons.Length; i++)
            {
                int roomNum = i + 1;
                if (resultSubStageIcons[i] == null) continue;

                if (roomNum <= maxClearedSub || roomNum < currentSub)
                {
                    resultSubStageIcons[i].color = Color.white; // 해금 상태는 하얀색 원래 아이콘
                }
                else
                {
                    resultSubStageIcons[i].color = new Color(0.25f, 0.25f, 0.25f, 1f); // 미클리어는 회색 유지
                }
            }
            yield return null;
        }

        // 결과창 버튼 로직 연결 (기존 코드 유지)
        if (replayButton != null)
        {
            replayButton.interactable = true;
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayStage);
        }

        if (backToLobbyButton != null)
        {
            backToLobbyButton.interactable = true;
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(GoToLobby);
        }
    }

    void ReplayStage()
    {
        Time.timeScale = 1f;
        if (StageManager.Instance != null)
            StageManager.Instance.CurrentSubStage = 1;
        else
            localSubStage = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToLobby()
    {
        Time.timeScale = 1f;
        if (StageManager.Instance != null)
        {
            StageManager.Instance.CurrentSubStage = 1;
        }
        SceneManager.LoadScene("LobbyScene");
    }

    void SpawnNextWave()
    {
        int mainStageNum = localMainStage;
        int subStageNum = localSubStage;

        if (StageManager.Instance != null)
        {
            mainStageNum = StageManager.Instance.SelectedMainStage;
            subStageNum = StageManager.Instance.CurrentSubStage;
        }

        int themeIndex = (mainStageNum - 1) / 3;
        for (int i = 0; i < themeEnvironments.Length; i++)
        {
            if (themeEnvironments[i] != null)
            {
                themeEnvironments[i].SetActive(i == themeIndex);
            }
        }

        int mainIndex = mainStageNum - 1;
        int subIndex = subStageNum - 1;

        if (allMainStages != null && allMainStages.Length > mainIndex)
        {
            for (int i = 0; i < allMainStages.Length; i++)
            {
                if (allMainStages[i].subStageRooms == null) continue;

                for (int j = 0; j < allMainStages[i].subStageRooms.Length; j++)
                {
                    if (allMainStages[i].subStageRooms[j] != null)
                    {
                        bool isCurrentRoom = (i == mainIndex && j == subIndex);
                        allMainStages[i].subStageRooms[j].SetActive(isCurrentRoom);
                    }
                }
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObj.transform.position = new Vector3(0f, playerObj.transform.position.y, 0f);
        }
    }

    void UpdateStageUI()
    {
        if (stageText != null)
        {
            if (StageManager.Instance != null)
                stageText.text = StageManager.Instance.SelectedMainStage + "-" + StageManager.Instance.CurrentSubStage;
            else
                stageText.text = localMainStage + "-" + localSubStage + " (Test)";
        }
    }

    List<int> GetRandomIndices(int count, int total)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < total; i++) list.Add(i);

        List<int> result = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int rnd = Random.Range(0, list.Count);
            result.Add(list[rnd]);
            list.RemoveAt(rnd);
        }
        return result;
    }
}