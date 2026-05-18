using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelLayout
    {
        public int mainStage;
        public int subStage;
        public GameObject layoutPrefab; // 해당 스테이지에 등장할 지형물/적들이 배치된 프리팹
    }

    public LevelLayout[] allLayouts; // 인스펙터에서 모든 스테이지 레이아웃 프리팹을 등록
    public Transform spawnPoint;     // 맵 중앙 등 프리팹이 생성될 기준 위치

    private void Start()
    {
        // 에러가 났던 부분입니다. 이제 아래에 실제 함수가 있으므로 에러가 사라집니다!
        GenerateStage();
    }

    // 맵 구성을 동적으로 생성하는 함수
    private void GenerateStage()
    {
        // StageManager로부터 어떤 스테이지를 선택해 들어왔는지 확인
        int targetMain = StageManager.Instance.SelectedMainStage;
        int targetSub = StageManager.Instance.CurrentSubStage;

        GameObject prefabToSpawn = null;

        // 알맞은 프리팹 찾기
        foreach (var layout in allLayouts)
        {
            if (layout.mainStage == targetMain && layout.subStage == targetSub)
            {
                prefabToSpawn = layout.layoutPrefab;
                break;
            }
        }

        if (prefabToSpawn != null)
        {
            // 찾은 프리팹을 맵에 동적 생성
            Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity, this.transform);
        }
        else
        {
            Debug.LogError($"{targetMain}-{targetSub} 레이아웃 프리팹이 등록되지 않았습니다!");
        }
    }

    // 플레이어가 현재 구역(예: 1-1)의 적을 모두 물리치고 승리했을 때 호출되는 함수
    public void OnSubStageWin()
    {
        int currentMain = StageManager.Instance.SelectedMainStage;
        int currentSub = StageManager.Instance.CurrentSubStage;

        // 1. 현재 깬 단계를 저장소에 기록 경신 시도
        StageManager.Instance.ClearSubStage(currentMain, currentSub);

        // 2. 다음 단계로 전진
        if (currentSub < 3)
        {
            StageManager.Instance.CurrentSubStage++;
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            // 1-3까지 다 깼다면 로비로 복귀
            SceneManager.LoadScene("LobbyScene");
        }
    }

    // 플레이어의 체력이 0이 되어 게임오버 되었을 때 호출되는 함수
    public void OnPlayerGameOver()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}