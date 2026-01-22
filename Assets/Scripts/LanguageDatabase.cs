using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Added for advanced filtering

[CreateAssetMenu(fileName = "LanguageDatabase", menuName = "Language/MasterDatabase")]
public class LanguageDatabase : ScriptableObject
{
    public List<WordData> allEntries = new List<WordData>();

    // Find a specific word by its unique key
    public WordData GetWord(string key, WordData.Language language)
    {
        return allEntries.Find(w => w.key == key && w.language == language);
    }

    // Filter by Category (e.g., Get all "Food")
    public List<WordData> GetWordsByCategory(WordData.Language language, string category)
    {
        return allEntries.Where(w => w.language == language &&
                                     w.category.Equals(category, System.StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // Filter by SubCategory (e.g., Get only "Fruits")
    public List<WordData> GetWordsBySubCategory(WordData.Language language, string subCategory)
    {
        return allEntries.Where(w => w.language == language &&
                                     w.subCategory.Equals(subCategory, System.StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // Get specific Formality levels (Useful for NPC dialogue)
    public List<WordData> GetWordsByFormality(WordData.Language language, WordData.Formality formality)
    {
        return allEntries.Where(w => w.language == language && w.formality == formality).ToList();
    }

    // Get all words in a specific Lesson/Unit
    public List<WordData> GetWordsByLesson(WordData.Language language, int unit, int lesson)
    {
        return allEntries.Where(w => w.language == language && w.unit == unit && w.lesson == lesson).ToList();
    }
}