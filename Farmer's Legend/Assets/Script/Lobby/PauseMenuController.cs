using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요
#if UNITY_EDITOR
using UnityEditor; // 에디터에서 종료 확인을 위해 필요
#endif

public class PauseMenuController : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject pausePanel; // 가운데 뜨는 일시정지 창 (SettingsPanel)

    void Start()
    {
        // 시작할 때는 일시정지 창을 숨깁니다.
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // 1. 일시정지 창 열기 (우측 상단 || 버튼에 연결)
    public void OpenPauseMenu()
    {
        // 로비 매니저가 있을 때만 상세창 체크를 하고, 없을 때(게임씬)는 그냥 통과합니다.
        LobbyManager lobby = FindFirstObjectByType<LobbyManager>();
        if (lobby != null && lobby.stageDetailPanel != null && lobby.stageDetailPanel.activeInHierarchy)
        {
            Debug.LogWarning("⚠️ 스테이지 세부 정보 창이 열려 있어 일시정지 창 오픈을 차단합니다.");
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // 게임 시간을 멈춤
        }
    }

    // 2. 일시정지 창 닫기 (X 버튼에 연결)
    public void ClosePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // 게임 시간을 다시 흐르게 함
        }
    }

    // 3. 타이틀 씬으로 이동 (Title 버튼에 연결)
    public void GoToTitle()
    {
        Time.timeScale = 1f; // 이동 전 시간 초기화는 필수!
        SceneManager.LoadScene("TitleScene");
    }

    // ⭐ [추가] 로비 씬으로 이동 (게임 도중 나갈 때 사용)
    public void GoToLobby()
    {
        Time.timeScale = 1f; // 시간 초기화
        if (StageManager.Instance != null)
        {
            StageManager.Instance.CurrentSubStage = 1; // 진행 중이던 서브스테이지 초기화
        }
        SceneManager.LoadScene("LobbyScene");
    }

    // 4. 게임 종료 (Quit 버튼에 연결)
    public void QuitGame()
    {
        Debug.Log("게임 종료!");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 에디터에서 실행 중일 때
#else
        Application.Quit(); // 빌드된 게임에서 실행 중일 때
#endif
    }
}