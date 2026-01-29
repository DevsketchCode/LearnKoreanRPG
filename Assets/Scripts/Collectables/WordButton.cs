using UnityEngine;
using Assets.Scripts.Collectables; // Make sure to include your namespace
using TMPro;

public class WordButton : MonoBehaviour
{
    private string englishWordKey; // **Set this when you create the button, to the English word**
    public WordsLearned.WordKnowledgeLevel newKnowledgeLevel; // Set this to Known or Mastered in Inspector for each button type
    public GameObject WordsLearnedGO;

    private void Awake() // Use Awake to ensure Text_English is found early
    {

        // Just leave this empty or remove it. 
        // We will receive our englishWordKey from the parent script now.

        // Find the sibling Text_English object
        //Transform textEnglishTransform = transform.parent.Find("Text_English"); // Assuming Text_English is a direct sibling (adjust path if needed)

        //if (textEnglishTransform != null)
        //{
        //    TextMeshPro textMeshPro = textEnglishTransform.GetComponent<TextMeshPro>();
        //    if (textMeshPro != null)
        //    {
        //        englishWordKey = textMeshPro.text; // Get the text from Text_English and set englishWordKey
        //        Debug.Log($"[WordButton - Awake] Found Text_English: '{englishWordKey}' for button: {gameObject.name}"); // Debug log to confirm
        //    }
        //    else
        //    {
        //        Debug.LogError($"[WordButton - Awake] TextMeshPro component NOT found on Text_English sibling for button: {gameObject.name}!");
        //    }
        //}
        //else
        //{
        //    Debug.LogError($"[WordButton - Awake] Sibling Text_English NOT found for button: {gameObject.name}! Make sure Text_English is a sibling under the same parent.");
        //}

        //if (string.IsNullOrEmpty(englishWordKey))
        //{
        //    Debug.LogError($"[WordButton - Awake] englishWordKey is EMPTY after trying to get it from Text_English for button: {gameObject.name}!");
        //}
    }

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

        FinalizeWordCollectionForWord();
    }

    private void FinalizeWordCollectionForWord()
    {
        if (WordsLearnedGO == null)
        {
            Debug.LogError($"[WordButton - FinalizeWordCollectionForWord] WordsLearnedGO is NOT assigned in Inspector for button: {gameObject.name}! Cannot finalize collection.");
            return;
        }

        WordsLearned wordsLearnedScript = WordsLearnedGO.GetComponent<WordsLearned>();
        if (wordsLearnedScript == null)
        {
            Debug.LogError($"[WordButton - FinalizeWordCollectionForWord] WordsLearned component NOT found on WordsLearnedGO for button: {gameObject.name}!");
            return;
        }


        wordsLearnedScript.FinalizeWordCollection(newKnowledgeLevel); // **Call the NEW FinalizeWordCollection function in WordsLearned.cs!**

        // OPTIONAL:  You might want to disable the buttons or the popup after a button is clicked,
        // or handle any other UI cleanup here.  For example:
        // transform.parent.gameObject.SetActive(false); // Disable the entire popup panel.
        // gameObject.GetComponent<Button>().interactable = false; // Disable just this button.

        Debug.Log($"[WordButton - FinalizeWordCollectionForWord] FinalizeWordCollection called in WordsLearned.cs for word: '{englishWordKey}', level: {newKnowledgeLevel}.");
    }

    // Function not currently used, but leaving it, as this function will update the WordKnowledgeLevel elsewhere other than the buttons if needed
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
            wordData.KnowledgeLevel = level; // **Update the knowledgeLevel property of the WordData object!**
            if (WordsLearnedGO != null)
            {
                WordsLearnedGO.GetComponent<WordsLearned>().WordKnowledgeLevelProp = level;
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