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

    // 특정 메인 스테이지의 최고 클리어 서브 스테이지 번호를 가져오는 함수
    public int GetMaxClearedSubStage(int mainStageNum)
    {
        // 깨본 적이 없다면 0 리턴
        return PlayerPrefs.GetInt($"MainStage_{mainStageNum}_ClearedSub", 0);
    }

    // 서브 스테이지를 클리어했을 때 기록을 경신하는 함수
    public void ClearSubStage(int mainStageNum, int subStageNum)
    {
        int currentMax = GetMaxClearedSubStage(mainStageNum);

        // 이번에 깬 스테이지가 기존 기록보다 높다면 기록 경신!
        if (subStageNum > currentMax)
        {
            PlayerPrefs.SetInt($"MainStage_{mainStageNum}_ClearedSub", subStageNum);
        }

        // 만약 1-3까지 다 깼다면 다음 메인 스테이지(Stage 2)를 해금
        if (subStageNum == 3 && mainStageNum == UnlockedMainStage)
        {
            UnlockedMainStage = mainStageNum + 1;
            PlayerPrefs.SetInt("UnlockedMainStage", UnlockedMainStage);
        }

        PlayerPrefs.Save();
    }
}