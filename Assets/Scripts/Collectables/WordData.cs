using Assets.Scripts.Collectables;
using System; // Required for Serializable if you want to save WordData

[Serializable] // Optional: If you want to save WordData later using serialization
public class WordData
{
    public string altLangWord;
    public WordsLearned.WordKnowledgeLevel KnowledgeLevel;
    public WordsLearned WordsLearnedGameObject { get; private set; }

    // Constructor (optional, but good practice to initialize)
    public WordData(string altWord, WordsLearned.WordKnowledgeLevel level)
    {
        altLangWord = altWord;
        KnowledgeLevel = level;
    }

    // Default constructor (required if you use [Serializable] and might instantiate without arguments sometimes)
    public WordData() { }
}