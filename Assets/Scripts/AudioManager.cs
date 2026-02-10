using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer & Routing")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup ttsGroup;
    public AudioMixerGroup ambienceGroup;

    [Header("Snapshots")]
    public AudioMixerSnapshot menuSnapshot;
    public AudioMixerSnapshot gameplaySnapshot;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource ttsSampleSource; // Dedicated for sample feedback

    [Header("Feedback Clips")]
    public AudioClip buttonPopClip; // button pop sound
    public AudioClip ttsSampleClip; // A short voice clip (e.g., "Ready")
    public AudioClip ambienceSampleClip; // ambience clip

    private float lastSampleTime;
    private const float sampleDelay = 0.15f; // Prevents "machine gun" sounds when sliding

    private void Awake()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isMenuScene = (sceneName == "_MainMenu" || sceneName == "_Settings" || sceneName == "_Credits");

        if (isMenuScene)
        {
            // MENU LOGIC: Persist across menu scenes
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // GAMEPLAY LOGIC: Local manager takes priority
            // If an old Menu manager followed us here, kill it
            if (Instance != null && Instance.gameObject.scene.name == "DontDestroyOnLoad")
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
            // Note: We do NOT call DontDestroyOnLoad here
        }
    }

    private void Start()
    {
        SyncVolumesWithPrefs();
    }

    public void SyncVolumesWithPrefs()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.8f));
        SetTTSVolume(PlayerPrefs.GetFloat("TTSVol", 1.0f));
        SetAmbienceVolume(PlayerPrefs.GetFloat("AmbienceVol", 0.6f));
    }


    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        // Start the fade-in process
        StartCoroutine(FadeInSource(musicSource, PlayerPrefs.GetFloat("MusicVol", 0.5f)));
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;

        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.Play();

        // Start the fade-in process
        StartCoroutine(FadeInSource(ambienceSource, PlayerPrefs.GetFloat("AmbienceVol", 0.6f)));
    }

    private IEnumerator FadeInSource(AudioSource source, float targetLinearVolume)
    {
        float duration = 2.0f; // Seconds to reach full volume
        float currentTime = 0;

        // We start at 0 and move toward 1.0 (internal AudioSource volume)
        // Note: This is independent of the Mixer volume, which acts as the "Master"
        source.volume = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0, 1, currentTime / duration);
            yield return null;
        }

        source.volume = 1;
    }

    public void PlaySFX(AudioClip clip) => sfxSource.PlayOneShot(clip);

    public void StopAmbience() => ambienceSource.Stop();

    // --- SNAPSHOT TRANSITIONS ---
    public void TransitionToMenu(float duration = 1.0f) => menuSnapshot.TransitionTo(duration);
    public void TransitionToGameplay(float duration = 1.0f) => gameplaySnapshot.TransitionTo(duration);

    // --- VOLUME SETTERS WITH UX FEEDBACK ---

    public void SetMusicVolume(float value)
    {
        SetMixerVolume("MusicVol", value);
        PlayerPrefs.SetFloat("MusicVol", value);
        // No sample needed for Music because it's already playing in the background
    }

    public void SetSFXVolume(float value)
    {
        SetMixerVolume("SFXVol", value);
        PlayerPrefs.SetFloat("SFXVol", value);
        PlaySampleSound(buttonPopClip, sfxSource);
    }

    public void SetTTSVolume(float value)
    {
        SetMixerVolume("TTSVol", value);
        PlayerPrefs.SetFloat("TTSVol", value);
        PlaySampleSound(ttsSampleClip, ttsSampleSource);
    }

    public void SetAmbienceVolume(float value)
    {
        SetMixerVolume("AmbienceVol", value);
        PlayerPrefs.SetFloat("AmbienceVol", value);
        // Optional: Play a wind loop sample here if desired
        PlaySampleSound(ambienceSampleClip, ambienceSource);
    }

    private void SetMixerVolume(string parameter, float value)
    {
        // Logarithmic conversion for natural hearing
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(parameter, db);
        PlayerPrefs.Save();
    }

    private void PlaySampleSound(AudioClip clip, AudioSource source)
    {
        // Only play the sample if enough time has passed since the last one
        if (Time.unscaledTime - lastSampleTime > sampleDelay && clip != null)
        {
            source.PlayOneShot(clip);
            lastSampleTime = Time.unscaledTime;
        }
    }
}