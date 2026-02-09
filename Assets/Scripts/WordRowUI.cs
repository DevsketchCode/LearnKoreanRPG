using UnityEngine;
using TMPro;
using Assets.Scripts.Collectables;
using UnityEngine.UI;

public class WordRowUI : MonoBehaviour
{
    public TMP_Text englishText;
    public TMP_Text foreignText;
    public GameObject knowledgeLevelPanel;

    public void Setup(string english, string foreign, WordsLearned.WordKnowledgeLevel level)
    {
        englishText.text = english;
        foreignText.text = foreign;
        knowledgeLevelPanel.GetComponentInChildren<TMP_Text>().text = level.ToString();

        // Color Scheme
        WordsLearned wl = new WordsLearned(); // Create an instance to access the enum values

        Image knowledgeLevelImage = knowledgeLevelPanel.GetComponent<Image>();

        // Optional: Color the level text to match your game's theme
        if (level == WordsLearned.WordKnowledgeLevel.Mastered) knowledgeLevelImage.color = wl.masteredWordColor;
        else if (level == WordsLearned.WordKnowledgeLevel.Known) knowledgeLevelImage.color = wl.knownWordColor;
        else if (level == WordsLearned.WordKnowledgeLevel.Familiar) knowledgeLevelImage.color = wl.familiarWordColor;
        else if (level == WordsLearned.WordKnowledgeLevel.New) knowledgeLevelImage.color = wl.newWordColor;
    }
}