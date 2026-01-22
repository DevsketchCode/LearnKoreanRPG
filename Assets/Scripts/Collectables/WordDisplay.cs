using UnityEngine;
using TMPro;
using Assets.Scripts.Collectables;

public class WordDisplay : MonoBehaviour
{
    [Header("Data Link")]
    [SerializeField] private LanguageDatabase masterDB;
    [SerializeField] public string wordID; // Manually set this to "tree_01" etc. in inspector

    [Header("UI Target")]
    [SerializeField] private TextMeshPro altLangTextMesh;

    void Start()
    {
        RefreshText();
    }

    // Call this on Start or whenever you change the language in a menu
    public void RefreshText()
    {
        // 1. Get the current language from settings (default to Korean)
        string savedLang = PlayerPrefs.GetString("SelectedLanguage", "Korean");
        if (!System.Enum.TryParse(savedLang, out WordData.Language currentLanguage))
        {
            // If the string doesn't match any enum, default to Korean
            currentLanguage = WordData.Language.Korean;
        }

        // 2. Pull the specific data from your already-parsed ScriptableObject
        WordData data = masterDB.GetWord(wordID, currentLanguage);

        if (data != null)
        {
            // 3. Update the TextMeshPro with the "Complex" (Hangul) string
            altLangTextMesh.text = data.complex;
            Debug.Log($"Displaying {currentLanguage} for {wordID}: {data.complex}");
        }
        else
        {
            Debug.LogWarning($"Word ID {wordID} not found for language {currentLanguage}!");
        }
    }
}