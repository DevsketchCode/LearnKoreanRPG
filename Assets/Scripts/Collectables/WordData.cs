using Assets.Scripts.Collectables;
using System; // Required for Serializable if you want to save WordData

[Serializable] // Optional: If you want to save WordData later using serialization
public class WordData
{
    public enum WordDataType { Phrase, Sentence, Question, Word, Character, Particle, Honorific }
    public enum PartOfSpeech
    {
        //NA,  // Not Applicable
        Noun, Pronoun, Verb, Adjective, Adverb,
        Particle,    // Korean: 조사 (eun/neun, i/ga)
        Honorific,   // Korean: 존댓말 specific words (e.g., kyesida vs itda)
        Affix,       // Tagalog/Ilocano: (um-, mag-, -in-)
        Determiner,  // Tagalog: (Ang, Ng, Sa)
        Conjunction, Preposition, Interjection, NA
    }
    public enum Gender { Male, Female, NA }
    public enum Tense { Base, Past, Present, Future, NA }
    public enum Formality { Informal, Informal_Polite, Informal_Casual, Polite, Formal, Formal_Polite, Formal_Casual, NA }

    // Core IDs
    public string key;
    public string language; // Korean, Ilocano, etc.

    // Translation Data
    public string english;
    public string complex;   // e.g., Hangul
    public string romanized; // Optional
    public string phonetic;  // Optional

    // Grammatical Data
    public WordDataType wordDataType;
    public PartOfSpeech partOfSpeech = PartOfSpeech.NA;
    public Gender gender = Gender.NA;
    public Tense tense = Tense.NA;
    public Formality formality = Formality.NA;

    // Organization
    public string category; // "Food"
    public string subCategory; // "Fruits", "Meats", "Vegetables"
    public int unit;
    public int lesson;

    public string voiceId; // To be used by the TTS Engine

    // Learning State
    public WordsLearned.WordKnowledgeLevel KnowledgeLevel;
    public WordsLearned WordsLearnedGameObject { get; private set; }
    public bool IsLearned { get; set; }


    // Constructor (optional, but good practice to initialize)
    public WordData(string key, string language, string english, string complex,
                    string romanized, string phonetic, WordDataType type, PartOfSpeech pos,
                    Gender gender, Tense tense, Formality formality, string category, string subCategory, int unit, int lesson, string voiceId,
                    Assets.Scripts.Collectables.WordsLearned.WordKnowledgeLevel knowledgeLevel = Assets.Scripts.Collectables.WordsLearned.WordKnowledgeLevel.New)
    {
        this.key = key;
        this.language = language;
        this.english = english;
        this.complex = complex;
        this.romanized = romanized;
        this.phonetic = phonetic;
        this.wordDataType = type;
        this.partOfSpeech = pos;
        this.gender = gender;
        this.tense = tense;
        this.formality = formality;
        this.category = category;
        this.subCategory = subCategory;
        this.unit = unit;
        this.lesson = lesson;
        this.voiceId = voiceId;

        this.KnowledgeLevel = knowledgeLevel;
        this.IsLearned = false;
    }

    // Default constructor (required if you use [Serializable] and might instantiate without arguments sometimes)
    public WordData() { }

    // Override ToString to make it simplier to print out everything from the dictionary, or a wordData item.
    public override string ToString()
    {
        return $"[{language}] Key: {key} | Eng: {english} | Complex: {complex} | " +
               $"Type: {wordDataType} | Tense: {tense} | Gender: {gender} | " +
               $"Level: {KnowledgeLevel} | Learned: {IsLearned}";
    }
}