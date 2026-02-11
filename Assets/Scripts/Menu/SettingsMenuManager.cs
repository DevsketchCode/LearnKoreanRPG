using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts.Collectables;
using UnityEngine.UI;
using System.Linq; // Access your WordSaveData/KnowledgeWrapper

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Dynamic Stat UI")]
    public Transform statsContentContainer; // The Vertical Layout Group
    public GameObject languageStatRowPrefab;
    private WordData.Language languageToReset;

    [Header("Word List View")]
    public GameObject statsListPanel;

    [Header("Word List View")]
    public GameObject wordListPanel;
    public Transform wordListContentContainer;
    public GameObject wordRowPrefab; // A simple UI prefab with 3 TMP_Texts



    [Header("References")]
    public LanguageDatabase masterDB; // Drag your MasterDB here
    public ConfirmationModal confirmationModal; // Drag your prefab instance here

    void Start()
    {
        RefreshAllLanguageStats();
    }

    // --- STATS DISPLAY LOGIC ---
    public void RefreshAllLanguageStats()
    {
        // 1. Clear existing rows
        foreach (Transform child in statsContentContainer) Destroy(child.gameObject);

        // 2. Get all possible languages from the Enum
        System.Array allLanguages = System.Enum.GetValues(typeof(WordData.Language));

        foreach (WordData.Language lang in allLanguages)
        {
            // Skip "None" if you have a default/null entry in your enum
            if (lang.ToString() == "None" || lang.ToString() == "English") continue;

            // Pull data from PlayerPrefs
            string statString = GetStatString(lang);

            string prefix = lang.ToString() + "_";
            int wordsCount = PlayerPrefs.GetInt(prefix + "WordsLearned", 0);

            // 3. Instantiate and Setup the row
            GameObject rowGO = Instantiate(languageStatRowPrefab, statsContentContainer);
            LanguageStatRowUI rowScript = rowGO.GetComponent<LanguageStatRowUI>();
            rowScript.Setup(lang, statString, wordsCount, this);
        }
    }

    private string GetStatString(WordData.Language lang)
    {
        // Pull data from PlayerPrefs
        string prefix = lang.ToString() + "_";
        int xp = PlayerPrefs.GetInt(prefix + "Experience", 0);
        int words = PlayerPrefs.GetInt(prefix + "WordsLearned", 0);
        return $"{lang}: {words} Words | {xp} XP";
    }

    public void RequestReset(int langIndex) // Called by UI Buttons (0=Korean, etc)
    {
        languageToReset = (WordData.Language)langIndex;

        // The Modal handles the Activation, the Text, and the Colors
        confirmationModal.Show(
        $"Wipe all progress for {languageToReset}?",
        () => ConfirmReset(), true
    );
    }

    // --- THE DANGEROUS RESET LOGIC ---
    public void ConfirmReset() // Called by the "YES" button in the popup
    {
        GameManager.Instance.HardResetLanguage(languageToReset);
        confirmationModal.Hide();
        RefreshAllLanguageStats();
    }

    // --- THE DICTIONARY VIEWER ---
    public void OpenWordList(int langIndex)
    {
        WordData.Language lang = (WordData.Language)langIndex;

        // Simply tell the UIManager to fill and show the list for this language
        UIManager.Instance.FillWordList(lang);

        // Hide your local stats panel as before
        statsListPanel.SetActive(false);
    }

    public void CloseWordList()
    {
        wordListPanel.SetActive(false);
        statsListPanel.SetActive(true);
    }
}