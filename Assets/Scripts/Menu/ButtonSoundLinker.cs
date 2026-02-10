using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundLinker : MonoBehaviour
{
    public AudioClip popSound;

    void Awake()
    {
        // Find all buttons that are children of this object (or this object itself)
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            // Add the listener via code so you don't have to do it in the Inspector
            btn.onClick.AddListener(() => PlayPop());
        }
    }

    void PlayPop()
    {
        if (AudioManager.Instance != null && popSound != null)
        {
            AudioManager.Instance.PlaySFX(popSound);
        }
    }
}