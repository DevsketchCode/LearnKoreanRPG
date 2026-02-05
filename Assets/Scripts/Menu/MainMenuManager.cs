using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
    public LanguageDatabase languageDB;
    public TMP_Dropdown languageDropdown;
    public TMP_Text enterLaneButtonText;
    public TextMeshProUGUI versionText;

    // Maps Language Name (e.g., "Korean") to Prefix (e.g., "KR")
    private Dictionary<string, string> languagePrefixMap = new Dictionary<string, string>();

    void Start()
    {
        PopulateLanguageDropdown();
        LoadLastUsedLanguage();
        UpdateStartButtonText();

        if (versionText != null)
        {
            // Dynamically set the text to the version in Project Settings
            versionText.text = "v" + Application.version;
        }
        else
        {
            Debug.LogWarning("Version Text reference is missing in MainMenuManager.");
        }
    }

    private void LoadLastUsedLanguage()
    {
        // We store the Name (e.g., "Korean") so it's easy to match with the dropdown options
        if (PlayerPrefs.HasKey("LastUsedLanguageName"))
        {
            string savedLangName = PlayerPrefs.GetString("LastUsedLanguageName");

            // Find the index of this name in the dropdown options
            int index = languageDropdown.options.FindIndex(option => option.text == savedLangName);

            if (index != -1)
            {
                languageDropdown.SetValueWithoutNotify(index);
                // Manually update the GameManager's prefix to match the loaded selection
                GameManager.Instance.currentLanguage = languagePrefixMap[savedLangName];
            }
        }
    }

    private void PopulateLanguageDropdown()
    {
        if (languageDB == null || languageDropdown == null) return;

        languageDropdown.ClearOptions();
        languagePrefixMap.Clear();

        // 1. Get unique languages from the DB entries
        // We use the Language enum/column and parse the AudioKey for the prefix
        var uniqueLanguages = languageDB.allEntries
            .GroupBy(w => w.language)
            .Select(group => group.First())
            .ToList();

        List<string> dropdownOptions = new List<string>();

        foreach (WordData entry in uniqueLanguages)
        {
            string langName = entry.language.ToString();

            // Per your requirement: first 2 chars of AudioKey = lowercase prefix (e.g. "kr")
            // We convert it to Uppercase for our SaveState logic (e.g. "KR")
            string prefix = entry.audioKey.Substring(0, 2).ToUpper();

            if (!languagePrefixMap.ContainsKey(langName))
            {
                languagePrefixMap.Add(langName, prefix);
                dropdownOptions.Add(langName);
            }
        }

        languageDropdown.AddOptions(dropdownOptions);

        // Listen for when the player changes the selection to update the button text
        languageDropdown.onValueChanged.AddListener(delegate { OnLanguageChanged(); });
    }

    private void OnLanguageChanged()
    {
        string selectedLang = languageDropdown.options[languageDropdown.value].text;
        PlayerPrefs.SetString("LastUsedLanguageName", selectedLang);
        PlayerPrefs.Save();

        UpdateStartButtonText();
    }

    private void UpdateStartButtonText()
    {
        string selectedLang = languageDropdown.options[languageDropdown.value].text;
        string prefix = languagePrefixMap[selectedLang] + "_";

        // Check if a save exists for THIS specific language
        if (PlayerPrefs.HasKey(prefix + "SaveState"))
        {
            enterLaneButtonText.text = "Continue Your Lane";
        }
        else
        {
            enterLaneButtonText.text = "Enter Your Lane";
        }
    }

    public void OnEnterLaneClicked()
    {
        string selectedLang = languageDropdown.options[languageDropdown.value].text;
        string prefix = languagePrefixMap[selectedLang];

        // 1. Tell the GameManager which language we are using
        GameManager.Instance.currentLanguage = selectedLang;

        // Set the language in Player Prefences
        PlayerPrefs.SetString("SelectedLanguage", selectedLang);

        // 2. Check for the last scene saved for this language
        string lastScene = PlayerPrefs.GetString(selectedLang + "_LastScene");
        
        Debug.Log("Last Scene for this language: " + selectedLang + " was " + lastScene);

        if (lastScene == string.Empty)
        {
            lastScene = "Home";
        }

        GameManager.Instance.loadingFromMenu = true;

        UIJuice juice = GetComponent<UIJuice>();
        if (juice != null)
        {
            juice.PlayButtonClick();
        }

        // 3. Load the scene
        // GameManager.LoadState will automatically run on OnSceneLoaded
        SceneManager.LoadScene(lastScene);
    }
}