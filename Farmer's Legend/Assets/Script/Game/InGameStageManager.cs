using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // ⭐ [추가] 씬 이동(로비로 가기)을 위해 무조건 필요합니다!

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

    [Header("⭐ 결과창 UI 세팅")]
    public GameObject resultPanel;        // 인펙터에서 새로 만들 결과창 패널 연결
    public Button backToLobbyButton;     // 결과창 안에 있는 '로비로 가기' 버튼 연결

    private List<GameObject> enemiesInRoom = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false); // 시작할 땐 결과창 꺼두기
    }

    void Start()
    {
        UpdateStageUI();
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
            StartCoroutine(ShowRewardsRoutine());
        }
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

    // 🎯 [수정됨] 1-3 클리어 시 다음 맵을 틀지 않고 결과창으로 빠지는 핵심 로직
    void NextSubStage()
    {
        if (StageManager.Instance != null)
        {
            int mainStage = StageManager.Instance.SelectedMainStage;
            int subStage = StageManager.Instance.CurrentSubStage;

            // 1. 현재 대스테이지의 해당 방 클리어 기록 저장 (1-3 클리어 데이터 기록!)
            StageManager.Instance.ClearSubStage(mainStage, subStage);

            // 2. 🔥 [핵심 브레이크] 만약 방금 깬 방이 3번 방(-3) 이라면?
            if (subStage == 3)
            {
                // 다음 대스테이지 해금은 이미 세이브 데이터(ClearSubStage) 함수 내부에서 알아서 처리해 줍니다!
                ShowResultScreen();
                return; // ⭐ 중요: 뒤에 있는 SpawnNextWave()를 실행하지 않고 여기서 함수를 강제 종료합니다!
            }

            // 3. 3번 방이 아닐 때만(1-1, 1-2 일 때만) 다음 세부 방 번호로 증가
            StageManager.Instance.CurrentSubStage++;
        }

        UpdateStageUI();
        SpawnNextWave();
    }

    // 🏆 결과창을 화면에 띄우는 함수
    void ShowResultScreen()
    {
        Time.timeScale = 0f; // 게임 일시정지

        if (resultPanel != null) resultPanel.SetActive(true); // 결과창 On

        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(GoToLobby); // 버튼에 로비 이동 함수 연결
        }
    }

    // 🚪 버튼을 누르면 실제로 로비 씬으로 퇴장하는 함수
    void GoToLobby()
    {
        Time.timeScale = 1f; // 멈췄던 시간 다시 흐르게 원상복구

        // 다음 게임 진입을 위해 서브 스테이지 진행 번호는 깔끔하게 1번 방으로 초기화해 둡니다.
        if (StageManager.Instance != null)
        {
            StageManager.Instance.CurrentSubStage = 1;
        }

        // 유저님의 실제 로비 씬 파일 이름인 "LobbyScene" 으로 로드!
        SceneManager.LoadScene("LobbyScene");
    }

    void SpawnNextWave()
    {
        if (StageManager.Instance == null) return;

        int mainStageNum = StageManager.Instance.SelectedMainStage;
        int subStageNum = StageManager.Instance.CurrentSubStage;

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

        void SpawnNextWave()
        {
            // ... (기존 테마 및 서브방 On/Off 로직) ...

            // ⭐ [추가] 다음 방이 열릴 때 플레이어 위치를 시작 지점으로 강제 강제 이동!
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // 맵 중앙(0,0,0) 혹은 원하시는 시작 좌표(예: Vector3.zero)로 이동시킵니다.
                playerObj.transform.position = new Vector3(0f, playerObj.transform.position.y, 0f);
                Debug.Log("🤠 플레이어를 다음 방 시작 위치로 이동시켰습니다.");
            }
        }
    }

    void UpdateStageUI()
    {
        if (stageText != null && StageManager.Instance != null)
        {
            stageText.text = StageManager.Instance.SelectedMainStage + "-" + StageManager.Instance.CurrentSubStage;
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