using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Collectables;
using UnityEngine.SceneManagement;

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance;
    public AudioSource audioSource;
    public AudioClip cachedSettingsClip; // The English "Hello" sample
    public GameObject loadingSpinner;

    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Only pre-load the sample if we are actually in the Settings scene
        if (SceneManager.GetActiveScene().name == "_Settings")
        {
            StartCoroutine(PreloadSettingsSample());
        }
    }

    public void Speak(WordData data)
    {
        if (data == null) return;

        string textToSpeak = !string.IsNullOrEmpty(data.complex) ? data.complex : data.english;
        string langCode = GetLangCode(data.language);
        string cacheKey = $"{textToSpeak}_{langCode}";

        if (audioCache.ContainsKey(cacheKey))
        {
            audioSource.clip = audioCache[cacheKey];
            audioSource.Play();
        }
        else
        {
            // Pass the original language so we can handle specific fallbacks like Ilocano -> Tagalog
            StartCoroutine(DownloadAndPlay(textToSpeak, langCode, cacheKey, data.language));
        }
    }

    private IEnumerator PreloadSettingsSample()
    {
        // Use your existing logic structure for consistency
        string url = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q=Hello&tl=en";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                cachedSettingsClip = DownloadHandlerAudioClip.GetContent(www);
                cachedSettingsClip.name = "TTS_Sample_Hello";
                Debug.Log("[TTS] Settings sample cached successfully.");
            }
            else
            {
                Debug.LogWarning("[TTS] Failed to cache settings sample: " + www.error);
            }
        }
    }

    IEnumerator DownloadAndPlay(string text, string lang, string cacheKey, WordData.Language originalLang)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        string encodedText = UnityWebRequest.EscapeURL(text);
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q={encodedText}&tl={lang}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                if (loadingSpinner != null) loadingSpinner.SetActive(false);

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioCache[cacheKey] = clip;
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                // HIDE SPINNER before showing notification
                if (loadingSpinner != null) loadingSpinner.SetActive(false);

                // --- FALLBACK LOGIC ---

                // 1. If Ilocano failed, try Tagalog before giving up
                if (originalLang == WordData.Language.Ilocano && lang == "ilo")
                {
                    Debug.LogWarning($"[TTS] Ilocano failed for '{text}'. Trying Tagalog fallback...");
                    yield return StartCoroutine(DownloadAndPlay(text, "tl", cacheKey, originalLang));
                }
                else
                {
                    // 2. FINAL FAILURE: Show the user a notification via your new system
                    string errorMsg = "Audio unavailable. Check your internet connection or try again later.";

                    if (originalLang == WordData.Language.Ilocano)
                        errorMsg = $"Pronunciation for '{text}' is currently unavailable in Ilocano or Tagalog.";

                    Debug.LogError($"[TTS Final Error] {www.error}");

                    // Call your new UIManager Notifier
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification(errorMsg);
                    }
                }
            }
        }
    }

    private string GetLangCode(WordData.Language lang)
    {
        switch (lang)
        {
            case WordData.Language.Korean: return "ko";
            case WordData.Language.Tagalog: return "tl";
            case WordData.Language.Ilocano: return "ilo";
            default: return "en";
        }
    }
}