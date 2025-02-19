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
        public bool wordLearned = false;

        [SerializeField] private int experience = 0;
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

        // Public Property for Knowledge Level with Setter Logic
        public WordKnowledgeLevel WordKnowledgeLevelProp // Renamed to PascalCase for property convention
        {
            get { return wordKnowledgeLevel; }
            set
            {
                wordKnowledgeLevel = value; // Set the backing field

                Debug.Log($"[WordsLearned - WordKnowledgeLevelProp SET] Word: '{learnedWord_eng}', Knowledge Level: {wordKnowledgeLevel}");

                UpdateKnowledgeLevelButtonColor(); // **Call UI update function here!**
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

                // **Dynamically Find Buttons**
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
            UpdateKnowledgeLevelButtonColor(); // **Initial button color update on Start**
        }


        protected override void OnCollect()
        {
            Debug.Log("WORDSLEARNED: OnCollect()");

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.instance is NULL! Cannot collect word.");
                return;
            }

            if (wordLearned)
            {
                Debug.Log($"[WordsLearned - OnCollect] Word Learned Already! Skipping word collection for {gameObject.name}.");
                base.OnCollect(); // Still call base.OnCollect for timed-out collections (important for saving state etc.)
                
                return;
            }

            // Implement action on collect
            if(wordKnowledgeLevel == WordKnowledgeLevel.New)
            {
                GameManager.Instance.WordsLearned++;
                GameManager.Instance.Experience += experience;
                // Set default knowledge level to the Prop
                WordKnowledgeLevelProp = WordKnowledgeLevel.Familiar; // **Use the Property Setter!**


                // **Get English Word**
                if (englishWordTextPro != null)
                {
                    learnedWord_eng = englishWordTextPro.text;
                }
                else
                {
                    Debug.LogError("englishWordTextPro is null! Cannot get English word text.");
                    return; // Exit if we can't get English word text
                }

                // **Get Alternate Language Word**
                if (altLangWordTextPro != null)
                {
                    learnedWord_alt = altLangWordTextPro.text;
                }
                else
                {
                    Debug.LogError("altLangWordTextPro is null! Cannot get alternate language word text.");
                    return; // Exit if we can't get AltLang word text
                }

                // **Add word pair to WordsLearnedDictionary in GameManager**
                if (!string.IsNullOrEmpty(learnedWord_eng) && !string.IsNullOrEmpty(learnedWord_alt)) // Check if both words are not empty
                {
                    if (!GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng)) // Check if English word (key) already exists
                    {
                        // Create a new WordData object
                        WordData wordData = new WordData(learnedWord_alt, wordKnowledgeLevel); // Create WordData object, initial level = New

                        GameManager.Instance.wordsLearnedDictionary.Add(learnedWord_eng, wordData); // Add to dictionary, English word as key, and wordData object as value

                        Debug.Log($"Word added to WordsLearnedDictionary. English: '{learnedWord_eng}', AltLang: '{learnedWord_alt}', Knowledge Level: {WordKnowledgeLevel.New}"); // Log knowledge level
                    }
                    else
                    {
                        Debug.Log($"Word '{learnedWord_eng}' already in WordsLearnedDictionary. (Duplicate collection?)");
                    }
                }
                else
                {
                    Debug.LogError($"Could not add word pair. English word: '{learnedWord_eng}', AltLang word: '{learnedWord_alt}'. One or both are empty!");
                }

                Debug.Log($"WordsLearned incremented. New value: {GameManager.Instance.WordsLearned} \nTotal Experience: {GameManager.Instance.Experience}");
            }

        }
        // The Collectable.OnCollect() method already handles everything else (destroying/disabling the object etc.)

        public string GetEnglishWord() 
        {
            Debug.Log($"[WordsLearned - GetEnglishWord] Returning English word: '{learnedWord_eng}' for object: {gameObject.transform.parent.parent.name}");
            return learnedWord_eng;
        }

        public void SetKnowledgeLevel(WordKnowledgeLevel level)
        {
            //wordKnowledgeLevel = level; // DO NOT set the backing field directly!
            WordKnowledgeLevelProp = level; // **Use the Property Setter!**
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