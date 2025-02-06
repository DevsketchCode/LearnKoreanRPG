using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System; // Required for Action

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    // Events for notifying other scripts about changes
    public event Action<int> OnWordsLearnedChanged;
    public event Action<int> OnExperienceChanged;

    // Player related
    public GameObject playerPrefab;
    private GameObject playerGO;

    // Resources
    public List<Sprite> playerSprites;
    public List<Sprite> attachmentSprites;
    public List<int> attachmentPrices;
    public List<int> xpTable;

    // References
    public Player player; // Consider if this is actually used. If not, remove.
    public FloatingTextManager floatingTextManager;

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

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadState(scene);
        InitializeCollectables();
    }

    // Helper methods
    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        floatingTextManager.Show(msg, fontSize, color, position, motion, duration);
    }

    public void SaveState(string activeScene, string enteredFrom)
    {
        // Find the player
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found. Can't save.");
            return;
        }

        player1WorldPos = playerGO.transform.position;

        // Save player position based on scene and entry direction.  Simplified logic.
        if (activeScene != "Town1")
        {
            // Adjust position slightly based on entry direction for smoother transitions.
            float yOffset = 0f;
            if (enteredFrom == "Bottom") yOffset = -0.20f;
            else if (enteredFrom == "Top") yOffset = 0.20f;
            player1WorldPos.y += yOffset;
        }
        PlayerPrefs.SetFloat("PlayerPosX", player1WorldPos.x);
        PlayerPrefs.SetFloat("PlayerPosY", player1WorldPos.y);


        // Save game data
        PlayerPrefs.SetInt("KoreanWon", koreanWon);
        PlayerPrefs.SetInt("Experience", experience);
        PlayerPrefs.SetInt("WordsLearned", numberWordsLearned);
        PlayerPrefs.SetString("SaveState", "true");

        // Save collectable states
        SaveCollectableStates();

        PlayerPrefs.Save();
    }

    public void LoadState(Scene scene)
    {
        if (!PlayerPrefs.HasKey("SaveState"))
        {
            Debug.Log("No save data found. Player will start at default position.");
            InitializeCollectables(); // Initialize collectables even without save data.
            return;
        }

        koreanWon = PlayerPrefs.GetInt("KoreanWon");
        experience = PlayerPrefs.GetInt("Experience");
        numberWordsLearned = PlayerPrefs.GetInt("WordsLearned");

        if (scene.name == "Town1")
        {
            player1WorldPos = new Vector3(PlayerPrefs.GetFloat("PlayerPosX"), PlayerPrefs.GetFloat("PlayerPosY"), 0);

            // Find or instantiate the player.  Simplified logic.
            playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
            {
                playerGO = Instantiate(playerPrefab);
                playerGO.tag = "Player";
            }
            playerGO.transform.position = player1WorldPos;
        }

        LoadCollectableStates(); // Load collectable states after other game data.
        InitializeCollectables(); // Ensure collectables are initialized.

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
                    Destroy(collectable.gameObject); // Destroy it immediately if collected.
                }
            }
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
}