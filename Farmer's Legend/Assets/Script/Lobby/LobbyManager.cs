using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("📱 테마별 UI 패널 (카메라 stagePositions 등록 순서와 똑같이 배치)")]
    public GameObject[] themePanels;

    [Header("🎯 전 월드 메인 스테이지 버튼들 (1번부터 9번까지 순서대로 등록)")]
    public Button[] allMainStageButtons;

    [Header("Stage Detail Panel")]
    public GameObject stageDetailPanel;
    public TextMeshProUGUI stageTitleText;
    public Image[] subStageIcons;

    // ⭐⭐⭐ [추가] 1-1, 1-2 텍스트들을 자동으로 바꾸기 위해 인스펙터에서 등록할 슬롯
    [Header("📝 서브 스테이지 번호 텍스트들 (3개 순서대로 등록)")]
    public TextMeshProUGUI[] subStageTexts;

    private void Start()
    {
        stageDetailPanel.SetActive(false);
        RefreshMainStages();
        UpdateThemePanel(0);
    }

    public void RefreshMainStages()
    {
        if (StageManager.Instance == null)
        {
            Debug.LogError("❌ [로비 에러] StageManager 프리팹이 씬에 존재하지 않습니다! DontDestroyOnLoad 작동을 확인하세요.");
            return;
        }

        int unlockedMain = StageManager.Instance.UnlockedMainStage;

        Debug.Log($"💾 [로비 세이브 추적] 하드디스크에서 읽어온 최고 해금 스테이지 수치: {unlockedMain}");
        Debug.Log($"📊 [로비 버튼 추적] 현재 LobbyManager 인스펙터 창에 등록된 버튼 개수: {allMainStageButtons.Length}개");

        if (allMainStageButtons.Length == 0)
        {
            Debug.LogWarning("⚠️ [경고]allMainStageButtons 배열이 비어있습니다! 인스펙터에서 버튼들을 드래그해 넣으셨는지 확인하세요.");
        }

        for (int i = 0; i < allMainStageButtons.Length; i++)
        {
            if (allMainStageButtons[i] == null) continue;

            int stageNum = i + 1;

            if (stageNum <= unlockedMain)
            {
                allMainStageButtons[i].interactable = true;
                allMainStageButtons[i].image.color = Color.white;
            }
            else
            {
                allMainStageButtons[i].interactable = false;
                allMainStageButtons[i].image.color = new Color(0.25f, 0.25f, 0.25f, 0.75f);
            }
        }
    }

    public void UpdateThemePanel(int themeIndex)
    {
        if (themePanels == null) return;

        for (int i = 0; i < themePanels.Length; i++)
        {
            if (themePanels[i] != null)
            {
                themePanels[i].SetActive(i == themeIndex);
            }
        }
    }

    public void OnClickMainStage(int stageNum)
    {
        // [이전 피드백 반영] 일시정지 창이 열려 있다면 스테이지 선택 차단
        PauseMenuController pauseMenu = FindFirstObjectByType<PauseMenuController>();
        if (pauseMenu != null && pauseMenu.pausePanel != null && pauseMenu.pausePanel.activeInHierarchy)
        {
            return;
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.SelectedMainStage = stageNum;
        }
        stageTitleText.text = $"Stage {stageNum}";

        stageDetailPanel.SetActive(true);
        RefreshSubStageIcons(stageNum); // 여기서 스테이지 번호를 넘겨 UI를 그립니다.
    }

    private void RefreshSubStageIcons(int mainStageNum)
    {
        if (StageManager.Instance == null) return;
        int maxClearedSub = StageManager.Instance.GetMaxClearedSubStage(mainStageNum);

        for (int i = 0; i < subStageIcons.Length; i++)
        {
            int subStageNum = i + 1;

            // ⭐⭐⭐ [핵심 추가] 현재 클릭한 대스테이지 번호에 맞춰서 텍스트를 "2-1", "2-2" 등으로 실시간 조립합니다.
            if (subStageTexts != null && i < subStageTexts.Length && subStageTexts[i] != null)
            {
                subStageTexts[i].text = $"{mainStageNum}-{subStageNum}";
            }

            if (subStageNum <= maxClearedSub)
            {
                subStageIcons[i].color = Color.white;
            }
            else
            {
                subStageIcons[i].color = new Color(0.25f, 0.25f, 0.25f, 1f);
            }
        }
    }

    public void OnClickPlay()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.CurrentSubStage = 1;
        }
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickQuit()
    {
        stageDetailPanel.SetActive(false);
    }
}