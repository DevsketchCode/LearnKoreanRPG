using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Assets.Scripts.Collectables
{
    public class WordsLearned : Collectable
    {
        public enum WordKnowledgeLevel
        {
            New,        // For words the player has first encountered
            Familiar,   // Recognize it sometimes
            Known,      // Can recall it, use it in simple sentences
            Mastered    // Fluent knowledge, automatic recall
        }

        [SerializeField] private WordKnowledgeLevel wordKnowledgeLevel; // Backing field is now SerializedField and private

        [SerializeField] private int familiarExperience = 0;
        [SerializeField] private int knownExperience = 0;
        [SerializeField] private int masteredExperience = 0;
        private WordKnowledgeLevel selectedLevelExperience;
        private TextMeshPro englishWordTextPro; // Reference for English TextPro
        private TextMeshPro altLangWordTextPro;  // Reference for Alternate Language TextPro
        private string learnedWord_eng;
        private string learnedWord_alt;

        [Header("UI Button Integration")] // Add a header in inspector for organization
        private Button buttonFamiliar; // buttons will be dynamically set
        private Button buttonKnown;
        private Button buttonMastered;
        public Color newWordColor = Color.white;         // Set colors in inspector
        public Color familiarWordColor = Color.yellow;
        public Color knownWordColor = Color.cyan;
        public Color masteredWordColor = Color.green;

        private bool levelSelectedForCurrentWord = false; // Flag to track if a level has been selected

        // Public Property for Knowledge Level with Setter Logic
        public WordKnowledgeLevel WordKnowledgeLevelProp // Renamed to PascalCase for property convention
        {
            get { return wordKnowledgeLevel; }
            set
            {
                wordKnowledgeLevel = value; // Set the backing field

                Debug.Log($"[WordsLearned - WordKnowledgeLevelProp SET] Word: '{learnedWord_eng}', Knowledge Level: {wordKnowledgeLevel}");

                UpdateKnowledgeLevelButtonColor(); // Call UI update function here!
            }
        }

        protected override void Awake() // Use Awake to get the references early
        {
            base.Awake();

            // Debug.Log("Ultimate ParentObjectName: " + transform.parent.parent.name);
            Transform panelBackground = transform.parent.Find("PopupCanvas").Find("Panel_Background");
            if (panelBackground != null)
            {
                Transform englishTextObject = panelBackground.Find("Text_English");
                if (englishTextObject != null)
                {
                    // Debug.Log($"[WordsLearned - Awake] Text_English FOUND: {englishTextObject.name}"); // Success Log for englishTextObject
                    englishWordTextPro = englishTextObject.GetComponent<TextMeshPro>();
                    if (englishWordTextPro == null)
                    {
                        Debug.LogError("TextMeshProUGUI component NOT found on Text_English!");
                    }
                    else
                    {
                        learnedWord_eng = englishWordTextPro.text;
                        Debug.Log($"[WordsLearned - Awake] Initialized English Word: '{learnedWord_eng}' for object: {gameObject.transform.parent.parent.name}");
                    }
                }
                else
                {
                    Debug.LogError("Text_English not found under Panel_Background!");
                }

                Transform altLangTextObject = panelBackground.Find("Text_AltLang");
                if (altLangTextObject != null)
                {
                    // Debug.Log($"[WordsLearned - Awake] Text_AltLang FOUND: {altLangTextObject.name}"); // Success Log for altLangTextObject
                    altLangWordTextPro = altLangTextObject.GetComponent<TextMeshPro>();
                    if (altLangWordTextPro == null)
                    {
                        Debug.LogError("TextMeshProUGUI component NOT found on Text_AltLang!");
                    }
                }
                else
                {
                    Debug.LogError("Text_AltLang not found under Panel_Background!");
                }

                // Dynamically Find Buttons
                buttonFamiliar = panelBackground.Find("Button_Familiar").GetComponent<Button>();
                if (buttonFamiliar == null) Debug.LogError("Button_Familiar NOT found under Panel_Background!");
                buttonKnown = panelBackground.Find("Button_Known").GetComponent<Button>();
                if (buttonKnown == null) Debug.LogError("Button_Known NOT found under Panel_Background!");
                buttonMastered = panelBackground.Find("Button_Mastered").GetComponent<Button>();
                if (buttonMastered == null) Debug.LogError("Button_Mastered NOT found under Panel_Background!");
            }
            else
            {
                Debug.LogError("Panel_Background not found under PopupCanvas under Translation!");
            }
        }

        protected override void Start()
        {
            base.Start();
            UpdateKnowledgeLevelFromDictionary(); // Initialize knowledge level from dictionary on Start
            UpdateKnowledgeLevelButtonColor(); // Initial button color update on Start
        }


        protected override void OnCollect()
        {
            Debug.Log("WORDSLEARNED: OnCollect()");
            levelSelectedForCurrentWord = false; // Reset the flag when a new word is collected


            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.instance is NULL! Cannot collect word.");
                return;
            }

            // Check persistent 'IsLearned' flag in GameManager dictionary
            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng) && GameManager.Instance.wordsLearnedDictionary[learnedWord_eng].IsLearned)
            {
                Debug.Log($"[WordsLearned - OnCollect] Word ALREADY LEARNED (persistent data) for {gameObject.name}. Ignoring trigger.");
                base.OnCollect(); // Still call base.OnCollect to handle potential timed actions
                return; // Exit early if already learned
            }

            // Get words if they aren't already fetched (to be safe, in case Awake didn't run in time in some edge cases)
            if (string.IsNullOrEmpty(learnedWord_eng) || string.IsNullOrEmpty(learnedWord_alt))
            {
                if (englishWordTextPro != null) learnedWord_eng = englishWordTextPro.text;
                if (altLangWordTextPro != null) learnedWord_alt = altLangWordTextPro.text;

                if (string.IsNullOrEmpty(learnedWord_eng) || string.IsNullOrEmpty(learnedWord_alt))
                {
                    Debug.LogError($"[WordsLearned - OnCollect] Could not retrieve English or AltLang word text for: {gameObject.name}!");
                    return; // Cannot proceed without the words
                }
            }

            // Word is now "activated" and waiting for button press. Do NOT increment WordsLearned/Experience or set knowledge level here!
            // This object will remain active and visible until a WordButton associated with it is clicked.

            // The Collectable.OnCollect() method already handles everything else (destroying/disabling the object etc.)
        }

        // Initialize knowledge level from dictionary on Start/Load
        private void UpdateKnowledgeLevelFromDictionary()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[WordsLearned - UpdateKnowledgeLevelFromDictionary] GameManager.instance is NULL!");
                return;
            }

            if (string.IsNullOrEmpty(learnedWord_eng))
            {
                Debug.LogError("[WordsLearned - UpdateKnowledgeLevelFromDictionary] learnedWord_eng is NULL or empty! Cannot retrieve data from dictionary.");
                return;
            }

            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng))
            {
                WordData wordData = GameManager.Instance.wordsLearnedDictionary[learnedWord_eng];
                //wordKnowledgeLevel = wordData.KnowledgeLevel; // DO NOT set backing field directly!
                WordKnowledgeLevelProp = wordData.KnowledgeLevel; // Use property setter to trigger UI update
                Debug.Log($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Loaded knowledge level '{WordKnowledgeLevelProp}' from dictionary for word: '{learnedWord_eng}'.");

                // Crucially, we don't need to set a persistent 'wordLearned' flag here in WordsLearned.cs anymore!
                // The 'IsLearned' flag in WordData in the dictionary is now the persistent source of truth.

            }
            else
            {
                Debug.Log($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Word '{learnedWord_eng}' NOT found in dictionary on Start. Starting as 'New'.");
                WordKnowledgeLevelProp = WordKnowledgeLevel.New; // Default to New if not in dictionary
            }
        }

        // Called this when a button is clicked
        public void FinalizeWordCollection(WordKnowledgeLevel selectedLevel)
        {
            WordData wordData;

            Debug.Log($"[WordsLearned - FinalizeWordCollection] START - Word: '{learnedWord_eng}', levelSelectedForCurrentWord: {levelSelectedForCurrentWord}"); // **DEBUG LOG - START**

            selectedLevelExperience = selectedLevel;
            // Get or Add word pair to WordsLearnedDictionary in GameManager
            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng))
            {
                wordData = GameManager.Instance.wordsLearnedDictionary[learnedWord_eng]; // Get existing WordData
            }
            else
            {
                wordData = new WordData(learnedWord_alt, selectedLevel); // Create new WordData with selected level
                GameManager.Instance.wordsLearnedDictionary.Add(learnedWord_eng, wordData); // Add to dictionary
                Debug.Log($"Word added to WordsLearnedDictionary (new entry). English: '{learnedWord_eng}', AltLang: '{learnedWord_alt}', Knowledge Level: {selectedLevel}");
            }

            if (wordData.IsLearned) 
            {
                // Already had the Knowledge Level Button Clicked

                // Check to see if the KnowledgeLevel has been updated
                if (wordData.KnowledgeLevel == selectedLevel)
                {
                    Debug.LogWarning($"[WordsLearned - FinalizeWordCollection] Word '{learnedWord_eng}' already finalized! Ignoring button click.");
                }
                else
                {
                    // Process the collection and set knowledge level based on button click
                    WordKnowledgeLevelProp = selectedLevel; // Use property setter to set level AND update UI
                    wordData.KnowledgeLevel = selectedLevel; // Get or Add word pair to WordsLearnedDictionary in GameManager

                    Debug.Log("Do stuff here, the KnowledgeLevel has changed.");
                }
            } 
            else if (!wordData.IsLearned)
            {
                // First Time a Knowledge Level button Clicked

                // Process the collection and set knowledge level based on button click
                WordKnowledgeLevelProp = selectedLevel; // Use property setter to set level AND update UI
                wordData.KnowledgeLevel = selectedLevel; // Get or Add word pair to WordsLearnedDictionary in GameManager

                GameManager.Instance.WordsLearned++; // Increment word count
                switch (selectedLevel)
                {
                    case WordKnowledgeLevel.Familiar:
                        GameManager.Instance.Experience += familiarExperience;
                        break;
                    case WordKnowledgeLevel.Known:
                        GameManager.Instance.Experience += knownExperience;
                        break;
                    case WordKnowledgeLevel.Mastered:
                        GameManager.Instance.Experience += masteredExperience;
                        break;
                }
                //GameManager.Instance.Experience += familiarExperience; // Add experience
                if (selectedLevelExperience == WordKnowledgeLevel.Familiar)
                {
                    Debug.Log("SelectedLevelExperience: " + selectedLevelExperience.ToString() + " selectedLevel: " + selectedLevel.ToString());
                }

                wordData.IsLearned = true; // PERSISTENTLY set IsLearned flag in WordData to TRUE!
                levelSelectedForCurrentWord = true; // Set the flag to prevent further level selections for this popup instance
            }



            //if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng))
            //{
            //    wordData = GameManager.Instance.wordsLearnedDictionary[learnedWord_eng]; // Get existing WordData
            //    wordData.KnowledgeLevel = selectedLevel; // Update Knowledge Level
            //}


            Debug.Log($"[WordsLearned - FinalizeWordCollection] Word '{learnedWord_eng}' collection finalized at level: {selectedLevel}. WordsLearned: {GameManager.Instance.WordsLearned}, Experience: {GameManager.Instance.Experience}");

            Debug.Log($"[WordsLearned - FinalizeWordCollection] END - Word '{learnedWord_eng}' collection finalized at level: {selectedLevel}. levelSelectedForCurrentWord set to TRUE. Further button clicks will be ignored for this word popup."); // **DEBUG LOG - END (Successful Collection)**





            // Optionally disable/destroy the Collectable object after successful finalization.
            base.OnCollect(); // Call base.OnCollect to handle object disabling/destruction
        }

        public string GetEnglishWord() 
        {
            Debug.Log($"[WordsLearned - GetEnglishWord] Returning English word: '{learnedWord_eng}' for object: {gameObject.transform.parent.parent.name}");
            return learnedWord_eng;
        }

        public void SetKnowledgeLevel(WordKnowledgeLevel level)
        {
            //wordKnowledgeLevel = level; // DO NOT set the backing field directly!
            WordKnowledgeLevelProp = level; // Use the Property Setter!
            Debug.Log($"[WordsLearned - SetKnowledgeLevel] Word '{learnedWord_eng}' knowledge level set to: {WordKnowledgeLevelProp} (triggered by button or load)");
        }

        private void UpdateKnowledgeLevelButtonColor()
        {
            if (buttonFamiliar == null || buttonKnown == null || buttonMastered == null)
            {
                Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelButtonColor] One or more Knowledge Level Buttons are NOT found for word: '{learnedWord_eng}'. UI update skipped.");
                return; // Exit if buttons are missing!
            }

            Image familiarButtonImage = buttonFamiliar.GetComponent<Image>();
            Image knownButtonImage = buttonKnown.GetComponent<Image>();
            Image masteredButtonImage = buttonMastered.GetComponent<Image>();

            if (familiarButtonImage == null || knownButtonImage == null || masteredButtonImage == null)
            {
                Debug.LogError($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Image component MISSING on one or more Knowledge Level Buttons for word: '{learnedWord_eng}'. UI update skipped.");
                return; // Exit if Image component is missing!
            }

            // Reset all buttons to default color first
            familiarButtonImage.color = newWordColor;
            knownButtonImage.color = newWordColor;
            masteredButtonImage.color = newWordColor;


            switch (WordKnowledgeLevelProp) // Use the Property here!
            {
                case WordKnowledgeLevel.New:
                    // No button is highlighted for "New" level
                    break;
                case WordKnowledgeLevel.Familiar:
                    familiarButtonImage.color = familiarWordColor;
                    break;
                case WordKnowledgeLevel.Known:
                    knownButtonImage.color = knownWordColor;
                    break;
                case WordKnowledgeLevel.Mastered:
                    masteredButtonImage.color = masteredWordColor;
                    break;
                default:
                    Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Unknown WordKnowledgeLevel: {WordKnowledgeLevelProp} for word: '{learnedWord_eng}'. No button highlighted.");
                    break;
            }

            Debug.Log($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Button colors updated for word: '{learnedWord_eng}' to level: {WordKnowledgeLevelProp}");
        }

    }
}