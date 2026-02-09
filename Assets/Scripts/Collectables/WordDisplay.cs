using UnityEngine;
using TMPro;
using Assets.Scripts.Collectables;

public class WordDisplay : MonoBehaviour
{
    [Header("Data Link")]
    [SerializeField] private LanguageDatabase masterDB;
    [SerializeField] public string wordID; // Manually set this to "tree_01" etc. in inspector

    [Header("UI Target (World Space)")]
    [SerializeField] private TextMeshProUGUI popupEnglish; // The UI text in the popup (set dynamically)
    [SerializeField] private TextMeshProUGUI popupAltLang; // The UI text in the popup (set dynamically)
    [SerializeField] private TextMeshProUGUI popupAltLang_Romanized; // The UI text in the popup (set dynamically)

    void Start()
    {
        RefreshText();
    }

    public void RefreshText()
    {
        string savedLang = PlayerPrefs.GetString("SelectedLanguage", "Korean");
        if (!System.Enum.TryParse(savedLang, out WordData.Language currentLanguage))
        {
            currentLanguage = WordData.Language.Korean;
        }

        if (masterDB == null)
        {
            Debug.LogError($"[WordDisplay] masterDB is NULL on {gameObject.name}!");
            return;
        }

        WordData data = masterDB.GetWord(wordID, currentLanguage);

        if (data != null)
        {
            // Use the Singleton Instance instead of searching the scene.
            // This ensures we are hitting the correct UI Manager immediately.
            if (UIManager.Instance != null && UIManager.Instance.translationPanel != null)
            {
                // Find Text_English and Text_AltLang under the translationPanel on the Singleton Instance
                popupEnglish = UIManager.Instance.translationPanel.Find("Panel_English/Text_English")?.GetComponent<TextMeshProUGUI>();
                popupAltLang = UIManager.Instance.translationPanel.Find("Panel_AltLang/Text_AltLang")?.GetComponent<TextMeshProUGUI>();
                popupAltLang_Romanized = UIManager.Instance.translationPanel.Find("Panel_AltLang/Text_AltLang_Romanized")?.GetComponent<TextMeshProUGUI>();

                if (popupEnglish != null) popupEnglish.text = data.english;
                if (popupAltLang != null) popupAltLang.text = data.complex;
                if (popupAltLang_Romanized != null) popupAltLang_Romanized.text = "[ " + data.romanized + " ]";

                // Debug.Log($"[WordDisplay] Populated UIManager Popup for {wordID} via Singleton Instance");
            }
            else if (UIManager.Instance == null)
            {
                Debug.LogError($"[WordDisplay] UIManager.Instance is NULL! Cannot update UI for {wordID}");
            }
        }
        else
        {
            Debug.LogWarning($"Word ID {wordID} not found for language {currentLanguage}!");
        }
    }
}