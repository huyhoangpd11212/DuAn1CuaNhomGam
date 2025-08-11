using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Nguồn phát nhạc nền
    [SerializeField] private AudioSource sfxSource;   // Nguồn phát hiệu ứng âm thanh

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;      // NhạcNềnBackGround.mp3
    public AudioClip playerShoot;          // Bắnđạn.wav
    public AudioClip enemyExplosion;       // BắnRơiEnemy.wav
    public AudioClip playerExplosion;      // Playerbithuong.wav
    public AudioClip gameOverSound;        // GameOver.wav
    public AudioClip victorySound;         // WinManNho.wav
    public AudioClip alertSound;           // mixkit-explainer-video-game-alert-sweep-236.wav
    // Thêm các AudioClip khác nếu cần

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Singleton pattern để dễ dàng truy cập từ các script khác
    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ AudioManager tồn tại khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Tạo AudioSource nếu chưa có
        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        // Tạo Music Source nếu chưa có
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        
        // Tạo SFX Source nếu chưa có
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
        
        // Cấu hình Music Source
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        
        // Cấu hình SFX Source
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        // Bật nhạc nền khi game bắt đầu
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("🎵 Background music started playing");
        }
        else
        {
            Debug.LogWarning("🎵 Background music or music source is missing!");
        }
    }

    // Hàm để phát một hiệu ứng âm thanh
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeMultiplier * sfxVolume);
        }
        else if (clip == null)
        {
            Debug.LogWarning("🔊 Trying to play null AudioClip!");
        }
    }

    // Các hàm tiện ích có thể gọi từ nơi khác
    public void PlayPlayerShootSound()
    {
        PlaySFX(playerShoot, 0.5f); // Volume nhỏ hơn vì bắn nhiều
        Debug.Log("🔫 Player shoot sound played");
    }

    public void PlayEnemyExplosionSound()
    {
        PlaySFX(enemyExplosion, 0.8f);
        Debug.Log("💥 Enemy explosion sound played");
    }

    public void PlayPlayerExplosionSound()
    {
        PlaySFX(playerExplosion, 1f);
        Debug.Log("💀 Player explosion sound played");
    }
    
    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound, 0.9f);
        Debug.Log("😵 Game over sound played");
    }
    
    public void PlayVictorySound()
    {
        PlaySFX(victorySound, 1f);
        Debug.Log("🎉 Victory sound played");
    }
    
    public void PlayAlertSound()
    {
        PlaySFX(alertSound, 0.7f);
        Debug.Log("⚠️ Alert sound played");
    }
    
    // Hàm để điều khiển âm lượng
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        Debug.Log($"🎵 Music volume set to: {musicVolume}");
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        Debug.Log($"🔊 SFX volume set to: {sfxVolume}");
    }

    // Load saved volume settings
    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // Music control methods
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Method để thay đổi background music
    public void PlayBackgroundMusic(AudioClip newMusicClip)
    {
        if (musicSource != null && newMusicClip != null)
        {
            musicSource.clip = newMusicClip;
            musicSource.Play();
            Debug.Log($"🎵 Background music changed to: {newMusicClip.name}");
        }
    }
}
