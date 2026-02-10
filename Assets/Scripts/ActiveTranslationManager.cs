using UnityEngine;
using Assets.Scripts.Collectables;

public class ActiveTranslationManager : MonoBehaviour
{
    public static ActiveTranslationManager Instance { get; private set; }

    public ActiveTranslationSession CurrentSession;
    public bool IsSessionActive => CurrentSession != null;

    public GameObject ActiveGameObject = null;

    public WordData ActiveWordData = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // This is called by the Interaction Button or the Word itself
    public void StartSession(ActiveTranslationSession newSession, GameObject activeGO, WordData activeWD)
    {

        // Explicitly overwrite the old data with the new data
        this.CurrentSession = newSession;
        this.ActiveGameObject = activeGO;
        this.ActiveWordData = activeWD;

        if (UIManager.Instance != null)
        {
            // Populate the UI with the NEW data
            UIManager.Instance.UpdateUI(newSession.english, newSession.altLang, newSession.altLang_Romanized);
            UIManager.Instance.IsStudySessionActive = true;
        }

        // Debug.Log($"[ActiveTranslationManager] Session STARTED for: {CurrentSession.english}");
    }

    public void ClearSession()
    {
        // Log what we are clearing so we can track the "Ghost" data
        string oldWord = CurrentSession != null ? CurrentSession.english : "None";

        CurrentSession = null; // Kill the data reference

        if (UIManager.Instance != null)
        {
            ActiveGameObject = null; // Kill game object reference
            UIManager.Instance.IsStudySessionActive = false;
            UIManager.Instance.activeWordScript = null;
        }

        WordsLearned wl = new WordsLearned();
        wl.currentWordData = null; // reset current word upon leaving
        ActiveWordData = null; // reset current word upon leaving

        // Debug.Log($"[ActiveTranslationManager] Session CLEARED for: {oldWord}");
    }
}