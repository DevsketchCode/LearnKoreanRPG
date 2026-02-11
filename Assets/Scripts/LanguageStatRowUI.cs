using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageStatRowUI : MonoBehaviour
{
    public TMP_Text languageNameText;
    public TMP_Text statsSummaryText;
    public Button viewWordsButton;

    private WordData.Language myLang;
    private SettingsMenuManager settingsManager;

    public void Setup(WordData.Language lang, string statString, int wordCount, SettingsMenuManager mngr)
    {
        myLang = lang;
        settingsManager = mngr;

        languageNameText.text = lang.ToString();
        statsSummaryText.text = statString; // Already formatted by the Manager

        // Disable the button if no words are learned
        if (viewWordsButton != null)
        {
            viewWordsButton.interactable = wordCount > 0;
        }
    }

    // Hook these up to your prefab buttons in the inspector!
    public void OnViewWordsClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FillWordList(myLang);

            // We still want the Settings UI to hide its local stats panel 
            // so the WordList panel is clear.
            settingsManager.statsListPanel.SetActive(false);
        }
    }

    public void OnResetClicked() => settingsManager.RequestReset((int)myLang);
}