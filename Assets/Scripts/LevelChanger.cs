using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : Collidable
{
    public string sceneToLoad;
    public enum EnterSceneByGoingDropdown
    {
        Up, 
        Down, 
        Left, 
        Right
    }
    public EnterSceneByGoingDropdown EnterSceneByGoing;
    public string targetPortalName;
    public Animator animator;
    private BoxCollider2D boxCollider2d; // Add reference to BoxCollider2D
    private bool sceneLoadingInitiated = false; // Track if scene loading has begun
    private Player player; // Reference to PlayerMovement script

    protected override void Start()
    {
        base.Start(); // Call base Start if Collidable has one
        boxCollider2d = GetComponent<BoxCollider2D>(); // Get BoxCollider2D on LevelChanger
        if (boxCollider2d == null)
        {
            Debug.LogError("LevelChanger script needs a BoxCollider2D component!");
        }

        // Get PlayerMovement reference in Start (more efficient)
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player"); // Find Player GO
        if (playerGameObject != null)
        {
            player = playerGameObject.GetComponent<Player>(); // Get PlayerMovement component
            if (player == null)
            {
                Debug.LogError("PlayerMovement script not found on Player GameObject!");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found with tag 'Player'!");
        }
    }


    protected override void OnCollide(Collider2D coll)
    {
        if (sceneLoadingInitiated) return; // Early Exit Check

        if (coll.CompareTag("Player"))
        {
            // Check if the player is allowed to teleport yet
            if (player != null && !player.CanTransition())
            {
                return; // Exit if they just spawned
            }

            sceneLoadingInitiated = true; // If OnCollide is somehow called again very quickly, the early exit condition will prevent a second scene load

            GameManager.Instance.pendingSceneName = sceneToLoad;
            string currentSceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"[LevelChanger] Player collided with LevelChanger. Current Scene: {currentSceneName}, Scene to Load: {sceneToLoad}, EnterSceneByGoing: {EnterSceneByGoing}");

            // **DISABLE PLAYER MOVEMENT IMMEDIATELY on collision**
            if (player != null)
            {
                player.DisableMovement();
            }
            else
            {
                Debug.LogError("PlayerMovement reference is null in Portal, cannot disable movement!");
            }

            // 1. Tell the GameManager which scene we are EXPECTING to load
            PlayerPrefs.SetString("TransitionForScene", sceneToLoad);
            // 2. Flip the switch to tell LoadState to use Path B (The Handshake)
            PlayerPrefs.SetInt("HasTransition", 1);
            // 3. Ensure the portal name is exactly what we want to find
            PlayerPrefs.SetString("TargetPortalName", targetPortalName);

            PlayerPrefs.SetString("EnterSceneByGoing", EnterSceneByGoing.ToString());
            Debug.Log($"[LevelChanger] Saving state for {currentSceneName}. Transitioning to {sceneToLoad} via {targetPortalName}");

            // We must call SaveState so GameManager knows the bounds of the portal we are currently using
            if (boxCollider2d != null)
            {
                GameManager.Instance.SaveState(currentSceneName, EnterSceneByGoing.ToString(), targetPortalName, transform.position, boxCollider2d.bounds);
                Debug.Log($"[LevelChanger] Saving state for {currentSceneName}. Transitioning to {sceneToLoad} via {targetPortalName}");
            }
            else
            {
                Debug.LogError("BoxCollider2D is null on LevelChanger, cannot save bounds!");
                GameManager.Instance.SaveState(currentSceneName, EnterSceneByGoing.ToString(), targetPortalName, transform.position, new Bounds());
            }

            PlayerPrefs.Save();

            // Disable LevelChanger Collider IMMEDIATELY!
            if (boxCollider2d != null)
            {
                boxCollider2d.enabled = false; // DEACTIVATE COLLIDER
                Debug.Log($"[LevelChanger] LevelChanger Collider DISABLED: {gameObject.name}, Scene Load Started: {sceneToLoad}"); // Log when collider is disabled
            }

            // Fade to black BEFORE loading scene

            // StartCoroutine(StartSceneTransition(sceneToLoad)); // Call new coroutine for transition
            FadeToLevel();
            Debug.Log($"[LevelChanger] Starting LoadSceneAsync for: {sceneToLoad}");
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        Debug.Log($"[LevelChanger - LoadSceneAsync] Async load started for: {sceneToLoad}");
        yield return asyncLoad;
        Debug.Log($"[LevelChanger - LoadSceneAsync] Scene loaded asynchronously: {sceneToLoad}");
    }

    private void FadeToLevel()
    {
        if (animator == null)
        {
            animator = this.GetComponent<Animator>(); // Get LevelChanger's Animator component
        }
        
        animator.SetTrigger("FadeOut");
    }
}