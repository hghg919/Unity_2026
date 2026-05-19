using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InGameStageManager : MonoBehaviour
{
    // 이름을 InGameStageManager로 변경하여 모호성 해결!
    public static InGameStageManager Instance;

    [Header("보상 시스템 UI")]
    public GameObject rewardPanel;
    public Button[] rewardButtons;
    public Text stageText;

    private List<GameObject> enemiesInRoom = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        if (rewardPanel != null) rewardPanel.SetActive(false);
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
            ShowRewards();
        }
    }

    void ShowRewards()
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);
        Time.timeScale = 0f;

        List<int> rewardIndices = GetRandomIndices(3, 5);

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            int index = rewardIndices[i];
            rewardButtons[i].GetComponentInChildren<Text>().text = "보상 " + index;

            rewardButtons[i].onClick.RemoveAllListeners();
            rewardButtons[i].onClick.AddListener(() => SelectReward(index));
        }
    }

    void SelectReward(int rewardID)
    {
        Debug.Log(rewardID + "번 보상 선택됨!");
        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1f;

        NextSubStage();
    }

    void NextSubStage()
    {
        // 로비에서 넘어온 기존 StageManager의 데이터와 연동합니다.
        if (StageManager.Instance != null)
        {
            int mainStage = StageManager.Instance.SelectedMainStage;
            int subStage = StageManager.Instance.CurrentSubStage;

            // 데이터 저장 및 경신 호출
            StageManager.Instance.ClearSubStage(mainStage, subStage);

            // 다음 서브 스테이지로 증가
            StageManager.Instance.CurrentSubStage++;
            if (StageManager.Instance.CurrentSubStage > 3)
            {
                StageManager.Instance.CurrentSubStage = 1;
                StageManager.Instance.SelectedMainStage++; // 1-3 깨면 2-1로
            }
        }

        UpdateStageUI();
        SpawnNextWave();
    }

    void UpdateStageUI()
    {
        // 기존 매니저에 저장된 메인-서브 스테이지 정보를 가져와 화면에 띄웁니다.
        if (stageText != null && StageManager.Instance != null)
        {
            stageText.text = StageManager.Instance.SelectedMainStage + "-" + StageManager.Instance.CurrentSubStage;
        }
    }

    void SpawnNextWave()
    {
        Debug.Log("다음 스테이지 적 소환!");
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