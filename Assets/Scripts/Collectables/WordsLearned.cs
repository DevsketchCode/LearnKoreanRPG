using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        public WordData currentWordData; // Stores the full metadata for this object
        public string currentWordID;

        [Header("State")]
        [SerializeField] private WordKnowledgeLevel wordKnowledgeLevel;
        private WordData.WordDataType wordDataType; // Now synced from DB

        [Header("Experience Rewards")]
        [SerializeField] private int familiarExperience = 10;
        [SerializeField] private int knownExperience = 25;
        [SerializeField] private int masteredExperience = 50;

        [Header("UI References")]
        private TextMeshProUGUI englishWordTextPro; // Reference for English TextPro
        private TextMeshProUGUI altLangWordTextPro;  // Reference for Alternate Language TextPro
        private TextMeshProUGUI altLangRomanizedWordTextPro;  // Reference for Alternate Language TextPro
        public string learnedWord_eng;
        public string learnedWord_alt;
        public string learnedWord_alt_romanized;

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

        private Button audioAltLangButton;

        // Public Property for Knowledge Level with Setter Logic
        public WordKnowledgeLevel WordKnowledgeLevelProp // Renamed to PascalCase for property convention
        {
            get { return wordKnowledgeLevel; }
            set
            {
                wordKnowledgeLevel = value; // Set the backing field

                // Ensure the Glow_Highlights are found even if the UI was just enabled
                SetupUIReferences();

                // Debug.Log($"[WordsLearned - WordKnowledgeLevelProp SET] Word: '{learnedWord_eng}', Knowledge Level: {wordKnowledgeLevel}");

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

        public void InitializeFromDatabase()
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
                //CollectableID = parentDisplay.wordID;
                CollectableID = parentDisplay.wordID;

                // Debug.Log($"[WordsLearned] Found key '{CollectableID}' from Parent ({transform.parent.name})");
            }
            else
            {
                Debug.LogWarning($"[WordsLearned] No WordDisplay found on parent of {gameObject.name}. Falling back to local CollectableID.");
            }

            // Determine user language - Defaulting to Korean if not set
            string savedLang = PlayerPrefs.GetString("SelectedLanguage");
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
                learnedWord_alt_romanized = currentWordData.romanized; // Romanized version if available
                this.wordDataType = currentWordData.wordDataType;


                // Debug.Log($"[WordsLearned] Successfully loaded {targetLang} data for {CollectableID}");

                PushDataToButtons();
            }
            else
            {
                Debug.LogWarning($"[WordsLearned] No entry found for ID: {CollectableID} in {targetLang}");
            }

            Debug.Log($"<color=yellow>[ID Check]</color> GO:<b>{gameObject.name}</b> | Key:<b>{CollectableID}</b> | Result:<b>{learnedWord_eng}</b>", gameObject);
        }

        // Handshakes with child WordButtons to prevent "Sibling Not Found" errors
        private void PushDataToButtons()
        {
            // Find all WordButtons that are children of the panel
            WordButton[] buttons = GetComponentsInChildren<WordButton>(true);

            foreach (WordButton btn in buttons)
            {
                // Assign the English key and the reference to this script
                btn.InitializeButton(learnedWord_eng);

                // Ensure the button knows which object to call Finalize on
                btn.WordsLearnedGO = this.gameObject;
            }
            // Debug.Log($"[WordsLearned] Pushed '{learnedWord_eng}' to {buttons.Length} buttons.");
        }

        private void SetupUIReferences()
        {
            // Debug.Log("Ultimate ParentObjectName: " + transform.parent.parent.name);

            // Find the persistent UIManager in the scene
            UIManager uiManager = Object.FindAnyObjectByType<UIManager>();

            if (uiManager != null)
            {
                // Access the PopupCanvas through the UIManager reference
                // Then find Panel_Background within that Canvas
                panelBackground = uiManager.popupTranslationCanvas?.transform.Find("Panel_Background");
            }
            else
            {
                Debug.LogError($"[WordsLearned] UIManager not found in scene for {gameObject.name}!");
                return;
            }

            if (panelBackground != null && uiManager != null)
            {
                // --- English Text Check ---
                // Using the direct reference from UIManager instead of hardcoded path
                Transform translationPanel = uiManager.translationPanel;

                if (translationPanel != null)
                {
                    englishTextObject = translationPanel.Find("Panel_English/Text_English");
                    if (englishTextObject != null)
                    {
                        englishWordTextPro = englishTextObject.GetComponent<TextMeshProUGUI>(); // Changed to TextMeshProUGUI for Canvas UI
                        if (englishWordTextPro == null)
                        {
                            Debug.LogError($"[WordsLearned] TextMeshPro component NOT found on {englishTextObject.name}!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[WordsLearned] Text_English not found under Panel_Translation on {gameObject.name}!");
                    }

                    // --- Alt Language Text Check ---
                    Transform altLangTextObject = translationPanel.Find("Panel_AltLang/Text_AltLang");
                    if (altLangTextObject != null)
                    {
                        altLangWordTextPro = altLangTextObject.GetComponent<TextMeshProUGUI>(); // Changed to TextMeshProUGUI for Canvas UI
                        if (altLangWordTextPro == null)
                        {
                            Debug.LogError($"[WordsLearned] TextMeshPro component NOT found on {altLangTextObject.name}!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[WordsLearned] Text_AltLang not found under Panel_Translation on {gameObject.name}!");
                    }

                    // --- Alt Language Romanized Text Check ---
                    Transform altLangRomanizedTextObject = translationPanel.Find("Panel_AltLang/Text_AltLang_Romanized");
                    if (altLangRomanizedTextObject != null)
                    {
                        altLangRomanizedWordTextPro = altLangRomanizedTextObject.GetComponent<TextMeshProUGUI>(); // Changed to TextMeshProUGUI for Canvas UI
                        if (altLangRomanizedWordTextPro == null)
                        {
                            Debug.LogError($"[WordsLearned] TextMeshPro component NOT found on {altLangRomanizedTextObject.name}!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[WordsLearned] Text_AltLang_Romanized not found under Panel_Translation on {gameObject.name}!");
                    }
                }

                // --- Buttons Check ---
                // Using the direct reference from UIManager
                Transform familiarityPanel = uiManager.familiarityPanel;

                if (familiarityPanel != null)
                {
                    buttonFamiliar = familiarityPanel.Find("FamiliarButton/Button_Familiar")?.GetComponent<Button>();
                    if (buttonFamiliar == null) Debug.LogError($"[WordsLearned] Button_Familiar NOT found under Panel_Familiarity on {gameObject.name}!");

                    buttonKnown = familiarityPanel.Find("KnownButton/Button_Known")?.GetComponent<Button>();
                    if (buttonKnown == null) Debug.LogError($"[WordsLearned] Button_Known NOT found under Panel_Familiarity on {gameObject.name}!");

                    buttonMastered = familiarityPanel.Find("MasteredButton/Button_Mastered")?.GetComponent<Button>();
                    if (buttonMastered == null) Debug.LogError($"[WordsLearned] Button_Mastered NOT found under Panel_Familiarity on {gameObject.name}!");
                }
                else
                {
                    Debug.LogError($"[WordsLearned] Panel_Familiarity not found via UIManager reference on {gameObject.name}!");
                }


                // Find the Audio Button
                audioAltLangButton = translationPanel.Find("Button_Audio_AltLang")?.GetComponent<Button>();
                if (audioAltLangButton == null)
                {
                    Debug.LogError($"[WordsLearned] Button_Audio_AltLang NOT found under Panel_Translation!");
                }
            }
            else
            {
                Debug.LogError($"[WordsLearned] Panel_Background not found under UIManager's PopupCanvas for {gameObject.name}!");
            }
        }
        public override void OnCollect()
        {
            // 1. Instant access via Singleton
            // We use UIManager.Instance to ensure we are talking to the correct scene instance without searching
            if (UIManager.Instance == null)
            {
                Debug.LogError("UIManager.Instance is NULL! Ensure a UIManager exists in the scene and has a Singleton Awake() setup.");
                return;
            }

            // 2. Now check the instance-based lock
            // This uses the Singleton to ensure only one study session happens at a time
            if (UIManager.Instance.IsStudySessionActive)
            {
                Debug.LogWarning($"[WordsLearned] Session busy. Ignoring trigger from {gameObject.name} because another session is active.");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.instance is NULL!");
                return;
            }

            InitializeFromDatabase();

            // Safety check: if for some reason the word never loaded, stop the crash
            if (currentWordData == null || string.IsNullOrEmpty(learnedWord_eng))
            {
                Debug.LogWarning($"[WordsLearned] {gameObject.name} data missing or null. Initializing from database...");

                if (currentWordData == null || string.IsNullOrEmpty(learnedWord_eng))
                {
                    Debug.LogError("Re-initialization failed. Aborting collection to prevent crash.");
                    Debug.LogError($"[WordsLearned] Cannot collect. '{gameObject.name}' has no valid English word assigned. Check if CollectableID matches CSV Key.");
                    return;
                }
            }

            // --- Refresh the Knowledge Level from the Dictionary right before packaging ---
            // This ensures we don't send "New" if the trigger didn't update the level yet.
            UpdateKnowledgeLevelFromDictionary();

            // --- AUDIO BUTTON ASSIGNMENT ---
            // We do this here so the button is ready as soon as the UI panel appears
            if (audioAltLangButton != null)
            {
                audioAltLangButton.onClick.RemoveAllListeners();
                audioAltLangButton.onClick.AddListener(OnSpeakerButtonClick);
                // Debug.Log($"[WordsLearned] Audio button linked to: {learnedWord_eng}");
            }

            // Debug exactly what this specific object thinks the UI components are
            Debug.Log($"[UI Link Check] {learnedWord_eng} is targetting UI Object: {englishWordTextPro.gameObject.name} at path {englishWordTextPro.transform.parent.name}", englishWordTextPro.gameObject);

            // --- KEY LOGIC: Using composite key to check persistent 'IsLearned' flag ---
            // Now that we've initialized above, currentWordData.key is guaranteed to be valid
            string uniqueSaveKey = GetUniqueKey(currentWordData, learnedWord_eng);

            // --- NEW ARCHITECTURE: Package and Start Session ---
            // Instead of manually updating labels and finding buttons here, we hand a data package 
            // to the ActiveTranslationManager. It will manage the UI state from now on.

            // Use a local variable 'session' instead of the static 'ActiveTranslationSession.activeSession'
            // to prevent other objects from overwriting this data during the frame.
            ActiveTranslationSession session = new ActiveTranslationSession
            {
                // Pulling directly from the refreshed currentWordData object
                wordID = this.CollectableID, // The ID from the DB (e.g., tree_01)
                english = this.learnedWord_eng,
                altLang = this.learnedWord_alt,
                altLang_Romanized = this.learnedWord_alt_romanized,
                collectableID = this.CollectableID, // The internal ID (people_general_woman...)
                currentLevel = this.WordKnowledgeLevelProp,
                sourceScript = this // Reference back to this script so the Manager can call Finalize later
            };

            if (ActiveTranslationManager.Instance != null)
            {
                // Debug session info to verify data before sending
                // Debug.Log($"[WordsLearned - OnCollect] Packaging: ID={session.wordID}, ENG={session.english}, ALT={session.altLang}, Level={session.currentLevel}");

                // Pass the localized session, the specific gameObject, and the specific currentWordData
                ActiveTranslationManager.Instance.StartSession(session, this.gameObject, this.currentWordData);
                // Debug.Log($"[WordsLearned - OnCollect] Session sent to ActiveTranslationManager for: {learnedWord_eng}");
            }
            else
            {
                Debug.LogError("ActiveTranslationManager.Instance is NULL! Cannot start study session.");
            }

            // Check persistent 'IsLearned' flag in GameManager dictionary
            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey) && GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey].IsLearned)
            {
                Debug.Log($"[WordsLearned - OnCollect] Word ALREADY LEARNED (persistent data) for {gameObject.name}, key: {uniqueSaveKey}. Ignoring trigger.");

                base.OnCollect(); // Still call base.OnCollect to handle potential timed actions
                return; // Exit early if already learned
            }

            // Word is now "activated" and waiting for button press via the Manager. 
            // This object will remain active and visible until the Manager calls FinalizeWordCollection.
        }

        // Call this when the player clicks a "Close" button or finishes collecting
        public void ReturnToWorldPrompt()
        {
            // Find the InitiateInteractionCanvas which is a sibling of this script's object
            Transform interactionCanvas = transform.parent.Find("InitiateInteractionCanvas");

            if (interactionCanvas != null)
            {
                //UIManager.Instance.IsStudySessionActive = false; // Release the lock

                interactionCanvas.gameObject.SetActive(true);
                Debug.Log($"[WordsLearned] Re-enabling interaction prompt for {learnedWord_eng}");
            }
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
            string uniqueSaveKey = GetUniqueKey(currentWordData, GetEnglishWord());

            if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey))
            {
                WordData savedData = GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey];
                //wordKnowledgeLevel = wordData.KnowledgeLevel; // DO NOT set backing field directly!
                WordKnowledgeLevelProp = savedData.KnowledgeLevel; // Use property setter to trigger UI update
                // Debug.Log($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Loaded knowledge level '{WordKnowledgeLevelProp}' from dictionary for key: '{uniqueSaveKey}'.");

                // Crucially, we don't need to set a persistent 'wordLearned' flag here in WordsLearned.cs anymore!
                // The 'IsLearned' flag in WordData in the dictionary is now the persistent source of truth.
            }
            else
            {
                Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelFromDictionary] Key '{uniqueSaveKey}' NOT found in dictionary on Start. Starting as 'New'.");
                WordKnowledgeLevelProp = WordKnowledgeLevel.New; // Default to New if not in dictionary
            }
        }

        // Called this when a button is clicked
        public void FinalizeWordCollection(WordKnowledgeLevel selectedLevel)
        {
            WordData sessionData;

            // Debug.Log($"[WordsLearned - FinalizeWordCollection] START - Word: '{learnedWord_eng}', knowledgeLevelAlreadySelected: {knowledgeLevelAlreadySelected}"); // **DEBUG LOG - START**

            // Use a unique composite key so Korean "Tree" and Spanish "Tree" are tracked separately
            string uniqueSaveKey = GetUniqueKey(currentWordData, GetEnglishWord());

            // DEBUG: Check what's in the dictionary vs what we are looking for
            // Debug.Log($"[DICTIONARY CHECK] Looking for: {uniqueSaveKey}. Dictionary Count: {GameManager.Instance.wordsLearnedDictionary.Count}");

            // Get the level CURRENTLY in the save file, not the script
            WordKnowledgeLevel previousSavedLevel = WordKnowledgeLevel.New;
            if (GameManager.Instance.wordsLearnedDictionary.TryGetValue(uniqueSaveKey, out var existingData))
            {
                previousSavedLevel = existingData.KnowledgeLevel;
                isNewWord = false;
            }
            else
            {
                isNewWord = true;
            }

            // Exit early ONLY if we aren't changing anything
            if (!isNewWord && previousSavedLevel == selectedLevel)
            {
                Debug.Log("No change detected. Skipping.");
                return;
            }

            // REMOVE FROM DICTIONARY IF RESET TO NEW
            if (selectedLevel == WordKnowledgeLevel.New)
            {
                if (GameManager.Instance.wordsLearnedDictionary.ContainsKey(uniqueSaveKey))
                {
                    GameManager.Instance.wordsLearnedDictionary.Remove(uniqueSaveKey);
                    Debug.Log($"[WordsLearned] Removed {uniqueSaveKey} from dictionary (Reset to New).");
                }

                // We set sessionData to null or dummy here because we are removing the entry
                sessionData = null;
            }
            else
            {
                // Create a copy of the Master Data for the user's save session
                sessionData = new WordData(
                    currentWordData.key, currentWordData.language, currentWordData.english,
                    currentWordData.complex, currentWordData.romanized, currentWordData.phonetic,
                    currentWordData.wordDataType, currentWordData.partOfSpeech, currentWordData.gender, currentWordData.tense,
                    currentWordData.formality, currentWordData.plural, currentWordData.category, currentWordData.subCategory, currentWordData.unit, currentWordData.lesson, currentWordData.audioKey, currentWordData.notes, currentWordData.example_english, currentWordData.example_altLang,
                    selectedLevel
                );

                // --- Use indexer instead of .Add to avoid "Key already exists" crash ---
                GameManager.Instance.wordsLearnedDictionary[uniqueSaveKey] = sessionData;

                Debug.Log($"Word added to WordsLearnedDictionary. English: '{learnedWord_eng}', AltLang: '{learnedWord_alt}', Knowledge Level: {selectedLevel}");
            }

            // **Get the PREVIOUS knowledge level** // We use previousSavedLevel here because sessionData already has the NEW level assigned from the constructor above
            WordKnowledgeLevel previousLevel = previousSavedLevel;

            // Exit early if the knowledge levels match.  Same button clicked.
            if (previousLevel == selectedLevel && !isNewWord)
            {
                Debug.LogWarning($"[WordsLearned - FinalizeWordCollection] Word '{learnedWord_eng}' already finalized! Ignoring button click.");
                return; // Exit early
            }

            WordKnowledgeLevelProp = selectedLevel; // Use property setter to set level AND update UI

            // Ensure sessionData isn't null before setting level (it will be null if selectedLevel is New)
            if (sessionData != null)
            {
                sessionData.KnowledgeLevel = selectedLevel; // Get or Add word pair to WordsLearnedDictionary in GameManager
            }

            // Calculate the DIFFERENCE
            int experienceDifference = GetExperienceForLevel(selectedLevel) - (isNewWord ? 0 : GetExperienceForLevel(previousLevel));

            // Apply the EXPERIENCE DIFFERENCE
            GameManager.Instance.Experience += experienceDifference;

            // 1. If we are moving FROM 'New' TO a 'Learned' level (Familiar/Known/Mastered)
            if (previousLevel == WordKnowledgeLevel.New && selectedLevel != WordKnowledgeLevel.New)
            {
                GameManager.Instance.WordsLearned++;
                Debug.Log("[WordsLearned] Incrementing count: Word moved from New to Learned.");
            }
            // 2. If we are moving FROM a 'Learned' level BACK to 'New'
            else if (previousLevel != WordKnowledgeLevel.New && selectedLevel == WordKnowledgeLevel.New)
            {
                // Safety check to prevent going below 0
                if (GameManager.Instance.WordsLearned > 0)
                {
                    GameManager.Instance.WordsLearned--;
                    Debug.Log("[WordsLearned] Decrementing count: Word reset to New.");
                }

                // --- THE "RESURRECTION" FIX: Reactivate visuals and collider ---
                if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = true;
                foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = true;
                Debug.Log($"[WordsLearned] Reactivated Collider and Renderers for {gameObject.name}");
            }

            if (sessionData != null)
            {
                sessionData.IsLearned = (selectedLevel != WordKnowledgeLevel.New); // PERSISTENTLY set IsLearned flag in WordData to TRUE!
            }

            //Debug.Log($"[WordsLearned - FinalizeWordCollection] END: Word '{learnedWord_eng}' collection finalized at level: {selectedLevel}. WordsLearned: {GameManager.Instance.WordsLearned}, Experience: {GameManager.Instance.Experience}");

            // Set this BEFORE calling base.OnCollect() to ensure 
            // the Handshake is finished before the object is disabled
            UIManager.Instance.IsStudySessionActive = false;
            UIManager.Instance.activeWordScript = null;

            ReturnToWorldPrompt();

            // Reset Audio Button Listeners to prevent "Sibling Not Found" errors if the player clicks another word before closing the panel
            if (audioAltLangButton != null)
            {
                audioAltLangButton.onClick.RemoveAllListeners();
            }

            // Call base.OnCollect so that the GameManager saves the scene state
            // and the Collectable state is recorded, regardless of the level chosen.
            base.OnCollect();

            // If it's New, we immediately "Undo" the deactivation/hiding that base.OnCollect just did
            if (selectedLevel == WordKnowledgeLevel.New)
            {
                if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = true;
                foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) r.enabled = true;

                Debug.Log($"[WordsLearned] Word reset to New. Reactivating {gameObject.name} visuals.");
            }

            // RELEASE THE LOCK so the next object can be collected
            if (UIManager.Instance != null)
            {
                UIManager.Instance.IsStudySessionActive = false;
                UIManager.Instance.activeWordScript = null;
            }
        }

        public static string GetUniqueKey(WordData data, string fallback_engWord)
        {
            string uniqueSaveKey = string.Empty;

            if (data != null)
            {
                uniqueSaveKey = data.key + "_" + data.language;
            }
            else
            {
                if (fallback_engWord != string.Empty)
                {
                    uniqueSaveKey = fallback_engWord; // Fallback to WordData is null
                }
                else 
                {
                    Debug.LogError("[WordsLearned - GetUniqueKey] Both WordData and fallback_engWord are NULL or EMPTY! Cannot build unique key.");
                }
                
            }
            return uniqueSaveKey;
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
            // Debug.Log($"[WordsLearned - GetEnglishWord] Returning English word: '{learnedWord_eng}' for object: {gameObject.transform.parent.parent.name}");
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

            // Note: If you don't have a specific 'buttonNew' reference variable, we find it via the sibling of buttonFamiliar
            GameObject buttonNewGO = buttonFamiliar.transform.parent.parent.Find("NewButton/Button_New")?.gameObject;
            Image newButtonImage = buttonNewGO?.GetComponent<Image>();

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

            // --- Sibling Glow Handling ---
            // Find the sibling glow objects under the same parent as the buttons
            GameObject newGlow = buttonNewGO?.transform.parent.Find("Glow_Highlight")?.gameObject;
            GameObject familiarGlow = buttonFamiliar.transform.parent.Find("Glow_Highlight")?.gameObject;
            GameObject knownGlow = buttonKnown.transform.parent.Find("Glow_Highlight")?.gameObject;
            GameObject masteredGlow = buttonMastered.transform.parent.Find("Glow_Highlight")?.gameObject;

            // Reset all buttons to default color first
            if (newButtonImage != null) newButtonImage.color = newWordColor;
            familiarButtonImage.color = newWordColor;
            knownButtonImage.color = newWordColor;
            masteredButtonImage.color = newWordColor;
            initiateInteractionCanvasImage.color = newWordColor;

            // Reset all glows to disabled
            if (newGlow != null) newGlow.SetActive(false);
            if (familiarGlow != null) familiarGlow.SetActive(false);
            if (knownGlow != null) knownGlow.SetActive(false);
            if (masteredGlow != null) masteredGlow.SetActive(false);

            switch (WordKnowledgeLevelProp) // Use the Property here!
            {
                case WordKnowledgeLevel.New:
                    // Highlight the New button and its glow
                    if (newButtonImage != null) newButtonImage.color = newWordColor;
                    if (newGlow != null) newGlow.SetActive(true);
                    break;
                case WordKnowledgeLevel.Familiar:
                    familiarButtonImage.color = familiarWordColor;
                    initiateInteractionCanvasImage.color = familiarWordColor;
                    if (familiarGlow != null) familiarGlow.SetActive(true); // Enable sibling glow
                    break;
                case WordKnowledgeLevel.Known:
                    knownButtonImage.color = knownWordColor;
                    initiateInteractionCanvasImage.color = knownWordColor;
                    if (knownGlow != null) knownGlow.SetActive(true); // Enable sibling glow
                    break;
                case WordKnowledgeLevel.Mastered:
                    masteredButtonImage.color = masteredWordColor;
                    initiateInteractionCanvasImage.color = masteredWordColor;
                    if (masteredGlow != null) masteredGlow.SetActive(true); // Enable sibling glow
                    break;
                default:
                    Debug.LogWarning($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Unknown WordKnowledgeLevel: {WordKnowledgeLevelProp} for word: '{learnedWord_eng}'. No button highlighted.");
                    break;
            }

            // Debug.Log($"[WordsLearned - UpdateKnowledgeLevelButtonColor] Button colors updated for word: '{learnedWord_eng}' to level: {WordKnowledgeLevelProp}");
        }

        // Text to Speech Implementation
        public void OnSpeakerButtonClick()
        {
            // If the instance is null, try one last time to find it in the scene
            if (TTSManager.Instance == null)
            {
                TTSManager.Instance = Object.FindAnyObjectByType<TTSManager>();
            }

            // Safety Check
            if (TTSManager.Instance != null)
            {
                // currentWordData is the data for the word currently being shown
                TTSManager.Instance.Speak(currentWordData);
            }
            else
            {
                Debug.LogError($"[WordsLearned] No TTSManager found in this scene! " +
                               $"Make sure the Management prefab is in the Hierarchy.");
            }
        }
    }
}