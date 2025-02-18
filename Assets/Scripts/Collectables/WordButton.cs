using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Collectables; // Make sure to include your namespace
using TMPro;

public class WordButton : MonoBehaviour
{
    private string englishWordKey; // **Set this when you create the button, to the English word**
    public WordsLearned.WordKnowledgeLevel newKnowledgeLevel; // Set this to Known or Mastered in Inspector for each button type
    public GameObject WordsLearnedGO;

    private void Awake() // Use Awake to ensure Text_English is found early
    {
        // Find the sibling Text_English object
        Transform textEnglishTransform = transform.parent.Find("Text_English"); // Assuming Text_English is a direct sibling (adjust path if needed)

        if (textEnglishTransform != null)
        {
            TextMeshPro textMeshPro = textEnglishTransform.GetComponent<TextMeshPro>();
            if (textMeshPro != null)
            {
                englishWordKey = textMeshPro.text; // Get the text from Text_English and set englishWordKey
                Debug.Log($"[WordButton - Awake] Found Text_English: '{englishWordKey}' for button: {gameObject.name}"); // Debug log to confirm
            }
            else
            {
                Debug.LogError($"[WordButton - Awake] TextMeshPro component NOT found on Text_English sibling for button: {gameObject.name}!");
            }
        }
        else
        {
            Debug.LogError($"[WordButton - Awake] Sibling Text_English NOT found for button: {gameObject.name}! Make sure Text_English is a sibling under the same parent.");
        }

        if (string.IsNullOrEmpty(englishWordKey))
        {
            Debug.LogError($"[WordButton - Awake] englishWordKey is EMPTY after trying to get it from Text_English for button: {gameObject.name}!");
        }
    }

    public void OnButtonClicked() // Call this function when the button is clicked (set in Button's OnClick event in Inspector)
    {
        Debug.Log("Button Clicked");
        UpdateWordKnowledgeLevel(newKnowledgeLevel);
    }

    public void UpdateWordKnowledgeLevel(WordsLearned.WordKnowledgeLevel level)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.instance is NULL!");
            return;
        }

        if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(englishWordKey)) // Make sure the word exists in the dictionary
        {
            WordData wordData = GameManager.Instance.wordsLearnedDictionary[englishWordKey]; // Get the WordData object from the dictionary
            wordData.knowledgeLevel = level; // **Update the knowledgeLevel property of the WordData object!**
            if (WordsLearnedGO != null)
            {
                WordsLearnedGO.GetComponent<WordsLearned>().wordKnowledgeLevel = level;
            }
            
            Debug.Log($"[WordButton] Knowledge level updated for word '{englishWordKey}' to: {level}");

            // OPTIONAL: Update UI to reflect the new knowledge level (e.g., change button color)
            // ... code to update button appearance based on level ...
        }
        else
        {
            Debug.LogError($"[WordButton] Word '{englishWordKey}' not found in wordsLearnedDictionary!");
        }
    }
}