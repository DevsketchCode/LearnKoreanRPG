using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHelper : MonoBehaviour
{
    public Slider musicSlider, sfxSlider, ttsSlider, ambSlider;

    void Start()
    {
        // Set the slider positions (This triggers onValueChanged!)
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.8f);
        ttsSlider.value = PlayerPrefs.GetFloat("TTSVol", 1.0f);
        ambSlider.value = PlayerPrefs.GetFloat("AmbienceVol", 0.6f);

        musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val, true));
        ttsSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetTTSVolume(val, true));
        ambSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetAmbienceVolume(val, true));
    }
}