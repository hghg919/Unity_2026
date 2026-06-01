using UnityEngine;

public class TitleBGMPlayer : MonoBehaviour
{
    [Header("🎵 타이틀 전용 배경음악")]
    public AudioClip titleBGM;

    private AudioSource audioSource;

    private void Awake()
    {
        // 1. 오디오 소스가 없다면 자동으로 추가해줍니다.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = titleBGM;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // ⭐⭐⭐ [음량 조절 걱정 해결!]
        // 유저의 세팅창 스크립트에서 저장하는 "BGM 볼륨 Key값"을 여기에 적어주세요.
        // (만약 세팅창에서 쓰는 볼륨 저장 키 이름이 다르다면 "BGMVolume"을 그걸로 변경)
        float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        audioSource.volume = savedVolume;
    }

    private void Start()
    {
        // ⭐⭐⭐ [예외 처리 핵심]
        // 로비 씬에 갔다가 다시 타이틀로 돌아온 상황이라면? 
        // 그때는 파괴되지 않고 살아남은 'StageManager'가 타이틀 곡을 틀어줄 테니,
        // 이 타이틀 전용 플레이어는 조용히 자신을 파괴하여 음악이 겹치는 걸 막습니다.
        if (StageManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 게임을 처음 켰을 때(StageManager가 아직 없을 때)만 순수하게 브금을 재생합니다.
        if (titleBGM != null)
        {
            audioSource.Play();
            Debug.Log("🎬 [타이틀 플레이어] 게임 최초 구동 확인 - 타이틀 브금을 독자적으로 재생합니다.");
        }
    }
}