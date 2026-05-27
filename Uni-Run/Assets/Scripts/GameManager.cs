using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // UI 클릭 방지용
using System.Collections.Generic;
using System;

[System.Serializable]
public class GameRecord
{
    public string name;
    public int score;
    public string date;

    public GameRecord(string name, int score, string date)
    {
        this.name = name;
        this.score = score;
        this.date = date;
    }
}

[System.Serializable]
public class RecordListWrapper
{
    public List<GameRecord> records = new List<GameRecord>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameover = false;
    public TextMeshProUGUI scoreText;
    public GameObject gameoverUI; // 기존 "GAMEOVER! JUMP TO RESTART" 오브젝트

    // ====== [추가] 배경음악 슬롯 생성 ======
    [Header("오디오 설정")]
    public AudioSource backgroundMusic;
    // ===================================

    [Header("1단계: 이름 입력 패널 UI")]
    public GameObject inputPanel;            // 이름 입력창, 저장, 취소가 담긴 패널
    public TMP_InputField nameInputField;    // 이름 입력 칸

    [Header("2단계: 랭킹 기록창 패널 UI")]
    public GameObject recordPanel;           // 랭킹 리스트와 다시시작 버튼이 담긴 패널
    public TextMeshProUGUI recordTextList;   // 랭킹 텍스트가 출력될 곳

    private int score = 0;
    private const string SaveKey = "UniRunLocalSaveData";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 게임 오버 상태에서 마우스 클릭을 했을 때
        if (isGameover && Input.GetMouseButtonDown(0))
        {
            // ⭐ 핵심 조건 변경: 
            // 랭킹 기록창(recordPanel)이 꺼져있고, UI 위를 클릭한 게 아닐 때만 [맵 클릭 재시작] 허용!
            if (!recordPanel.activeSelf && !EventSystem.current.IsPointerOverGameObject())
            {
                RestartGame();
            }
        }
    }

    public void AddScore(int newScore)
    {
        if (!isGameover)
        {
            score += newScore;
            scoreText.text = "Score : " + score;
        }
    }

    // 플레이어 캐릭터가 사망했을 때 최초 실행
    public void OnPlayerDead()
    {
        isGameover = true;

        // ====== [추가] 게임오버 시 배경음악 정지 ======
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
        // ===========================================

        gameoverUI.SetActive(true);  // "GAMEOVER! JUMP TO RESTART" 출력
        inputPanel.SetActive(true);  // 이름 입력 패널 켜기
        recordPanel.SetActive(false); // 랭킹 기록창은 아직 숨김
    }

    // 💾 [저장 버튼] 눌렀을 때 실행 (로컬 PC 저장 + 기록창 띄우기)
    public void SaveRecord()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) playerName = "Unknown";

        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 로컬 PC 저장 로직
        RecordListWrapper currentData = LoadRecords();
        currentData.records.Add(new GameRecord(playerName, score, currentDate));
        currentData.records.Sort((a, b) => b.score.CompareTo(a.score));

        string json = JsonUtility.ToJson(currentData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        // 🔄 화면 전환 흐름
        gameoverUI.SetActive(false);  // 기존 글자 숨기기
        inputPanel.SetActive(false);  // 이름 입력 패널 끄기
        recordPanel.SetActive(true);  // ⭐ 기록창 패널 켜기 (이제 맵 클릭 재시작은 안 됨)

        UpdateRecordDisplay(currentData);
    }

    // ❌ [취소 버튼] 눌렀을 때 실행
    public void CancelInput()
    {
        inputPanel.SetActive(false);  // 이름 입력 패널만 조용히 닫기
        // gameoverUI("JUMP TO RESTART")는 계속 켜져있고 recordPanel은 꺼져있으므로, 
        // 이제 맵 아무 데나 클릭하면 Update()에 의해 게임이 재시작됩니다.
    }

    // 🔄 [다시 시작 버튼] 눌렀을 때 실행 (기록창 전용)
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private RecordListWrapper LoadRecords()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<RecordListWrapper>(json);
        }
        return new RecordListWrapper();
    }

    private void UpdateRecordDisplay(RecordListWrapper data)
    {
        if (recordTextList == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < data.records.Count; i++)
        {
            // ✨ 앞에 있던 숫자(1., 2.)를 제거하고 이름, 점수, 날짜+시간만 한 줄로 구성합니다.
            sb.AppendLine($"{data.records[i].name}\t\t{data.records[i].score}\t\t{data.records[i].date}");
        }

        recordTextList.text = sb.ToString();
    }
}