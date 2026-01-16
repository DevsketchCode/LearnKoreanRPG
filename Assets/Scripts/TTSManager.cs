using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance;
    public AudioSource audioSource;

    private void Awake() => Instance = this;

    public void Speak(WordData data)
    {
        // Use "Complex" for Korean, "English" for English, etc.
        string textToSpeak = data.complex;
        string langCode = (data.language == "Korean") ? "ko" : "en";

        StartCoroutine(DownloadAndPlay(textToSpeak, langCode));
    }

    IEnumerator DownloadAndPlay(string text, string lang)
    {
        // The public Google Translate TTS URL
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen={text.Length}&client=tw-ob&q={text}&tl={lang}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("TTS Error: " + www.error);
            }
        }
    }
}