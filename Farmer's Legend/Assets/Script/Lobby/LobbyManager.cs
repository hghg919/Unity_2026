using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Main World Map")]
    public Button[] mainStageButtons; // Stage1, Stage2, Stage3 버튼들

    [Header("Stage Detail Panel")]
    public GameObject stageDetailPanel;
    public TextMeshProUGUI stageTitleText;
    public Image[] subStageIcons; // 1-1, 1-2, 1-3 이미지 아이콘들

    private void Start()
    {
        stageDetailPanel.SetActive(false);
        RefreshMainStages();
    }

    public void RefreshMainStages()
    {
        int unlockedMain = StageManager.Instance.UnlockedMainStage;
        for (int i = 0; i < mainStageButtons.Length; i++)
        {
            mainStageButtons[i].interactable = ((i + 1) <= unlockedMain);
        }
    }

    // 메인 스테이지 버튼 클릭 시 호출
    public void OnClickMainStage(int stageNum)
    {
        StageManager.Instance.SelectedMainStage = stageNum;
        stageTitleText.text = $"Stage {stageNum}";

        stageDetailPanel.SetActive(true);
        RefreshSubStageIcons(stageNum);
    }

    // ★ 핵심: 해당 메인 스테이지의 클리어 기록에 따라 아이콘 색상 변경
    private void RefreshSubStageIcons(int mainStageNum)
    {
        // 이 메인 스테이지에서 내가 "몇 번 서브 스테이지까지 깼었는지" 확인
        int maxClearedSub = StageManager.Instance.GetMaxClearedSubStage(mainStageNum);

        for (int i = 0; i < subStageIcons.Length; i++)
        {
            int subStageNum = i + 1; // 1, 2, 3

            // 현재 순번(subStageNum)이 내가 최고로 클리어한 숫자보다 작거나 같으면 불을 켬
            if (subStageNum <= maxClearedSub)
            {
                subStageIcons[i].color = Color.white; // 원래 색상 (클리어 완료 표시)
            }
            else
            {
                subStageIcons[i].color = new Color(0.25f, 0.25f, 0.25f, 1f); // 어두운 회색 (미클리어 표시)
            }
        }
    }

    // ★ 핵심: [PLAY] 버튼 클릭 시
    public void OnClickPlay()
    {
        // 게임오버 후 재시작이든 처음 시작이든 언제나 "1번 서브 스테이지"부터 출발!
        StageManager.Instance.CurrentSubStage = 1;

        SceneManager.LoadScene("GameScene");
    }

    public void OnClickQuit()
    {
        stageDetailPanel.SetActive(false);
    }
}