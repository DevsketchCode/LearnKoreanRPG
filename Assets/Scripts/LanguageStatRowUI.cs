using TMPro;
using UnityEngine;

public class LanguageStatRowUI : MonoBehaviour
{
    public TMP_Text languageNameText;
    public TMP_Text statsSummaryText;

    private WordData.Language myLang;
    private SettingsMenuManager manager;

    public void Setup(WordData.Language lang, string statString, SettingsMenuManager mngr)
    {
        myLang = lang;
        manager = mngr;

        languageNameText.text = lang.ToString();
        statsSummaryText.text = statString; // Already formatted by the Manager
    }

    // Hook these up to your prefab buttons in the inspector!
    public void OnViewWordsClicked() => manager.OpenWordList((int)myLang);
    public void OnResetClicked() => manager.RequestReset((int)myLang);
}