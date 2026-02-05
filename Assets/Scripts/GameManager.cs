using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System; // Required for Action
using Assets.Scripts.Collectables;
using NUnit.Framework.Constraints;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    // Events for notifying other scripts about changes
    public event Action<int> OnWordsLearnedChanged;
    public event Action<int> OnWordsLearnedDiffChanged;
    public event Action<int> OnExperienceChanged;
    public event Action<int> OnExperienceDiffChanged;

    // Player related
    public GameObject playerPrefab;
    public GameObject playerGO;
    public float playerOffsetFromPortal = 2.0f;

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
    public string currentLanguage = "KR"; // Default language prefix
    public int money;
    public int experience;
    public int numberWordsLearned;
    private Vector3 player1WorldPos; // Changed to private as it's only used internally
    public bool loadingFromMenu = false;
    public string pendingSceneName;

    // Collectable tracking
    public Dictionary<string, bool> CollectableStates = new Dictionary<string, bool>(); // More descriptive name

    // Debug flag
    public bool freshStart;

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        if (sceneName != "_MainMenu")
        {
            if (floatingTextManager == null && uiManager != null)
            {
                // Try to get it from the UIManager if it's sitting there
                floatingTextManager = uiManager.GetComponent<FloatingTextManager>();
            }

            // Find or instantiate the player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
            {
                playerGO = Instantiate(playerPrefab);
                playerGO.tag = "Player";
            }
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
        string prefix = currentLanguage + "_";

        numberWordsLearned = PlayerPrefs.GetInt(prefix + "WordsLearned", 0);
        money = PlayerPrefs.GetInt(prefix + "Money", 0);
        experience = PlayerPrefs.GetInt(prefix + "Experience", 0);
    }

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "_MainMenu")
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
        Debug.Log($"[OnSceneLoaded] Scene Loaded: {scene.name}");

        // 1. Prevent logic from running on the Main Menu
        if (scene.name == "_MainMenu") // Ensure this matches your menu scene name exactly
        {
            return;
        }

        // 2. Find the UIManager in the NEW scene
        // This allows you to have a fresh UI for every level while the GameManager persists
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            floatingTextManager = uiManager.GetComponent<FloatingTextManager>();
        }

        // 3. Handle Player Spawning
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            playerGO = Instantiate(playerPrefab);
            playerGO.tag = "Player";
        }

        // 4. Update Player Controls reference
        playerControls = playerGO.GetComponent<Player>();

        // 5. Load the specific language state and player position
        LoadState(scene);

        // Tell the player they just spawned so they can start their "immunity" cooldown
        if (playerControls != null)
        {
            playerControls.OnSpawn();
        }

        // 6. Initialize world objects (Triggers, Words, etc.)
        InitializeCollectables();
    }

    // Helper methods
    public void ShowText(string msg, Color color, Vector3 position, Vector3 motion, float duration)
    {
        floatingTextManager.Show(msg, color, position, motion, duration);
    }

    public void SaveState(string activeScene, string enterSceneByGoing, string targetPortal, Vector3 portalPosition, Bounds portalBounds) // Added portalBounds parameter
    {
        // Find the player
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found. Can't save.");
            return;
        }

        // Create a prefix based on the current language to isolate save data
        string prefix = currentLanguage + "_";

        player1WorldPos = playerGO.transform.position;

        // --- 1. GLOBAL PERMANENT SAVE (For Title Screen) ---
        PlayerPrefs.SetString(prefix + "LastScene", activeScene);
        PlayerPrefs.SetFloat(prefix + activeScene + "PlayerPosX", player1WorldPos.x);
        PlayerPrefs.SetFloat(prefix + activeScene + "PlayerPosY", player1WorldPos.y);

        // GLOBAL POSITION SAVE: Save player position for EVERY scene
        // --- 2. TRANSITION SAVE (For Scene-to-Scene Portal) ---
        if (portalBounds != default(Bounds))
        {
            // Save the NAME of the door in the next scene (e.g., "HomeDoor")
            PlayerPrefs.SetString("TargetPortalName", targetPortal);

            // Save which scene that door lives in
            PlayerPrefs.SetString("TransitionForScene", pendingSceneName);

            // Save which direction we are "entering" from to help LoadState calculate the offset
            PlayerPrefs.SetString("EnterSceneByGoing", enterSceneByGoing);

            // Mark that we have a transition waiting
            PlayerPrefs.SetInt("HasTransition", 1);

            loadingFromMenu = false;
        }
        else
        {
            // If we saved manually (like via Menu), clear any pending portal transitions
            PlayerPrefs.SetInt("HasTransition", 0);
        }

        // Save game data (now using language prefix)
        PlayerPrefs.SetInt(prefix + "Money", money);
        PlayerPrefs.SetInt(prefix + "Experience", experience);
        PlayerPrefs.SetInt(prefix + "WordsLearned", numberWordsLearned);

        // Using Singleton Instance instead of FindObjectOfType
        if (UIManager.Instance == null)
        {
            // If the instance isn't found, we default the debug window to false
            PlayerPrefs.SetString("DebugWindow", "False");
        }
        else
        {
            // Use the Singleton Instance to get the current state of the Debug Window
            PlayerPrefs.SetString("DebugWindow", UIManager.Instance.ActivateDebugWindow.ToString());
        }

        // Mark that a save exists for this language
        PlayerPrefs.SetString(prefix + "SaveState", "True");

        // Save collectable states (rest is fine)
        SaveCollectableStates();

        PlayerPrefs.Save();
        Debug.Log("Last Saved Scene was: " + PlayerPrefs.GetString(prefix + "LastScene"));
        Debug.Log($"SCENE SAVED!! Language: {currentLanguage} Scene: {activeScene}");
        Debug.Log($"[SaveState] Scene: {activeScene} SAVED. PlayerPrefs DebugWindow: {PlayerPrefs.GetString("DebugWindow")}");

        DebugPrintAllSavedData();
    }

    public void LoadState(Scene scene)
    {

        Debug.Log($"[HANDSHAKE DEBUG] Checking for scene: {scene.name}");
        Debug.Log($"[HANDSHAKE DEBUG] TransitionForScene is: {PlayerPrefs.GetString("TransitionForScene")}");
        Debug.Log($"[HANDSHAKE DEBUG] HasTransition is: {PlayerPrefs.GetInt("HasTransition")}");

        string targetName = PlayerPrefs.GetString("TargetPortalName");

        Debug.Log($"[LoadState] Loading Scene: {scene.name} for Language: {currentLanguage}");

        string prefix = currentLanguage + "_";

        // Check for Save Data and Transition status upfront
        bool hasSaveData = PlayerPrefs.HasKey(prefix + "SaveState");
        bool hasActiveTransition = PlayerPrefs.GetInt("HasTransition", 0) == 1;
        bool isTransitionForThisScene = PlayerPrefs.GetString("TransitionForScene") == scene.name;

        // Load language-specific stats ONLY if they exist
        if (hasSaveData)
        {
            money = PlayerPrefs.GetInt(prefix + "Money");
            experience = PlayerPrefs.GetInt(prefix + "Experience");
            numberWordsLearned = PlayerPrefs.GetInt(prefix + "WordsLearned");
        }
        else
        {
            // If no save, we don't 'return' yet because we might be in a Portal Transition
            Debug.Log($"No save data found for {currentLanguage}. Proceeding to check for transitions.");
        }

        // --- POSITION SELECTION LOGIC ---
        if (loadingFromMenu && hasSaveData)
        {
            // 1. Loading from Title Screen: Use the absolute last spot saved
            string previousScene = PlayerPrefs.GetString(prefix + "LastScene", scene.name);
            player1WorldPos = new Vector3(PlayerPrefs.GetFloat(prefix + previousScene + "PlayerPosX"), PlayerPrefs.GetFloat(prefix + previousScene + "PlayerPosY"), 0);
            loadingFromMenu = false; // Reset the flag
            PlayerPrefs.SetInt("HasTransition", 0); // Clear portal data
        }
        else if (hasActiveTransition && isTransitionForThisScene)
        {
            // PATH B: Portal Transition
            GameObject targetPortal = GameObject.Find(targetName);

            // If Find fails, try one more time by searching all objects (slower but foolproof for first-load)
            if (targetPortal == null)
            {
                LevelChanger[] allPortals = Resources.FindObjectsOfTypeAll<LevelChanger>();
                foreach (var p in allPortals)
                {
                    if (p.name == targetName)
                    {
                        targetPortal = p.gameObject;
                        break;
                    }
                }
            }

            Debug.Log($"[HANDSHAKE START] Searching for portal named: '{targetName}'");

            if (targetPortal != null)
            {
                Vector3 spawnPos = targetPortal.transform.position;
                string direction = PlayerPrefs.GetString("EnterSceneByGoing");

                if (direction == "Up") spawnPos.y += playerOffsetFromPortal;
                else if (direction == "Down") spawnPos.y -= playerOffsetFromPortal;
                else if (direction == "Left") spawnPos.x -= playerOffsetFromPortal;
                else if (direction == "Right") spawnPos.x += playerOffsetFromPortal;

                player1WorldPos = spawnPos;
                Debug.Log($"[HANDSHAKE SUCCESS] Found {targetName}. Spawning at {player1WorldPos}");

                // Clear transition after successful use
                PlayerPrefs.SetInt("HasTransition", 0);
            }
        }
        else if (hasSaveData)
        {
            // PATH C: Fallback (This only runs if Path A and B didn't)
            player1WorldPos.x = PlayerPrefs.GetFloat(prefix + scene.name + "PlayerPosX");
            player1WorldPos.y = PlayerPrefs.GetFloat(prefix + scene.name + "PlayerPosY");
            Debug.Log($"[PATH C] Using saved position for {scene.name}");
        }
        else
        {
            // NO SAVE AND NO TRANSITION: Fresh Start/Default Position
            Debug.Log("No save data or transition found. Starting at default scene position.");
            // If playerGO already exists in the scene, keep its current position, else use zero
            playerGO = GameObject.FindGameObjectWithTag("Player");
            player1WorldPos = playerGO != null ? playerGO.transform.position : Vector3.zero;

            InitializeCollectables();
            // We don't return here anymore so the final move logic can run
        }

        // FINAL MOVE: Apply the position once
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            playerGO = Instantiate(playerPrefab);
            playerGO.tag = "Player";
        }

        // APPLY POSITION ONCE
        playerGO.transform.position = player1WorldPos;

        // RE-LINK CONTROLS
        playerControls = playerGO.GetComponent<Player>();
        if (playerControls != null)
        {
            playerControls.EnableMovement();
        }

        Debug.Log("[LOAD STATE] Scene: " + scene.name + ", Player Position: " + player1WorldPos.ToString());

        // UI and Collectable logic...
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>(); // Find the UIManager
        if (uiManager != null) uiManager.ActivateDebugWindow = (PlayerPrefs.GetString("DebugWindow") == "True");

        LoadCollectableStates();
        InitializeCollectables();
        Debug.Log($"[LoadState] Scene: {scene.name} Load Complete. Pos: {player1WorldPos}");
    }

    public void SaveAndGoToMainMenu()
    {
        // 1. Get the current scene name
        string currentScene = SceneManager.GetActiveScene().name;

        // 2. Trigger the existing SaveState logic. 
        // We pass empty/default values for portal params because we are doing a manual save.
        SaveState(currentScene, "", "", Vector3.zero, new Bounds());

        // 3. Load the Main Menu
        SceneManager.LoadScene("_MainMenu");
    }

    // WordsLearned property with event firing
    public int WordsLearned
    {
        get => numberWordsLearned; // Expression body for getter
        set
        {
            // Calculate the difference (+1 or -1)
            int difference = value - numberWordsLearned;

            numberWordsLearned = value;
            PlayerPrefs.SetInt("WordsLearned", numberWordsLearned);
            PlayerPrefs.Save();

            OnWordsLearnedChanged?.Invoke(numberWordsLearned);

            // Fire the juice event if there was a change
            if (difference != 0)
            {
                OnWordsLearnedDiffChanged?.Invoke(difference);
            }
        }
    }

    // Experience property with event firing
    public int Experience
    {
        get => experience; // Expression body for getter
        set
        {
            int difference = value - experience; // Calculate the change
            experience = value;
            PlayerPrefs.SetInt("Experience", experience);
            PlayerPrefs.Save();

            // Fire the standard event for the total number
            OnExperienceChanged?.Invoke(experience);

            // Just fire the event with the difference
            // The UIManager will handle the floating text
            if (difference != 0)
            {
                OnExperienceDiffChanged?.Invoke(difference);
            }
        }
    }

    // Collectable state management
    private void SaveCollectableStates()
    {
        PlayerPrefs.SetString("CollectableStates", DictionaryToString(CollectableStates));
    }

    private void LoadCollectableStates()
    {
        Debug.Log("LoadCollectableStates for Scene: " + GameManager.Instance.pendingSceneName);
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

            // WordsLearned Specific Handling
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

                    // Debug.Log($"[InitializeCollectables] No saved data found in dictionary for word: '{englishWord} : {collectable.transform.root.name}' in scene. Keeping default 'New' level.");
                    
                    // It will remain at its default "New" level, which is fine for newly instantiated words or words not yet learned.
                }
            }
            // End of WordsLearned Specific Handling
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

    // This ensures that whenever a player exits (even via a menu in the middle of a level), their progress is saved first.
    public void QuitGame()
    {
        Debug.Log("GameManager handling Quit...");
        if (SceneManager.GetActiveScene().name != "_MainMenu")
        {
            Debug.Log("Game saves throughout the game, not on game quit (from menu)");
            // 1. Always save before leaving! 
            // We pass nulls/defaults because we just want a snapshot of current stats/pos
            SaveState(SceneManager.GetActiveScene().name, "", "", Vector3.zero, new Bounds());
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}