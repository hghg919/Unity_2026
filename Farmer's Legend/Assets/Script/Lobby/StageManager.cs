using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    // 📌 깔끔한 호출을 위한 효과음 종류 정의 (기획서 반영)
    public enum SFXType
    {
        PlayerShoot,    // 0: 투사체 발사음
        PlayerHit,      // 1: 플레이어 피격음
        PlayerDeath,    // 2: 플레이어 사망음
        EnemyHit,       // 3: 몬스터 피격음
        EnemyDeath,     // 4: 몬스터 사망음
        BossCharge,     // 5: 보스 초고속 돌진음
        UIClick,        // 6: 통합 버튼 클릭음
        RouletteTick    // 7: ⭐⭐⭐ [추가] 룰렛 회전 틱! 틱! 소리
    }

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

    // ⭐⭐⭐ [효과음용 슬롯 추가] 열거형 순서대로 유니티 인스펙터에 넣을 슬롯 (크기: 7)
    [Header("🔊 효과음(SFX) 에셋 설정")]
    public AudioClip[] sfxClips;

    private AudioSource bgmSource;
    private AudioSource sfxSource;        // ⭐ 효과음 전용 플레이어
    private AudioLowPassFilter lowPassFilter;
    private float originalVolume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnlockedMainStage = PlayerPrefs.GetInt("UnlockedMainStage", 1);

            // 1. BGM 플레이어 세팅
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = originalVolume;

            // ⭐ 2. 효과음 플레이어 세팅 (효과음은 루프하지 않고 바로 나옴)
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 1.0f; // 효과음 기본 볼륨은 짱짱하게 100%

            // 3. 필터 세팅
            lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null) lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.enabled = false;
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
        if (bgmSource != null) bgmSource.volume = originalVolume;
        if (lowPassFilter != null) lowPassFilter.enabled = false;

        if (scene.name == "TitleScene") PlayBGM(titleBGM);
        else if (scene.name == "LobbyScene") PlayLobbyThemeBGM((SelectedMainStage - 1) / 3);
        else if (scene.name == "GameScene") PlayGameStageBGM(SelectedMainStage);
    }

    // ⭐⭐⭐ [언제 어디서나 효과음 빵빵 터트리는 마법의 함수]
    // 사용법 예시: StageManager.Instance.PlaySFX(StageManager.SFXType.EnemyHit);
    public void PlaySFX(SFXType type, float customPitch = 1.0f)
    {
        if (sfxSource == null || sfxClips == null) return;

        int index = (int)type;
        if (index < 0 || index >= sfxClips.Length || sfxClips[index] == null) return;

        // 음정(Pitch) 조절 기능 추가 (UI 닫기 등 변칙용 기본값 1.0f)
        sfxSource.pitch = customPitch;

        // PlayOneShot은 여러 소리가 겹쳐도 이전 소리를 끊지 않고 동시 연주해줍니다.
        sfxSource.PlayOneShot(sfxClips[index]);
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

    public void SetPauseBGMState(bool isPaused)
    {
        if (bgmSource == null || lowPassFilter == null) return;

        if (isPaused)
        {
            bgmSource.volume = originalVolume * 0.6f;
            lowPassFilter.enabled = true;
            lowPassFilter.cutoffFrequency = 800f;
        }
        else
        {
            bgmSource.volume = originalVolume;
            lowPassFilter.enabled = false;
        }
    }

    public void PlayResultBGM(bool isWin)
    {
        if (bgmSource == null) return;
        if (lowPassFilter != null) lowPassFilter.enabled = false;

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