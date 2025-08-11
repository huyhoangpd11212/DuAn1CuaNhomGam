using UnityEngine;

[System.Serializable]
public class GameAudio
{
    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip gameOverSound;
    public AudioClip gameWinSound;
    public AudioClip newHighScoreSound;
    
    [Header("Gameplay Sounds")]
    public AudioClip playerShootSound;
    public AudioClip enemyDeathSound;
    public AudioClip playerHitSound;
    public AudioClip powerUpSound;
    
    [Header("Background Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;
}

public class SimpleAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Audio Clips")]
    [SerializeField] private GameAudio gameAudio;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    public static SimpleAudioManager instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    private void SetupAudioSources()
    {
        // Tạo AudioSource nếu chưa có
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        UpdateVolumes();
    }
    
    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
            
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
    }
    
    // Music Methods
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    // SFX Methods
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeMultiplier);
        }
    }
    
    // Convenience Methods
    public void PlayButtonClick() => PlaySFX(gameAudio.buttonClickSound);
    public void PlayGameOver() => PlaySFX(gameAudio.gameOverSound);
    public void PlayGameWin() => PlaySFX(gameAudio.gameWinSound);
    public void PlayNewHighScore() => PlaySFX(gameAudio.newHighScoreSound);
    public void PlayPlayerShoot() => PlaySFX(gameAudio.playerShootSound, 0.5f);
    public void PlayEnemyDeath() => PlaySFX(gameAudio.enemyDeathSound);
    public void PlayPlayerHit() => PlaySFX(gameAudio.playerHitSound);
    public void PlayPowerUp() => PlaySFX(gameAudio.powerUpSound);
    
    // Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
    
    // Load saved volume settings
    private void Start()
    {
        LoadVolumeSettings();
        
        // Play background music nếu có
        if (gameAudio.gameplayMusic != null)
        {
            PlayMusic(gameAudio.gameplayMusic);
        }
    }
    
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateVolumes();
    }
}
