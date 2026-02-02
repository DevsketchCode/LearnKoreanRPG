using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText
{
    public bool active;
    public GameObject go;
    public TMP_Text txt;
    public Vector3 motion;
    public float duration;
    public float lastShown;

    public void Show()
    {
        active = true;
        lastShown = Time.time;
        go.SetActive(active);
    }

    public void Hide()
    {
        active = false;
        go.SetActive(active);
    }

    public void UpdateFloatingText()
    {
        if (!active)
            return;

        float timeElapsed = Time.time - lastShown;

        if (timeElapsed > duration)
        {
            Hide();
            return;
        }

        // --- JUICE: Smooth Fade Out ---
        // As timeElapsed approaches duration, alpha goes from 1 to 0
        if (txt != null)
        {
            Color c = txt.color;
            c.a = 1.0f - (timeElapsed / duration);
            txt.color = c;
        }

        // Apply movement
        go.transform.position += motion * Time.deltaTime;
    }
}
