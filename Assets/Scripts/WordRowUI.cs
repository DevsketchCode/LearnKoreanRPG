using UnityEngine;
using TMPro;
using Assets.Scripts.Collectables; // To see the WordKnowledgeLevel enum

public class WordRowUI : MonoBehaviour
{
    public TMP_Text englishText;
    public TMP_Text foreignText;
    public TMP_Text levelText;

    public void Setup(string english, string foreign, WordsLearned.WordKnowledgeLevel level)
    {
        englishText.text = english;
        foreignText.text = foreign;
        levelText.text = level.ToString();

        // Optional: Color the level text to match your game's theme
        if (level == WordsLearned.WordKnowledgeLevel.Mastered) levelText.color = Color.green;
        else if (level == WordsLearned.WordKnowledgeLevel.New) levelText.color = Color.white;
    }
}