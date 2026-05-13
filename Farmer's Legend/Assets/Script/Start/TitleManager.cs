using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 이동하기 위해 꼭 필요한 네임스페이스입니다.

public class TitleManager : MonoBehaviour
{
    // Start 버튼을 눌렀을 때 실행될 함수
    public void OnClickStart()
    {
        Debug.Log("Start 버튼 클릭됨! 게임 씬으로 이동합니다.");

        // "GameScene" 부분은 이동할 씬의 정확한 이름으로 바꿔주세요 (예: LobbyScene)
        SceneManager.LoadScene("LobbyScene");
    }

    // Quit 버튼을 눌렀을 때 실행될 함수
    public void OnClickQuit()
    {
        Debug.Log("Quit 버튼 클릭됨! 게임을 종료합니다.");

        // 에디터에서는 이 코드가 작동하지 않는 것처럼 보일 수 있지만 빌드하면 정상 작동합니다.
        Application.Quit();
    }
}