using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
    public GameObject textContainer;
    public GameObject textPrefab;
    public float floatingFontSize = 55f;

    private List<FloatingText> floatingTexts = new List<FloatingText>();

    private void Update()
    {
        foreach(FloatingText txt in floatingTexts)
        {
            txt.UpdateFloatingText();
        }
    }
    public void Show(string msg, Color color, Vector3 position, Vector3 motion, float duration)
    {
        // Safety check for Camera
        if (Camera.main == null)
        {
            Debug.LogError("No Main Camera found! Make sure your camera is tagged 'MainCamera'.");
            return;
        }

        FloatingText floatingText = GetFloatingText();

        // Safety check for the Text component
        if (floatingText.txt == null)
        {
            Debug.LogError("FloatingText component is missing on the prefab clones!");
            return;
        }

        floatingText.txt.text = msg;
        floatingText.txt.fontSize = floatingFontSize;
        floatingText.txt.color = color;

        // Position conversion
        floatingText.go.transform.position = Camera.main.WorldToScreenPoint(position); // Transfer world space to screen space so we can use it in the UI

        floatingText.motion = motion;
        floatingText.duration = duration;

        floatingText.Show();
    }

    // pooling mechanic
    private FloatingText GetFloatingText()
    {
        FloatingText txt = floatingTexts.Find(t => !t.active);

        if (txt == null)
        {
            // 1. Create the class instance first!
            txt = new FloatingText();

            // 2. Now you can safely assign its members
            txt.go = Instantiate(textPrefab);
            txt.go.transform.SetParent(textContainer.transform);

            // Note: Make sure your prefab uses the legacy 'Text' component 
            // If you're using TextMeshPro, change this to TMP_Text
            txt.txt = txt.go.GetComponent<TMP_Text>();

            floatingTexts.Add(txt);
        }

        return txt;
    }

}
