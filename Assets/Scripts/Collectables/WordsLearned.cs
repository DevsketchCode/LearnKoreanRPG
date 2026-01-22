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

        [Header("Data References")]
        [SerializeField] private LanguageDatabase masterDB; // Assign MasterLanguageDB here
        private WordData currentWordData; // Stores the full metadata for this object

        [Header("State")]
        [SerializeField] private WordKnowledgeLevel wordKnowledgeLevel;
        private WordData.WordDataType wordDataType; // Now synced from DB

        [Header("Experience Rewards")]
        [SerializeField] private int familiarExperience = 10;
        [SerializeField] private int knownExperience = 25;
        [SerializeField] private int masteredExperience = 50;

        [Header("UI References")]
        private TextMeshPro englishWordTextPro; // Reference for English TextPro
        private TextMeshPro altLangWordTextPro;  // Reference for Alternate Language TextPro
        private string learnedWord_eng;
        private string learnedWord_alt;

        [Header("UI Button Integration")]
        private Button buttonFamiliar; // buttons will be dynamically set
        private Button buttonKnown;
        private Button buttonMastered;

        public Color newWordColor = Color.white;         // Set colors in inspector
        public Color familiarWordColor = Color.yellow;
        public Color knownWordColor = Color.cyan;
        public Color masteredWordColor = Color.green;

        private bool isNewWord = false;
        private Transform panelBackground;
        private Transform englishTextObject;

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
            SetupUIReferences();
        }

        protected override void Start()
        {
            base.Start();
            InitializeFromDatabase();
            UpdateKnowledgeLevelFromDictionary(); // Initialize knowledge level from dictionary on Start
            UpdateKnowledgeLevelButtonColor(); // Initial button color update on Start
        }

        private void InitializeFromDatabase()
        {
            if (masterDB == null)
            {
                Debug.LogError($"[WordsLearned] MasterDB is missing on {gameObject.name}!");
                return;
            }

            // This looks for WordDisplay on the parent object
            WordDisplay parentDisplay = GetComponentInParent<WordDisplay>();

            if (parentDisplay != null)
            {
                // Use the key from the parent instead of the local CollectableID
                CollectableID = parentDisplay.wordID;
                Debug.Log($"[WordsLearned] Found key '{CollectableID}' from Parent ({transform.parent.name})");
            }
            else
            {
                Debug.LogWarning($"[WordsLearned] No WordDisplay found on parent of {gameObject.name}. Falling back to local CollectableID.");
            }

            // Determine user language - Defaulting to Korean if not set
            string savedLang = PlayerPrefs.GetString("SelectedLanguage", "Korean");
            if (!System.Enum.TryParse(savedLang, out WordData.Language targetLang))
            {
                // If the string doesn't match any enum, default to Korean
                targetLang = WordData.Language.Korean;
            }

            // CollectableID inherited from Collectable.cs is used as the lookup Key
            currentWordData = masterDB.GetWord(CollectableID, targetLang);

            if (currentWordData != null)
            {
                learnedWord_eng = currentWordData.english;
                learnedWord_alt = currentWordData.complex; // Hangul/etc
                this.wordDataType = currentWordData.wordDataType;

                // Set the UI Text from the Database
                if (englishWordTextPro != null) englishWordTextPro.text = learnedWord_eng;
                if (altLangWordTextPro != null) altLangWordTextPro.text = learnedWord_alt;

                Debug.Log($"[WordsLearned] Successfully loaded {targetLang} data for {CollectableID}");
            }
            else
            {
                Debug.LogWarning($"[WordsLearned] No entry found for ID: {CollectableID} in {targetLang}");
            }
        }

        private void SetupUIReferences()
        {
            // Debug.Log("Ultimate ParentObjectName: " + transform.parent.parent.name);

            panelBackground = transform.parent.Find("PopupCanvas")?.Find("Panel_Background");

            if (panelBackground != null)
            {
                // --- English Text Check ---
                englishTextObject = panelBackground.Find("Text_English");
                if (englishTextObject != null)
                {
                    englishWordTextPro = englishTextObject.GetComponent<TextMeshPro>();
                    if (englishWordTextPro == null)
                    {
                        Debug.LogError($"[WordsLearned] TextMeshPro component NOT found on {englishTextObject.name}!");
                    }
                }
                else
                {
                    Debug.LogError($"[WordsLearned] Text_English not found under Panel_Background on {gameObject.name}!");
                }

                // --- Alt Language Text Check ---
                Transform altLangTextObject = panelBackground.Find("Text_AltLang");
                if (altLangTextObject != null)
                {
                    altLangWordTextPro = altLangTextObject.GetComponent<TextMeshPro>();
                    if (altLangWordTextPro == null)
                    {
                        Debug.LogError($"[WordsLearned] TextMeshPro component NOT found on {altLangTextObject.name}!");
                    }
                }
                else
                {
                    Debug.LogError($"[WordsLearned] Text_AltLang not found under Panel_Background on {gameObject.name}!");
                }

                // --- Buttons Check ---
                buttonFamiliar = panelBackground.Find("Button_Familiar")?.GetComponent<Button>();
                if (buttonFamiliar == null) Debug.LogError($"[WordsLearned] Button_Familiar NOT found under Panel_Background on {gameObject.name}!");

                buttonKnown = panelBackground.Find("Button_Known")?.GetComponent<Button>();
                if (buttonKnown == null) Debug.LogError($"[WordsLearned] Button_Known NOT found under Panel_Background on {gameObject.name}!");

                buttonMastered = panelBackground.Find("Button_Mastered")?.GetComponent<Button>();
                if (buttonMastered == null) Debug.LogError($"[WordsLearned] Button_Mastered NOT found under Panel_Background on {gameObject.name}!");
            }
            else
            {
                Debug.LogError($"[WordsLearned] Panel_Background not found under PopupCanvas for {gameObject.name}!");
            }
        }

        protected override void OnCollect()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.instance is NULL! Cannot collect word.");
                return;
            }

            // Safety check: if for some reason the word never loaded, stop the crash
            if (string.IsNullOrEmpty(learnedWord_eng))
            {
                Debug.LogWarning($"[WordsLearned] {gameObject.name} has no word data. Trying to re-initialize...");
                InitializeFromDatabase();

                if (string.IsNullOrEmpty(learnedWord_eng))
                {
                    Debug.LogError("Re-initialization failed. Aborting collection to prevent crash.");
                    Debug.LogError($"[WordsLearned] Cannot collect. '{gameObject.name}' has no valid English word assigned. Check if CollectableID matches CSV Key.");
                    return;
                }
            }

            // --- KEY LOGIC: Using composite key to check persistent 'IsLearned' flag ---
            string uniqueSaveKey = currentWordData != null ? currentWordData.key + "_" + currentWordData.language : learnedWord_eng;

            // Check persistent 'IsLearned' flag in GameManager dictionary
            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey) && GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey].IsLearned)
            {
                Debug.Log($"[WordsLearned - OnCollect] Word ALREADY LEARNED (persistent data) for {gameObject.name}, key: {uniqueSaveKey}. Ignoring trigger.");
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
        public void UpdateKnowledgeLevelFromDictionary()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[WordsLearned - UpdateKnowledgeLevelFromDictionary] GameManager.instance is NULL!");
                return;
            }

            // --- KEY LOGIC: Match the Save Key used in Finalize ---
            if (currentWordData == null)
            {
                Debug.LogError("[WordsLearned - UpdateKnowledgeLevelFromDictionary] currentWordData is NULL! Cannot build save key.");
                return;
            }
            string uniqueSaveKey = currentWordData.key + "_" + currentWordData.language;

            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey))
            {
                WordData savedData = GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey];
                //wordKnowledgeLevel = wordData.KnowledgeLevel; // DO NOT set backing field directly!
                WordKnowledgeLevelProp = savedData.KnowledgeLevel; // Use property setter to trigger UI update
                Debug.Log($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Loaded knowledge level '{WordKnowledgeLevelProp}' from dictionary for key: '{uniqueSaveKey}'.");

                // Crucially, we don't need to set a persistent 'wordLearned' flag here in WordsLearned.cs anymore!
                // The 'IsLearned' flag in WordData in the dictionary is now the persistent source of truth.
            }
            else
            {
                Debug.Log($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Key '{uniqueSaveKey}' NOT found in dictionary on Start. Starting as 'New'.");
                WordKnowledgeLevelProp = WordKnowledgeLevel.New; // Default to New if not in dictionary
            }
        }

        // Called this when a button is clicked
        public void FinalizeWordCollection(WordKnowledgeLevel selectedLevel)
        {
            WordData sessionData;

            // Debug.Log($"[WordsLearned - FinalizeWordCollection] START - Word: '{learnedWord_eng}', knowledgeLevelAlreadySelected: {knowledgeLevelAlreadySelected}"); // **DEBUG LOG - START**

            // Use a unique composite key so Korean "Tree" and Spanish "Tree" are tracked separately
            string uniqueSaveKey = currentWordData.key + "_" + currentWordData.language;

            // Get or Add word pair to WordsLearnedDictionary in GameManager
            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey))
            {
                sessionData = GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey]; // Get existing WordData
                isNewWord = false;
            }
            else
            {
                // Create a copy of the Master Data for the user's save session
                sessionData = new WordData(
                    currentWordData.key, currentWordData.language, currentWordData.english,
                    currentWordData.complex, currentWordData.romanized, currentWordData.phonetic,
                    currentWordData.wordDataType, currentWordData.partOfSpeech, currentWordData.gender, currentWordData.tense,
                    currentWordData.formality, currentWordData.plural, currentWordData.category, currentWordData.subCategory, currentWordData.unit, currentWordData.lesson, currentWordData.voiceId,
                    selectedLevel
                );

                // Add to dictionary
                GameManager.Instance.wordsLearnedDictionary.Add(uniqueSaveKey, sessionData); 
                isNewWord = true;
                Debug.Log($"Word added to WordsLearnedDictionary (new entry). English: '{learnedWord_eng}', AltLang: '{learnedWord_alt}', Knowledge Level: {selectedLevel}");
            }

            WordKnowledgeLevel previousLevel = sessionData.KnowledgeLevel; // **Get the PREVIOUS knowledge level**

            // Exit early if the knowledge levels match.  Same button clicked.
            if (previousLevel == selectedLevel && !isNewWord)
            {
                Debug.LogWarning($"[WordsLearned - FinalizeWordCollection] Word '{learnedWord_eng}' already finalized! Ignoring button click.");
                return; // Exit early
            }

            WordKnowledgeLevelProp = selectedLevel; // Use property setter to set level AND update UI
            sessionData.KnowledgeLevel = selectedLevel; // Get or Add word pair to WordsLearnedDictionary in GameManager

            int experienceDifference = GetExperienceForLevel(selectedLevel) - (isNewWord ? 0 : GetExperienceForLevel(previousLevel)); // Calculate the DIFFERENCE
            GameManager.Instance.Experience += experienceDifference; // Apply the EXPERIENCE DIFFERENCE

            if (isNewWord)
            {
                GameManager.Instance.WordsLearned++; // Increment word count
            }

            sessionData.IsLearned = true; // PERSISTENTLY set IsLearned flag in WordData to TRUE!

            //Debug.Log($"[WordsLearned - FinalizeWordCollection] END: Word '{learnedWord_eng}' collection finalized at level: {selectedLevel}. WordsLearned: {GameManager.Instance.WordsLearned}, Experience: {GameManager.Instance.Experience}");

            // Optionally disable/destroy the Collectable object after successful finalization.
            base.OnCollect(); // Call base.OnCollect to handle object disabling/destruction
        }

        private int GetExperienceForLevel(WordKnowledgeLevel level)
        {
            switch (level)
            {
                case WordKnowledgeLevel.Familiar:
                    return familiarExperience;
                case WordKnowledgeLevel.Known:
                    return knownExperience;
                case WordKnowledgeLevel.Mastered:
                    return masteredExperience;
                case WordKnowledgeLevel.New: // or default:
                default:
                    return 0; // "New" level and any unknown level gives 0 experience
            }
        }

        public string GetEnglishWord() 
        {
            Debug.Log($"[WordsLearned - GetEnglishWord] Returning English word: '{learnedWord_eng}' for object: {gameObject.transform.parent.parent.name}");
            return learnedWord_eng;
        }

        public void SetKnowledgeLevel(WordKnowledgeLevel level)
        {
            WordKnowledgeLevelProp = level; // Use the Property Setter!
            Debug.Log($"[WordsLearned - SetKnowledgeLevel] Word '{learnedWord_eng}' knowledge level set to: {WordKnowledgeLevelProp} (triggered by button or load)");
        }

        public void UpdateKnowledgeLevelButtonColor()
        {
            if (buttonFamiliar == null || buttonKnown == null || buttonMastered == null)
            {
                Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelButtonColor] One or more Knowledge Level Buttons are NOT found for word: '{learnedWord_eng}'. UI update skipped.");
                return; // Exit if buttons are missing!
            }

            Image familiarButtonImage = buttonFamiliar.GetComponent<Image>();
            Image knownButtonImage = buttonKnown.GetComponent<Image>();
            Image masteredButtonImage = buttonMastered.GetComponent<Image>();
            Image initiateInteractionCanvasImage = this.transform.parent.Find("InitiateInteractionCanvas").Find("Button_InitiateInteraction").GetComponent<Image>();

            if (familiarButtonImage == null || knownButtonImage == null || masteredButtonImage == null)
            {
                Debug.LogError($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Image component MISSING on one or more Knowledge Level Buttons for word: '{learnedWord_eng}'. UI update skipped.");
                return; // Exit if Image component is missing!
            }
            else if (initiateInteractionCanvasImage == null)
            {
                Debug.LogError($"[WordsLearned - UpdateKnowledgeLevelButtonColor] InitiateInteractionCanvasImage component MISSING for word: '{learnedWord_eng}'. UI update skipped.");
                return; // Exit if InitiateInteractionCanvasImage Image component is missing!
            }

            // Reset all buttons to default color first
            familiarButtonImage.color = newWordColor;
            knownButtonImage.color = newWordColor;
            masteredButtonImage.color = newWordColor;
            initiateInteractionCanvasImage.color = newWordColor;

            switch (WordKnowledgeLevelProp) // Use the Property here!
            {
                case WordKnowledgeLevel.New:
                    // No button is highlighted for "New" level
                    break;
                case WordKnowledgeLevel.Familiar:
                    familiarButtonImage.color = familiarWordColor;
                    initiateInteractionCanvasImage.color = familiarWordColor;
                    break;
                case WordKnowledgeLevel.Known:
                    knownButtonImage.color = knownWordColor;
                    initiateInteractionCanvasImage.color = knownWordColor;
                    break;
                case WordKnowledgeLevel.Mastered:
                    masteredButtonImage.color = masteredWordColor;
                    initiateInteractionCanvasImage.color = masteredWordColor;
                    break;
                default:
                    Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Unknown WordKnowledgeLevel: {WordKnowledgeLevelProp} for word: '{learnedWord_eng}'. No button highlighted.");
                    break;
            }

            Debug.Log($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Button colors updated for word: '{learnedWord_eng}' to level: {WordKnowledgeLevelProp}");
        }

        // Text to Speech Implementation
        public void OnSpeakerButtonClick()
        {
            // currentWordData is the data for the word currently being shown
            TTSManager.Instance.Speak(currentWordData);
        }

    }
}