using UnityEngine;
using TMPro;
using System.Collections;
using Assets.Scripts.Collectables;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance; // The Singleton

    public TMP_Text Text_WordsLearnedValue;
    public TMP_Text Text_ExperienceValue;
    public bool ActivateDebugWindow;
    public Canvas DebugWindowCanvas;
    public TMP_Text Text_DebugPlayerX;
    public TMP_Text Text_DebugPlayerY;

    public GameManager gameManager;
    public GameObject popupTranslationCanvas;

    public Transform familiarityPanel;
    public Transform translationPanel;

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
            gameManager.OnWordsLearnedChanged -= UpdateWordsLearnedText;
            gameManager.OnExperienceChanged -= UpdateExperienceText;
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
        gameManager.OnExperienceChanged += UpdateExperienceText;

        // Initialize UI *after* subscribing and GameManager is found
        UpdateWordsLearnedText(gameManager.WordsLearned);
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

    // --- NEW: Centralized UI Update Method ---
    // This is called by ActiveTranslationManager to populate the labels
    public void UpdateUI(string english, string altLang)
    {
        if (translationPanel == null)
        {
            Debug.LogError("[UIManager] translationPanel is missing! Cannot update UI text.");
            return;
        }

        // Find the TextMeshPro components under the translationPanel
        TMP_Text englishText = translationPanel.Find("Text_English")?.GetComponent<TMP_Text>();
        TMP_Text altLangText = translationPanel.Find("Text_AltLang")?.GetComponent<TMP_Text>();

        if (englishText != null) englishText.text = english;
        if (altLangText != null) altLangText.text = altLang;

        // Automatically show the canvas when a session starts
        if (popupTranslationCanvas != null)
        {
            popupTranslationCanvas.SetActive(true);
        }
    }
}