using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System; // Required for Action
using Assets.Scripts.Collectables;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    // Events for notifying other scripts about changes
    public event Action<int> OnWordsLearnedChanged;
    public event Action<int> OnExperienceChanged;

    // Player related
    public GameObject playerPrefab;
    public GameObject playerGO;

    // Resources
    public List<Sprite> playerSprites;
    public List<Sprite> attachmentSprites;
    public List<int> attachmentPrices;
    public List<int> xpTable;
    public Dictionary<string, WordData> wordsLearnedDictionary = new Dictionary<string, WordData>();

    // References
    public Player playerControls; 
    public FloatingTextManager floatingTextManager;
    public UIManager uiManager;


    // Game data
    public int koreanWon;
    public int experience;
    public int numberWordsLearned;
    private Vector3 player1WorldPos; // Changed to private as it's only used internally

    // Collectable tracking
    public Dictionary<string, bool> CollectableStates = new Dictionary<string, bool>(); // More descriptive name

    // Debug flag
    public bool freshStart;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find or instantiate the player
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            playerGO = Instantiate(playerPrefab);
            playerGO.tag = "Player";
        }

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Persist this object across scenes
        DontDestroyOnLoad(gameObject);

        // Handle fresh start or load from PlayerPrefs
        if (freshStart)
        {
            freshStart = false;
            ClearPlayerPrefs();
        }
        else
        {
            // Check for first run and clear PlayerPrefs if so.  This prevents issues with old save data.
            if (PlayerPrefs.GetInt("FirstRun", 1) == 1)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetInt("FirstRun", 0);
                PlayerPrefs.Save();
            }
            else
            {
                LoadCollectableStates();
            }
        }

        // Load game data from PlayerPrefs
        numberWordsLearned = PlayerPrefs.GetInt("WordsLearned", 0);
        koreanWon = PlayerPrefs.GetInt("KoreanWon", 0);
        experience = PlayerPrefs.GetInt("Experience", 0);
    }

    private void Start()
    {
        // Get PlayerMovement reference in Start (more efficient)
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player"); // Find Player GO
        if (playerGameObject != null)
        {
            playerControls = playerGameObject.GetComponent<Player>(); // Get PlayerMovement component
            if (playerControls == null)
            {
                Debug.LogError("PlayerMovement script not found on Player GameObject!");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found with tag 'Player'!");
        }
    }

    void Update() // For TESTING - REMOVE LATER!
    {
        if (Input.GetKeyDown(KeyCode.P)) // Press 'P' key to print dictionary to console (for testing)
        {
            Debug.Log("--- WordsLearnedDictionary Contents (Press 'P' to Refresh) ---");
            foreach (var pair in wordsLearnedDictionary)
            {
                // Debug.Log($"Word: '{pair.Key}', AltLang: {pair.Value.altLangWord}, WordType: {pair.Value.wordDataType}, Knowledge Level: {pair.Value.KnowledgeLevel}"); // Access properties of WordData object

                Debug.Log($"Word: '{pair.Key}'\nWord Data: {pair.Value}"); // Directly use the WordData object, with its ToString override
            }
            Debug.Log("--- End of Dictionary ---");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[OnSceneLoaded] Scene Loaded: {scene.name}, Mode: {mode}, Time: {Time.time} - SceneLoadCompleted"); 
        LoadState(scene);
        InitializeCollectables();
    }

    // Helper methods
    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        floatingTextManager.Show(msg, fontSize, color, position, motion, duration);
    }

    public void SaveState(string activeScene, string enteredFrom, Vector3 portalPosition, Bounds portalBounds) // Added portalBounds parameter
    {
        // Find the player
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found. Can't save.");
            return;
        }

        player1WorldPos = playerGO.transform.position;
        // Don't need to save the player position at this time for any other scene.
        if(activeScene == "Town1")
        {
            // Debug logs for loading (before offset)
            //Debug.Log($"[LoadState - Town1] Loaded PlayerPosX: {PlayerPrefs.GetFloat("PlayerPosX")}, PlayerPosY: {PlayerPrefs.GetFloat("PlayerPosY")}");


            // Save player position - With Offset
            PlayerPrefs.SetFloat("PlayerPosX", portalBounds.center.x);

            if (enteredFrom == "Bottom")
            {
                PlayerPrefs.SetFloat("PlayerPosY", (portalBounds.min.y - 0.5f));
            }
            else if (enteredFrom == "Top")
            {
                PlayerPrefs.SetFloat("PlayerPosY", (portalBounds.max.y + 0.5f));
            } 
            else
            {
                PlayerPrefs.SetFloat("PlayerPosY", player1WorldPos.y);
            }

            //Debug.Log($"[LoadState - Town1] Position after Y and X offset: X={player1WorldPos.x}, Y={player1WorldPos.y}"); // Log position after offsets
        }

        // Debug.Log($"[SaveState] Scene: {activeScene}, Leaving from: {enteredFrom}, Saving Player Pos: X={player1WorldPos.x}, Y={player1WorldPos.y}, LevelChanger Y={portalPosition.y}, LevelChanger MinX={portalBounds.min.x}, LevelChanger MaxX={portalBounds.max.x}");
        // Debug.Log($"[SaveState] Saved PlayerPosX: {PlayerPrefs.GetFloat("PlayerPosX")}, PlayerPosY: {PlayerPrefs.GetFloat("PlayerPosY")}");

        // Save game data (rest is fine)
        PlayerPrefs.SetInt("KoreanWon", koreanWon);
        PlayerPrefs.SetInt("Experience", experience);
        PlayerPrefs.SetInt("WordsLearned", numberWordsLearned);

        // Using Singleton Instance instead of FindObjectOfType
        if (UIManager.Instance == null)
        {
            // If the instance isn't found, we default the debug window to false
            // Debug.Log("UIManager.Instance was NULL during SaveState");
            PlayerPrefs.SetString("DebugWindow", "False");
        }
        else
        {
            // Use the Singleton Instance to get the current state of the Debug Window
            PlayerPrefs.SetString("DebugWindow", UIManager.Instance.ActivateDebugWindow.ToString());
        }

        PlayerPrefs.SetString("SaveState", "True");

        // Save collectable states (rest is fine)
        SaveCollectableStates();

        PlayerPrefs.Save();
        Debug.Log("SCENE SAVED!! Scene: " + activeScene);
        Debug.Log($"[SaveState] Scene: {activeScene} SAVED. PlayerPrefs DebugWindow: {PlayerPrefs.GetString("DebugWindow")}");

        DebugPrintAllSavedData();
    }

    public void LoadState(Scene scene)
    {
        Debug.Log($"[LoadState] Loading Scene: {scene.name}");

        if (!PlayerPrefs.HasKey("SaveState"))
        {
            Debug.Log("No save data found. Player will start at default position.");
            InitializeCollectables();
            return;
        }

        koreanWon = PlayerPrefs.GetInt("KoreanWon");
        experience = PlayerPrefs.GetInt("Experience");
        numberWordsLearned = PlayerPrefs.GetInt("WordsLearned");

        if (scene.name == "Town1")
        {
            player1WorldPos = new Vector3(PlayerPrefs.GetFloat("PlayerPosX"), PlayerPrefs.GetFloat("PlayerPosY"), 0);

            // Find or instantiate the player.
            playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
            {
                playerGO = Instantiate(playerPrefab);
                playerGO.tag = "Player";
            }

            if (playerGO != null)
            {
                playerGO.transform.position = player1WorldPos; // Set player position - NOW with Y and X offset
            }
            if (playerControls != null)
            {
                playerControls.EnableMovement();
            }

            //Debug.Log($"[LoadState - Town1] Setting Player Position: X={playerGO.transform.position.x}, Y={playerGO.transform.position.y} (GameObject)"); // Log final player position from GameObject
        }
        if (uiManager == null)
        {
            // Debug.Log("UIMANAGER was NULL");
            uiManager = FindObjectOfType<UIManager>(); // Find the UIManager
        }

        if (uiManager != null)
        {
            // Debug.Log("UIMANAGER EXISTS: Before: " + uiManager.ActivateDebugWindow);
            uiManager.ActivateDebugWindow = (PlayerPrefs.GetString("DebugWindow") == "True");
            // Debug.Log("UIMANAGER EXISTS: After: " + uiManager.ActivateDebugWindow);
        }

        LoadCollectableStates();
        InitializeCollectables();
        //Debug.Log("Loading Complete for " + scene.name + ". Experience: " + experience + " // Words Learned: " + numberWordsLearned);
        Debug.Log($"[LoadState] Scene: {scene.name} PlayerPrefs DebugWindow: {PlayerPrefs.GetString("DebugWindow")}");
    }

    // WordsLearned property with event firing
    public int WordsLearned
    {
        get => numberWordsLearned; // Expression body for getter
        set
        {
            numberWordsLearned = value;
            PlayerPrefs.SetInt("WordsLearned", numberWordsLearned);
            PlayerPrefs.Save();

            OnWordsLearnedChanged?.Invoke(numberWordsLearned);
        }
    }

    // Experience property with event firing
    public int Experience
    {
        get => experience; // Expression body for getter
        set
        {
            experience = value;
            PlayerPrefs.SetInt("Experience", experience);
            PlayerPrefs.Save();

            OnExperienceChanged?.Invoke(experience);
        }
    }

    // Collectable state management
    private void SaveCollectableStates()
    {
        PlayerPrefs.SetString("CollectableStates", DictionaryToString(CollectableStates));
    }

    private void LoadCollectableStates()
    {
        string collectableStatesString = PlayerPrefs.GetString("CollectableStates", "");
        CollectableStates = StringToDictionary(collectableStatesString);
    }

    private void InitializeCollectables()
    {
        // Clear the dictionary first to prevent duplicate keys.
        CollectableStates.Clear();

        // Get all Collectable components in the scene.
        Collectable[] collectionTriggers = FindObjectsOfType<Collectable>();

        foreach (Collectable collectable in collectionTriggers)
        {
            // Check if the collectable is already in the dictionary.
            if (!CollectableStates.ContainsKey(collectable.CollectableID))
            {
                // If not in the dictionary, load its state from PlayerPrefs.
                string collectedKey = "Collectable_" + collectable.CollectableID;
                bool isCollected = PlayerPrefs.GetInt(collectedKey, 0) == 1;
                CollectableStates[collectable.CollectableID] = isCollected; // Set based on PlayerPrefs

                if (isCollected)
                {
                    // Destroy(collectable.gameObject); // Destroy it immediately if collected.
                    collectable.gameObject.SetActive(false);
                }
            }

            // **NEW CODE - WordsLearned Specific Handling - ADD THIS BLOCK**
            if (collectable is WordsLearned wordObject) // Check if it's a WordsLearned object
            {
                string englishWord = wordObject.GetEnglishWord(); // **Need to create GetEnglishWord() function in WordsLearned.cs - Step B**

                if (!string.IsNullOrEmpty(englishWord) && wordsLearnedDictionary.ContainsKey(englishWord))
                {
                    // We have saved data for this word in the dictionary!
                    WordData wordData = wordsLearnedDictionary[englishWord];
                    WordsLearned.WordKnowledgeLevel savedKnowledgeLevel = wordData.KnowledgeLevel;

                    // Update the WordKnowledgeLevel on the WordsLearned script in the scene
                    wordObject.SetKnowledgeLevel(savedKnowledgeLevel); // **Use the SetKnowledgeLevel() function we created earlier**

                    Debug.Log($"[InitializeCollectables] Loaded Word Knowledge Level: '{savedKnowledgeLevel}' for word: '{englishWord}' from dictionary and applied to scene object.");
                }
                else
                {
                    // Word is in scene, but no saved data in dictionary (maybe a new word in this scene instance)
                    Debug.Log($"[InitializeCollectables] No saved data found in dictionary for word: '{englishWord}' in scene. Keeping default 'New' level.");
                    // It will remain at its default "New" level, which is fine for newly instantiated words or words not yet learned.
                }
            }
            // **End of NEW CODE - WordsLearned Specific Handling**
        }
    }

    // Utility methods
    private string DictionaryToString(Dictionary<string, bool> dict)
    {
        string result = "";
        foreach (var kvp in dict)
        {
            result += $"{kvp.Key}:{kvp.Value},"; // String interpolation for cleaner formatting
        }
        return result.TrimEnd(','); // Remove the trailing comma if any
    }

    private Dictionary<string, bool> StringToDictionary(string str)
    {
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        if (!string.IsNullOrEmpty(str))
        {
            string[] pairs = str.Split(',');
            foreach (string pair in pairs)
            {
                if (!string.IsNullOrEmpty(pair))
                {
                    string[] parts = pair.Split(':');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        if (bool.TryParse(parts[1], out bool value))
                        { // Use TryParse for robustness
                            dict[key] = value;
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse boolean value: {parts[1]}");
                        }
                    }
                }
            }
        }
        return dict;
    }

    // Clear PlayerPrefs (for debugging)
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [ContextMenu("Debug: Print All Saved Word Data")] // This allows you to run it from the Inspector!
    public void DebugPrintAllSavedData()
    {
        Debug.Log("======= [PLAYERPREFS SAVED DATA REPORT] =======");

        // 1. Print the serialized CollectableStates Dictionary
        //string rawStates = PlayerPrefs.GetString("CollectableStates", "EMPTY");
        //Debug.Log($"--- Raw CollectableStates String ---\n{rawStates}");

        // 2. Iterate through your actual runtime Dictionary to see what is currently active
        // Debug.Log("--- Current Runtime CollectableStates Dictionary ---");
        // foreach (var pair in CollectableStates)
        // {
        //    Debug.Log($"ID: {pair.Key} | IsCollected: {pair.Value}");
        // }

        // 3. Check the wordsLearnedDictionary (WordData Objects)
        Debug.Log("--- Runtime wordsLearnedDictionary (Metadata) ---");
        if (wordsLearnedDictionary.Count == 0) Debug.Log("Dictionary is empty.");
        foreach (var pair in wordsLearnedDictionary)
        {
            Debug.Log($"Word Key: {pair.Key} | Level: {pair.Value.KnowledgeLevel} | IsLearned: {pair.Value.IsLearned}");
        }

        // 4. Global Stats
        Debug.Log("--- Global Progress Stats ---");
        Debug.Log($"WordsLearned Count: {PlayerPrefs.GetInt("WordsLearned", 0)}");
        Debug.Log($"Experience: {PlayerPrefs.GetInt("Experience", 0)}");

        Debug.Log("===============================================");
    }
}