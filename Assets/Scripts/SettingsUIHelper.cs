using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHelper : MonoBehaviour
{
    public Slider musicSlider, sfxSlider, ttsSlider, ambSlider;

    void Start()
    {
        // 1. Set the slider positions to match saved data
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.8f);
        ttsSlider.value = PlayerPrefs.GetFloat("TTSVol", 1.0f);
        ambSlider.value = PlayerPrefs.GetFloat("AmbienceVol", 0.6f);

        // 2. Tell the sliders to talk to the CURRENT AudioManager Instance
        musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
        ttsSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetTTSVolume(val));
        ambSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetAmbienceVolume(val));
    }
}