using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    // 메인 스테이지 해금 상황 (1을 깨면 2가 열림)
    public int UnlockedMainStage = 1;

    // 현재 플레이어가 선택한 메인 스테이지 (로비 창 표시용)
    public int SelectedMainStage = 1;

    // 현재 게임 창에서 돌아가고 있는 서브 스테이지 (게임 씬 내부 진행용)
    public int CurrentSubStage = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnlockedMainStage = PlayerPrefs.GetInt("UnlockedMainStage", 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetMaxClearedSubStage(int mainStageNum)
    {
        return PlayerPrefs.GetInt($"MainStage_{mainStageNum}_ClearedSub", 0);
    }

    // 🛠️ [조건식 보완] 어떤 예외 상황에서도 안전하게 다음 스테이지를 해금하도록 수정 완료!
    public void ClearSubStage(int mainStageNum, int subStageNum)
    {
        int currentMax = GetMaxClearedSubStage(mainStageNum);

        if (subStageNum > currentMax)
        {
            PlayerPrefs.SetInt($"MainStage_{mainStageNum}_ClearedSub", subStageNum);
        }

        // ⭐ [변경] 칼 같은 칼매칭 대신, 현재 깬 스테이지가 해금 수치보다 크거나 같으면 무조건 다음 단계 해금!
        if (subStageNum == 3 && mainStageNum >= UnlockedMainStage)
        {
            UnlockedMainStage = mainStageNum + 1;
            PlayerPrefs.SetInt("UnlockedMainStage", UnlockedMainStage);
            Debug.Log($"🔓 [세이브 성공] 다음 대스테이지 해금 완료! 현재 해금된 최고 스테이지: {UnlockedMainStage}");
        }

        PlayerPrefs.Save();
    }
}