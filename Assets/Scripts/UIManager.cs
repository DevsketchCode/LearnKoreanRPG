using UnityEngine;
using TMPro;
using System.Collections;
using Assets.Scripts.Collectables;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance; // The Singleton

    [Header("Stats Components")]
    public TMP_Text Text_WordsLearnedValue;
    public TMP_Text Text_ExperienceValue;

    [Header("Debug Window Components")]
    public bool ActivateDebugWindow;
    public Canvas DebugWindowCanvas;
    public TMP_Text Text_DebugPlayerX;
    public TMP_Text Text_DebugPlayerY;

    [Header("References")]
    public GameManager gameManager;

    [Header("Translation Components")]
    public GameObject popupTranslationCanvas;
    public Transform familiarityPanel;
    public Transform translationPanel;

    [Header("Notification Components")]
    public Transform notifierPanel;

    [Header("Word List View")]
    public GameObject wordListPanel;
    public Transform wordListContentContainer;
    public GameObject wordRowPrefab;
    public LanguageDatabase masterDB; // Reference your MasterDB here too

    [Header("Active Word Tracking")]
    public WordsLearned activeWordScript; // The current active word for translation
    public bool IsStudySessionActive = false; // Locks in Study Sessions

    private string debugPrefs;

    private void Awake()
    {
        // Debug.Log("UIManager: BEFORE: ActivateDebugWindow: " + ActivateDebugWindow.ToString());

        if (!PlayerPrefs.HasKey("SaveState"))
        {
            Debug.Log("UIManager: No save data found.");
        }
        else
        {
            debugPrefs = PlayerPrefs.GetString("DebugWindow");
            // Debug.Log("DEBUG WINDOW ACTIVATION: " + debugPrefs);
        }

        if (debugPrefs != "")
        {
            ActivateDebugWindow = (debugPrefs == "True");
        }
        // Debug.Log("UIManager: AFTER: ActivateDebugWindow: " + ActivateDebugWindow.ToString());

        // Assign the Singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        DebugWindowCanvas.enabled = (ActivateDebugWindow);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForGameManager());
    }

    private void OnDisable()
    {
        if (gameManager != null)  // Check if gameManager is assigned before unsubscribing
        {
            // Unsubscribe events
            gameManager.OnWordsLearnedChanged -= UpdateWordsLearnedText;
            gameManager.OnWordsLearnedDiffChanged -= HandleWordJuice;
            gameManager.OnExperienceChanged -= UpdateExperienceText;
            gameManager.OnExperienceDiffChanged -= HandleXPJuice;
        }
    }

    private IEnumerator WaitForGameManager()
    {
        while (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>(); // Find the persistent GameManager
            yield return null; // Wait for the next frame
        }

        // Subscribe to events *after* GameManager is found
        gameManager.OnWordsLearnedChanged += UpdateWordsLearnedText;
        gameManager.OnWordsLearnedDiffChanged += HandleWordJuice;
        gameManager.OnExperienceChanged += UpdateExperienceText;
        gameManager.OnExperienceDiffChanged += HandleXPJuice;

        // Initialize UI *after* subscribing and GameManager is found
        UpdateWordsLearnedText(gameManager.TotalWordsLearned);
        UpdateExperienceText(gameManager.Experience);
    }

    private void UpdateWordsLearnedText(int wordsLearned)
    {
        if (Text_WordsLearnedValue != null)
        {
            Text_WordsLearnedValue.text = wordsLearned.ToString();
        }
    }

    private void UpdateExperienceText(int experience)
    {
        if (Text_ExperienceValue != null)
        {
            Text_ExperienceValue.text = experience.ToString();
        }
    }

    public void UpdateDebugWindow(float playerPosX, float playerPosY)
    {
        if (Text_DebugPlayerX != null)
        {
            Text_DebugPlayerX.text = "Player Position X: " + playerPosX.ToString();
        }

        if (Text_DebugPlayerY != null)
        {
            Text_DebugPlayerY.text = "Player Position Y: " + playerPosY.ToString();
        }
    }

    // Centralized UI Update Method
    // This is called by ActiveTranslationManager to populate the labels
    public void UpdateUI(string english, string altLang, string altLangRomanized)
    {
        if (translationPanel == null)
        {
            Debug.LogError("[UIManager] translationPanel is missing! Cannot update UI text.");
            return;
        }

        // Find the TextMeshPro components under the translationPanel
        TMP_Text englishText = translationPanel.Find("Panel_English/Text_English")?.GetComponent<TMP_Text>();
        TMP_Text altLangText = translationPanel.Find("Panel_AltLang/Text_AltLang")?.GetComponent<TMP_Text>();
        TMP_Text altLangRomanizedText = translationPanel.Find("Panel_AltLang/Text_AltLang_Romanized")?.GetComponent<TMP_Text>();

        if (englishText != null) englishText.text = english;
        if (altLangText != null) altLangText.text = altLang;
        if (altLangRomanizedText != null) altLangRomanizedText.text = "[ " + altLangRomanized + " ]";

        // Find the UI Image component (Destination)
        // It must be under Panel_Object/Sprite
        Transform spriteTransform = translationPanel.parent.Find("Panel_Object/Image_Sprite");
        Image uiImage = spriteTransform?.GetComponent<Image>();

        if (uiImage != null && ActiveTranslationManager.Instance != null)
        {
            GameObject activeGO = ActiveTranslationManager.Instance.ActiveGameObject.transform.root.gameObject;
            if (activeGO != null)
            {
                // Get the Sprite from the World Object's SpriteRenderer (Source)
                SpriteRenderer worldSR = activeGO.GetComponentInChildren<SpriteRenderer>();

                if (worldSR != null)
                {
                    // Set the UI image to match the world sprite
                    uiImage.sprite = worldSR.sprite;

                    // Ensure visibility
                    uiImage.enabled = true;
                    uiImage.color = Color.white;
                }
            }
        }

        // Automatically show the canvas when a session starts
        if (popupTranslationCanvas != null)
        {
            popupTranslationCanvas.SetActive(true);
        }
    }

    // This version is called by a "View Words" button in the HUD
    public void OpenCurrentLanguageWordList()
    {
        // If we are in-game, GameManager knows what language we are playing
        if (gameManager != null)
        {
            // Convert the string (e.g., "Korean") back into the Enum (WordData.Language.Korean)
            if (System.Enum.TryParse(gameManager.currentLanguage, out WordData.Language langEnum))
            {
                FillWordList(langEnum);
            }
            else
            {
                Debug.LogError($"[UIManager] Could not parse '{gameManager.currentLanguage}' into a valid Language Enum.");
            }
        }
    }

    public void CloseWordList()
    {
        wordListPanel.SetActive(false);

        // If the Settings Manager exists in this scene, tell it to show the stats again
        SettingsMenuManager settings = FindObjectOfType<SettingsMenuManager>();
        if (settings != null)
        {
            settings.CloseWordList(); // This just reactivates statsListPanel
        }
    }

    // This is the logic moved from SettingsMenuManager
    public void FillWordList(WordData.Language lang)
    {
        string prefix = lang.ToString() + "_";
        string json = PlayerPrefs.GetString(prefix + "KnowledgeData", "");

        // Clear previous rows
        foreach (Transform child in wordListContentContainer) Destroy(child.gameObject);

        wordListPanel.SetActive(true);

        if (string.IsNullOrEmpty(json)) return;

        GameManager.KnowledgeWrapper wrapper = JsonUtility.FromJson<GameManager.KnowledgeWrapper>(json);

        // --- SORTING LOGIC ---
        // Use .OrderByDescending to put highest levels (Mastered) at the top
        // Use .OrderBy to put lowest levels (New) at the top
        var sortedWords = wrapper.words
                .OrderBy(w => w.level)
                .ThenBy(w => w.uniqueSaveKey)
                .ToList();

        foreach (GameManager.WordSaveData savedWord in sortedWords)
        {
            // Split the key to find it in the DB (like we did in LoadState)
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

    public void ShowNotification(string message)
    {
        if (notifierPanel == null) return;

        // Find the TextMeshPro component inside the notifierPanel
        TextMeshProUGUI notifierText = notifierPanel.GetComponentInChildren<TextMeshProUGUI>();

        if (notifierText != null)
        {
            notifierText.text = message;
        }

        // Access the parent (Panel_Notifier_Border) and enable it
        // transform.parent gets the border; .gameObject.SetActive(true) shows it
        notifierPanel.parent.gameObject.SetActive(true);
        notifierPanel.parent.gameObject.GetComponent<UIJuice>().PlayPulse();
    }

    private void HandleXPJuice(int diff)
    {
        // Determine color and prefix based on gain/loss
        Color juiceColor = diff > 0 ? Color.green : Color.red;
        string prefix = diff > 0 ? "+" : "";

        // Trigger the Floating Text via GameManager's helper
        if (gameManager.playerGO != null)
        {
            gameManager.ShowText(
                $"{prefix}{diff} XP",
                juiceColor,
                gameManager.playerGO.transform.position,
                new Vector3(15, 50, 0), // Move up and slightly to the right
                1.5f
            );
        }

        // Pulse the Translation Panel if it's open
        if (popupTranslationCanvas != null && popupTranslationCanvas.activeSelf)
        {
            UIJuice juice = popupTranslationCanvas.GetComponentInChildren<UIJuice>();
            if (juice != null)
            {
                if (diff > 0) juice.PlayPulse(); // Celebrate gain
                else juice.PlayShake();         // Signal loss (see below)
            }
        }
    }

    private void HandleWordJuice(int diff)
    {
        if (diff == 0) return;

        // Use a distinct color for words (Cyan or Yellow) to differentiate from XP
        Color wordColor = diff > 0 ? Color.cyan : Color.red;
        string sign = diff > 0 ? "+" : "";
        string label = Mathf.Abs(diff) == 1 ? "Word" : "Words";

        if (gameManager.playerGO != null)
        {
            // Offset the position slightly so it doesn't overlap the XP text 
            // if they both fire at the exact same time
            Vector3 spawnPos = gameManager.playerGO.transform.position + new Vector3(0.5f, 0.5f, 0);

            gameManager.ShowText(
                $"{sign}{diff} {label}",
                wordColor,
                spawnPos,
                new Vector3(20, 50, 0), // Move up and slightly to the right
                2.0f
            );
        }
    }
}