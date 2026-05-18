using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요
#if UNITY_EDITOR
using UnityEditor; // 에디터에서 종료 확인을 위해 필요
#endif

public class PauseMenuController : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject pausePanel; // 가운데 뜨는 일시정지 창

    void Start()
    {
        // 시작할 때는 일시정지 창을 숨깁니다.
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // 1. 일시정지 창 열기 (우측 상단 || 버튼에 연결)
    public void OpenPauseMenu()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 게임 시간을 멈춤 (필요 시)
    }

    // 2. 일시정지 창 닫기 (X 버튼에 연결)
    public void ClosePauseMenu()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // 게임 시간을 다시 흐르게 함
    }

    // 3. 타이틀 씬으로 이동 (Title 버튼에 연결)
    public void GoToTitle()
    {
        Time.timeScale = 1f; // 이동 전 시간 초기화는 필수!
        SceneManager.LoadScene("TitleScene"); // 씬 이름이 정확해야 함
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