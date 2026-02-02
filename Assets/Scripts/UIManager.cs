using UnityEngine;
using TMPro;
using System.Collections;
using Assets.Scripts.Collectables;
using UnityEngine.UI;

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

        // 1. Find the UI Image component (Destination)
        // It must be under Panel_Object/Sprite
        Transform spriteTransform = translationPanel.parent.Find("Panel_Object/Image_Sprite");
        Image uiImage = spriteTransform?.GetComponent<Image>();

        if (uiImage != null && ActiveTranslationManager.Instance != null)
        {
            GameObject activeGO = ActiveTranslationManager.Instance.ActiveGameObject.transform.root.gameObject;
            if (activeGO != null)
            {
                // 2. Get the Sprite from the World Object's SpriteRenderer (Source)
                SpriteRenderer worldSR = activeGO.GetComponentInChildren<SpriteRenderer>();

                if (worldSR != null)
                {
                    // 3. The Handshake: Set the UI image to match the world sprite
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

    private void HandleXPJuice(int diff)
    {
        // 1. Determine color and prefix based on gain/loss
        Color juiceColor = diff > 0 ? Color.green : Color.red;
        string prefix = diff > 0 ? "+" : "";

        // 2. Trigger the Floating Text via GameManager's helper
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

        // 3. Pulse the Translation Panel if it's open
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