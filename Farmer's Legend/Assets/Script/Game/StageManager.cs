using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("스테이지 정보")]
    public int currentStage = 1;
    public int currentSubStage = 1;

    [Header("보상 시스템 UI")]
    public GameObject rewardPanel;      // 보상 창 (부모)
    public Button[] rewardButtons;      // 보상 버튼 3개
    public Text stageText;              // 화면에 표시될 "1-1" 텍스트

    // 현재 방에 남은 적 리스트
    private List<GameObject> enemiesInRoom = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        rewardPanel.SetActive(false); // 시작할 땐 보상창 끄기
    }

    void Start()
    {
        UpdateStageUI();
    }

    // 적이 생성될 때 이 함수를 호출해서 리스트에 넣어야 합니다.
    public void RegisterEnemy(GameObject enemy)
    {
        enemiesInRoom.Add(enemy);
    }

    // 적이 죽을 때 이 함수를 호출합니다.
    public void EnemyDied(GameObject enemy)
    {
        enemiesInRoom.Remove(enemy);

        // 모든 적을 처치했다면?
        if (enemiesInRoom.Count <= 0)
        {
            ShowRewards();
        }
    }

    void ShowRewards()
    {
        rewardPanel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지 (보상 고르는 동안)

        // 3개의 랜덤 보상 배치 (중복 없이)
        List<int> rewardIndices = GetRandomIndices(3, 5); // 예: 총 5종의 보상 중 3개 선택

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            int index = rewardIndices[i];
            rewardButtons[i].GetComponentInChildren<Text>().text = "보상 " + index; // 실제로는 보상 이름

            rewardButtons[i].onClick.RemoveAllListeners();
            rewardButtons[i].onClick.AddListener(() => SelectReward(index));
        }
    }

    void SelectReward(int rewardID)
    {
        Debug.Log(rewardID + "번 보상 선택됨!");
        // 여기서 실제로 플레이어의 스탯을 올려주는 로직 실행
        // 예: if(rewardID == 1) player.atkSpeed += 0.2f;

        rewardPanel.SetActive(false);
        Time.timeScale = 1f; // 다시 게임 시작

        NextSubStage();
    }

    void NextSubStage()
    {
        currentSubStage++;
        if (currentSubStage > 3) // 1-3까지 끝났다면
        {
            currentStage++;
            currentSubStage = 1;
            Debug.Log("다음 대스테이지로 이동!");
        }

        UpdateStageUI();
        SpawnNextWave(); // 다음 방 적 소환 로직
    }

    void UpdateStageUI()
    {
        if (stageText != null)
            stageText.text = currentStage + "-" + currentSubStage;
    }

    void SpawnNextWave()
    {
        // 여기서 다음 스테이지의 적들을 생성하거나 플레이어를 이동시킵니다.
        Debug.Log(currentStage + "-" + currentSubStage + " 시작!");
    }

    // 중복 없는 랜덤 인덱스 추출 함수
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