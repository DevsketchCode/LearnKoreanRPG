using Assets.Scripts.Collectables;
using System; // Required for Serializable if you want to save WordData

[Serializable] // Optional: If you want to save WordData later using serialization
public class WordData
{
    public enum Language { Korean, Tagalog, Ilocano, English }
    public enum WordDataType { Word, Phrase, Sentence, Question, Character, Affix, Honorific, Particle } // Affix is a prefix or suffix
    public enum PartOfSpeech
    {
        Abbreviation,   // Shortened form of a word or phrase
        Adjective,      // Describes or modifies a noun
        Adverb,         // Modifies a verb, adjective, or another adverb
        Conjunction,    // Connects words, phrases, or clauses (e.g., and, but)
        Determiner,     // Clarifies a noun (e.g., the, a, Tagalog: Ang, Ng)
        Honorific,      // Expresses respect or social status (e.g., kyesida)
        Marker,         // Indicates the grammatical function of a word/phrase
        Noun,           // Represents a person, place, thing, or idea
        NumberNative,   // Number system indigenous to the language (e.g., Hana, Dul)
        NumberOther,    // Adopted number system (e.g., Il, I or Arabic numerals)
        Particle,       // Small functional words (Korean: eun/neun, i/ga)
        Postposition,   // Follows a noun to show relationship (common in Korean)
        Prefix,         // Element added to the beginning of a word
        Preposition,    // Precedes a noun to show relationship (common in Tagalog)
        Pronoun,        // Replaces a noun (e.g., he, she, they, it)
        Suffix,         // Element added to the end of a word
        Verb,           // Expresses an action, occurrence, or state of being
        NA              // Not Applicable
    }
    public enum Gender { Masculine, Feminine, NA }
    public enum Tense { Base, Past, Present, Future, NA }
    public enum Formality {
        ExtremelyFormalPolite, // Hasipsio-che: Used for news, business, or military (-nida)
        FormalPolite,          // Haeyo-che: Standard polite/respectful daily speech (-yo)
        FormalCasual,          // Polite tone used with subordinates or in semi-formal letters
        InformalPolite,        // Friendly but respectful speech (common with acquaintances)
        InformalCasual,        // Banmal: Used with close friends and younger people
        Formal,                // General category for respectful language (Jondetmal)
        Informal,              // General category for casual language (Banmal)
        Slang, 
        NA 
    }

    public enum Plural { Singular, Plural, NA }

    // Core IDs
    public string key;
    public Language language; // Korean, Tagalog, Ilocano, etc.

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
    public Plural plural = Plural.NA;

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
    public WordData(string key, Language language, string english, string complex,
                    string romanized, string phonetic, WordDataType type, PartOfSpeech pos,
                    Gender gender, Tense tense, Formality formality, Plural plural, string category, string subCategory, int unit, int lesson, string voiceId,
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
        this.plural = plural;
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