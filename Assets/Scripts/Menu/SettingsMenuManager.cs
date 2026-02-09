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
   
        string prefix = lang.ToString() + "_";
        string json = PlayerPrefs.GetString(prefix + "KnowledgeData", "");

        // Clear previous rows
        foreach (Transform child in wordListContentContainer) Destroy(child.gameObject);

        if (string.IsNullOrEmpty(json)) return;

        GameManager.KnowledgeWrapper wrapper = JsonUtility.FromJson<GameManager.KnowledgeWrapper>(json);

        // --- SORTING LOGIC ---
        // Use .OrderByDescending to put highest levels (Mastered) at the top
        // Use .OrderBy to put lowest levels (New) at the top
        var sortedWords = wrapper.words
                .OrderBy(w => w.level)
                .ThenBy(w => w.uniqueSaveKey)
                .ToList();

        wordListPanel.SetActive(true);
        statsListPanel.SetActive(false);

        foreach (GameManager.WordSaveData savedWord in sortedWords)
        {
            // We need to split the key to find it in the DB (like we did in LoadState)
            int lastUnderscore = savedWord.uniqueSaveKey.LastIndexOf('_');
            string originalKey = savedWord.uniqueSaveKey.Substring(0, lastUnderscore);

            WordData masterWord = masterDB.GetWord(originalKey, lang);

            if (masterWord != null)
            {
                GameObject row = Instantiate(wordRowPrefab, wordListContentContainer);
                // Row UI script should handle setting text: English, AltLang, Level
                row.GetComponent<WordRowUI>().Setup(
                    masterWord.english, 
                    masterWord.complex, 
                    (WordsLearned.WordKnowledgeLevel)savedWord.level
                );
            }
        }
    }

    public void CloseWordList()
    {
        wordListPanel.SetActive(false);
        statsListPanel.SetActive(true);
    }
}