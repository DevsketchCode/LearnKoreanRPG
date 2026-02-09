using Assets.Scripts.Collectables;

[System.Serializable]
public class ActiveTranslationSession
{
    public string wordID;
    public string english;
    public string altLang;
    public string altLang_Romanized;
    public string collectableID;
    public WordsLearned.WordKnowledgeLevel currentLevel;
    public WordsLearned sourceScript; // Reference back to the world object to trigger its destruction/XP logic
}