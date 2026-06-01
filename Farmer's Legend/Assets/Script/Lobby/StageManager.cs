using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 감지를 위한 네임스페이스

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public int UnlockedMainStage = 1;
    public int SelectedMainStage = 1;
    public int CurrentSubStage = 1;

    [Header("🎵 배경음악(BGM) 확장 오디오 설정")]
    public AudioClip titleBGM;
    public AudioClip[] lobbyThemeBGMs;
    public AudioClip[] gameStageBGMs;

    [Header("🏆 결과창 전용 배경음악 설정")]
    public AudioClip victoryBGM;
    public AudioClip defeatBGM;

    private AudioSource bgmSource;
    // ⭐⭐⭐ [B안 핵심 추가] 유니티 오디오 필터 컴포넌트 변수
    private AudioLowPassFilter lowPassFilter;
    private float originalVolume = 1.0f;

    // --- StageManager.cs 내부에 아래 Start 함수를 추가해 줍니다 ---
    private void Start()
    {
        // 짚고 넘어가기: 최초 게임 구동 시 sceneLoaded 이벤트를 타이밍상 놓치기 때문에,
        // 현재 열려있는 씬(TitleScene)을 강제로 한 번 체크하여 브금을 수동 재생합니다.
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnlockedMainStage = PlayerPrefs.GetInt("UnlockedMainStage", 1);

            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = originalVolume;

            // ⭐⭐⭐ [B안 필터 자동 세팅] 
            // 오브젝트에 필터 컴포넌트가 없다면 코드가 스스로 알아서 추가해 줍니다.
            lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }
            lowPassFilter.enabled = false; // 게임 시작할 때는 필터를 꺼둡니다 (선명하게)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 💡 씬이 이동되면 일시정지 필터와 볼륨을 완전 초기화 상태로 돌려놓습니다.
        if (bgmSource != null) bgmSource.volume = originalVolume;
        if (lowPassFilter != null) lowPassFilter.enabled = false;

        if (scene.name == "TitleScene") PlayBGM(titleBGM);
        else if (scene.name == "LobbyScene") PlayLobbyThemeBGM((SelectedMainStage - 1) / 3);
        else if (scene.name == "GameScene") PlayGameStageBGM(SelectedMainStage);
    }

    public void PlayLobbyThemeBGM(int themeIndex)
    {
        if (lobbyThemeBGMs == null || themeIndex < 0 || themeIndex >= lobbyThemeBGMs.Length) return;
        PlayBGM(lobbyThemeBGMs[themeIndex]);
    }

    public void PlayGameStageBGM(int mainStageNum)
    {
        int bgmIndex = mainStageNum - 1;
        if (gameStageBGMs == null || bgmIndex < 0 || bgmIndex >= gameStageBGMs.Length) return;
        PlayBGM(gameStageBGMs[bgmIndex]);
    }

    // ⭐⭐⭐ [완벽한 B안: Low-Pass Filter 연출 제어장치]
    public void SetPauseBGMState(bool isPaused)
    {
        if (bgmSource == null || lowPassFilter == null) return;

        if (isPaused)
        {
            // 1) 일시정지 되었을 때:
            bgmSource.volume = originalVolume * 0.6f;  // 볼륨은 살짝만 줄이고 (60%)
            lowPassFilter.enabled = true;              // 먹먹한 필터 가동!
            lowPassFilter.cutoffFrequency = 800f;      // 800Hz 이상의 고음역대를 싹 깎아버림 (물속 소리 연출)
            Debug.Log("⏸️ [BGM 매니저] 일시정지 - Low-Pass Filter 발동 (물속 사운드)");
        }
        else
        {
            // 2) 게임으로 복귀했을 때:
            bgmSource.volume = originalVolume;         // 볼륨 원상 복구 (100%)
            lowPassFilter.enabled = false;             // 필터 완전히 끄기 (다시 선명하게)
            Debug.Log("▶️ [BGM 매니저] 일시정지 해제 - Filter 해제 (선명한 사운드)");
        }
    }

    public void PlayResultBGM(bool isWin)
    {
        if (bgmSource == null) return;
        if (lowPassFilter != null) lowPassFilter.enabled = false; // 결과창 뜰 때는 필터 끄기

        AudioClip resultClip = isWin ? victoryBGM : defeatBGM;
        if (resultClip != null)
        {
            bgmSource.clip = resultClip;
            bgmSource.volume = originalVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        if (clip != null) bgmSource.Play();
        else bgmSource.Stop();
    }

    public int GetMaxClearedSubStage(int mainStageNum) { return PlayerPrefs.GetInt($"MainStage_{mainStageNum}_ClearedSub", 0); }
    public void ClearSubStage(int mainStageNum, int subStageNum)
    {
        int currentMax = GetMaxClearedSubStage(mainStageNum);
        if (subStageNum > currentMax) PlayerPrefs.SetInt($"MainStage_{mainStageNum}_ClearedSub", subStageNum);
        if (subStageNum == 3 && mainStageNum >= UnlockedMainStage)
        {
            UnlockedMainStage = mainStageNum + 1;
            PlayerPrefs.SetInt("UnlockedMainStage", UnlockedMainStage);
        }
        PlayerPrefs.Save();
    }
}