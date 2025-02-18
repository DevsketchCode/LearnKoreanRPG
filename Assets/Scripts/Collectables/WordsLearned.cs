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

        public WordKnowledgeLevel wordKnowledgeLevel;
        public bool wordLearned = false;

        [SerializeField] private int experience = 0;
        private TextMeshPro englishWordTextPro; // Reference for English TextPro
        private TextMeshPro altLangWordTextPro;  // Reference for Alternate Language TextPro
        private string learnedWord_eng;
        private string learnedWord_alt;



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
            }
            else
            {
                Debug.LogError("Panel_Background not found under PopupCanvas under Translation!");
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        protected void Update()
        {
            
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
                wordKnowledgeLevel = WordKnowledgeLevel.Familiar;

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
            }


            // **Add word pair to WordsLearnedDictionary in GameManager**
            if (!string.IsNullOrEmpty(learnedWord_eng) && !string.IsNullOrEmpty(learnedWord_alt)) // Check if both words are not empty
            {
                if (!GameManager.Instance.wordsLearnedDictionary.ContainsKey(learnedWord_eng)) // Check if English word (key) already exists
                {
                    KeyValuePair<string, string> wordPair = new KeyValuePair<string, string>(learnedWord_eng, learnedWord_alt); // Create the word pair
                    GameManager.Instance.wordsLearnedDictionary.Add(learnedWord_eng, wordPair); // Add to dictionary, English word as key, pair as value

                    Debug.Log($"Word pair added to WordsLearnedDictionary. English: '{learnedWord_eng}', Korean: '{learnedWord_alt}'");
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
        // The Collectable.OnCollect() method already handles everything else (destroying/disabling the object etc.)
    }
}