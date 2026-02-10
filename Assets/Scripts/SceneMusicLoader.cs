using UnityEngine;

public class SceneMusicLoader : MonoBehaviour
{
    // This creates the dropdown in the Inspector
    public enum AudioState { Menu, Gameplay }

    [Header("Scene Settings")]
    public AudioState sceneType;
    public AudioClip sceneBGM;
    public AudioClip sceneAmbience;

    public float transitionTime = 1.0f;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            // Play the music (looping is handled by AudioManager)
            if (sceneBGM != null)
            {
                AudioManager.Instance.PlayBackgroundMusic(sceneBGM);
            }

            // Play ambience (looping is handled by AudioManager)
            if (sceneType == AudioState.Gameplay && sceneAmbience != null)
            {
                AudioManager.Instance.PlayAmbience(sceneAmbience);
            }
            else
            {
                AudioManager.Instance.StopAmbience();
            }

            // Trigger the Snapshot transition
            if (sceneType == AudioState.Menu)
            {
                AudioManager.Instance.TransitionToMenu(transitionTime);
            }
            else
            {
                AudioManager.Instance.TransitionToGameplay(transitionTime);
            }
        }
    }
}