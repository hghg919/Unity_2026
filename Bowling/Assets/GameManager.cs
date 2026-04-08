using UnityEngine;
using TMPro; // TMP를 쓰기 위해 반드시 필요!
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Reference")]
    public TextMeshProUGUI scoreText; // 점수 텍스트 연결
    public TextMeshProUGUI roundText; // 라운드 텍스트 연결

    public int totalScore = 0;
    public int currentRound = 1;
    public int maxRounds = 5;

    public List<GameObject> pins = new List<GameObject>();
    private List<Vector3> pinPositions = new List<Vector3>();
    private List<Quaternion> pinRotations = new List<Quaternion>();

    void Awake()
    {
        instance = this;
        foreach (GameObject pin in pins)
        {
            pinPositions.Add(pin.transform.position);
            pinRotations.Add(pin.transform.rotation);
        }
    }

    void Start()
    {
        UpdateUI(); // 시작할 때 UI 초기화
    }

    public void AddScore()
    {
        totalScore++;
        UpdateUI();
    }

    public void NextRound()
    {
        if (currentRound < maxRounds)
        {
            currentRound++;
            UpdateUI();
            ResetPins();
        }
        else
        {
            // 게임 종료 시 메시지 표시
            scoreText.text = "Game Over!";
            roundText.text = $"Final Score: {totalScore}";
        }
    }

    // 텍스트를 화면에 갱신하는 함수
    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {totalScore}";
        if (roundText != null) roundText.text = $"Round: {currentRound} / {maxRounds}";
    }

    void ResetPins()
    {
        for (int i = 0; i < pins.Count; i++)
        {
            pins[i].SetActive(true);
            Rigidbody rb = pins[i].GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            pins[i].transform.position = pinPositions[i];
            pins[i].transform.rotation = pinRotations[i];
            pins[i].GetComponent<PinScript>().isDown = false;
        }
    }
}