using Assets.Scripts.Collectables;
using System; // Required for Serializable if you want to save WordData

[Serializable] // Optional: If you want to save WordData later using serialization
public class WordData
{

    public enum WordDataType
    {
        Phrase,
        Sentence,
        Question,
        Word
    }

    public WordDataType wordDataType;
    public string altLangWord;
    public WordsLearned.WordKnowledgeLevel KnowledgeLevel;
    public WordsLearned WordsLearnedGameObject { get; private set; }
    public bool IsLearned { get; set; }


    // Constructor (optional, but good practice to initialize)
    public WordData(WordDataType wordDataType, string altLangWord, WordsLearned.WordKnowledgeLevel knowledgeLevel)
    {
        this.wordDataType = wordDataType;
        this.altLangWord = altLangWord;
        this.KnowledgeLevel = knowledgeLevel;
        this.IsLearned = false;
    }

    // Default constructor (required if you use [Serializable] and might instantiate without arguments sometimes)
    public WordData() { }

    // Override ToString to make it simplier to print out everything from the dictionary, or a wordData item.
    public override string ToString()
    {
        return $"AltLang: {altLangWord}, WordType: {wordDataType}, KnowledgeLevel: {KnowledgeLevel}, isLearned: {IsLearned}";
    }
}