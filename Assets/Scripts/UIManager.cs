using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public TMP_Text Text_WordsLearnedValue;
    public TMP_Text Text_ExperienceValue;
    public bool ActivateDebugWindow;
    public Canvas DebugWindowCanvas;
    public TMP_Text Text_DebugPlayerX;
    public TMP_Text Text_DebugPlayerY;

    public GameManager gameManager;

    private string debugPrefs;

    private void Awake()
    {
        Debug.Log("UIManager: BEFORE: ActivateDebugWindow: " + ActivateDebugWindow.ToString());

        if (!PlayerPrefs.HasKey("SaveState"))
        {
            Debug.Log("UIManager: No save data found.");
        }
        else
        {
            debugPrefs = PlayerPrefs.GetString("DebugWindow");
            Debug.Log("DEBUG WINDOW ACTIVATION: " + debugPrefs);
        }

        if (debugPrefs != "")
        {
            ActivateDebugWindow = (debugPrefs == "True");
        }
        Debug.Log("UIManager: AFTER: ActivateDebugWindow: " + ActivateDebugWindow.ToString());
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
}