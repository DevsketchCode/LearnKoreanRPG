using UnityEngine;
using Assets.Scripts.Collectables; // Make sure to include your namespace
using TMPro;

public class WordButton : MonoBehaviour
{
    private string englishWordKey; // **Set this when you create the button, to the English word**
    public WordsLearned.WordKnowledgeLevel newKnowledgeLevel; // Set this to Known or Mastered in Inspector for each button type
    public GameObject WordsLearnedGO;

    public void InitializeButton(string wordKey)
    {
        englishWordKey = wordKey;
        // Debug.Log($"[WordButton] {gameObject.name} initialized with key: {englishWordKey}");
    }

    public void OnButtonClicked() // Call this function when the button is clicked (set in Button's OnClick event in Inspector)
    {
        Debug.Log("Button Clicked");

        // Play the bounce
        UIJuice juice = GetComponent<UIJuice>();
        if (juice != null)
        {
            juice.PlayButtonClick();
        }

        // NEW ARCHITECTURE:
        // Instead of calling a local Finalize method that relies on UIManager.activeWordScript,
        // we delegate the entire completion process to the ActiveTranslationManager.
        //if (ActiveTranslationManager.Instance != null && ActiveTranslationManager.Instance.IsSessionActive)
        //{
        //    // The button simply tells the manager what level was picked
        //    ActiveTranslationManager.Instance.CompleteSession(this.newKnowledgeLevel);
        //}
        //else
        //{
            // Falling back to local finalize if needed for debugging, but CompleteSession handles this now
            FinalizeWordCollectionForWord();
        //}
    }

    private void FinalizeWordCollectionForWord()
    {
        // 1. Cache the reference locally so it can't turn null mid-execution
        // Check both UIManager and ActiveTranslationManager for redundancy
        WordsLearned activeScript = null;

        if (ActiveTranslationManager.Instance != null && ActiveTranslationManager.Instance.CurrentSession != null)
        {
            activeScript = ActiveTranslationManager.Instance.CurrentSession.sourceScript;
        }
        else if (UIManager.Instance != null)
        {
            activeScript = UIManager.Instance.activeWordScript;
        }

        if (activeScript != null)
        {
            // 2. Perform the logic using the local variable
            activeScript.FinalizeWordCollection(newKnowledgeLevel);

            // 3. Use the local variable for logging to avoid the NullReference
            Debug.Log($"[WordButton] Called Finalize for: {activeScript.learnedWord_eng}");
            Debug.Log($"[ActiveWordScript] CollectableID: {activeScript.CollectableID}");
        }
        else
        {
            Debug.LogError("[WordButton] No active word script found in UIManager or ActiveTranslationManager!");
        }
    }

    // Function not currently used, but leaving it, as this function will update the WordKnowledgeLevel elsewhere other than the buttons if needed
    public void UpdateWordKnowledgeLevel(WordsLearned.WordKnowledgeLevel level)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.instance is NULL!");
            return;
        }

        // We use the English word key from the ActiveSession if the local one is empty
        string keyToUse = englishWordKey;
        if (string.IsNullOrEmpty(keyToUse) && ActiveTranslationManager.Instance.IsSessionActive)
        {
            keyToUse = ActiveTranslationManager.Instance.CurrentSession.english;
        }

        if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(keyToUse)) // Make sure the word exists in the dictionary
        {
            WordData wordData = GameManager.Instance.wordsLearnedDictionary[keyToUse]; // Get the WordData object from the dictionary
            wordData.KnowledgeLevel = level; // **Update the knowledgeLevel property of the WordData object!**

            // Note: We still use the cached WordsLearnedGO here if assigned, 
            // but for the study session, FinalizeWordCollectionForWord handles the logic via UIManager
            if (WordsLearnedGO != null)
            {
                WordsLearnedGO.GetComponent<WordsLearned>().WordKnowledgeLevelProp = level;
            }

            Debug.Log($"[WordButton] Knowledge level updated for word '{keyToUse}' to: {level}");

            // OPTIONAL: Update UI to reflect the new knowledge level (e.g., change button color)
            // ... code to update button appearance based on level ...
        }
        else
        {
            Debug.LogError($"[WordButton] Word '{keyToUse}' not found in wordsLearnedDictionary!");
        }
    }
}