using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 이동하기 위해 꼭 필요한 네임스페이스입니다.

public class TitleManager : MonoBehaviour
{
    // Start 버튼을 눌렀을 때 실행될 함수
    public void OnClickStart()
    {
        // ⭐ 효과음 추가: 일반 클릭음 (Pitch = 1.0f)
        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.UIClick);
        }

        Debug.Log("Start 버튼 클릭됨! 게임 씬으로 이동합니다.");
        SceneManager.LoadScene("LobbyScene");
    }

    // Quit 버튼을 눌렀을 때 실행될 함수
    public void OnClickQuit()
    {
        // ⭐ 효과음 추가: 종료 버튼 클릭음 (Pitch = 1.0f)
        if (StageManager.Instance != null)
        {
            StageManager.Instance.PlaySFX(StageManager.SFXType.UIClick);
        }

        Debug.Log("Quit 버튼 클릭됨! 게임을 종료합니다.");
        Application.Quit();
    }
}