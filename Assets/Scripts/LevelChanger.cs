using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : Collidable
{
    public string sceneToLoad;
    public enum EnteredFromDropdown
    {
        Top, 
        Bottom, 
        Left, 
        Right
    }
    public EnteredFromDropdown EnteredFrom;
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
            sceneLoadingInitiated = true; // If OnCollide is somehow called again very quickly, the early exit condition will prevent a second scene load

            string currentSceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"[LevelChanger] Player collided with LevelChanger. Current Scene: {currentSceneName}, Scene to Load: {sceneToLoad}, EnteredFrom: {EnteredFrom}");

            // **DISABLE PLAYER MOVEMENT IMMEDIATELY on collision**
            if (player != null)
            {
                player.DisableMovement();
                
            }
            else
            {
                Debug.LogError("PlayerMovement reference is null in Portal, cannot disable movement!");
            }

            if (sceneToLoad == "Town1") // Entering Town1
            {
                PlayerPrefs.SetString("LastEnteredFrom", EnteredFrom.ToString());
                PlayerPrefs.Save();
                Debug.Log($"[LevelChanger - Town1 Entry] Saving EnteredFrom: {EnteredFrom} as LastEnteredFrom when entering Town1");
            }
            else if (currentSceneName == "Town1") // Leaving Town1 - Pass LevelChanger position and bounds
            {
                if (boxCollider2d != null)
                {
                    GameManager.Instance.SaveState(currentSceneName, EnteredFrom.ToString(), transform.position, boxCollider2d.bounds); // Pass LevelChanger position and bounds
                    Debug.Log($"[LevelChanger - Town1 Exit] Passing EnteredFrom: {EnteredFrom}, LevelChanger Position: {transform.position.y}, LevelChanger Bounds: MinX={boxCollider2d.bounds.min.x}, MaxX={boxCollider2d.bounds.max.x} to SaveState when leaving Town1");
                }
                else
                {
                    Debug.LogError("BoxCollider2D is null on LevelChanger, cannot save bounds!");
                    GameManager.Instance.SaveState(currentSceneName, EnteredFrom.ToString(), transform.position, new Bounds()); // Pass default Bounds if collider missing (error case)
                }
            }
            else // Leaving other scene
            {
                if (boxCollider2d != null)
                {
                    GameManager.Instance.SaveState(currentSceneName, EnteredFrom.ToString(), transform.position, boxCollider2d.bounds); // Pass LevelChanger position and bounds
                    Debug.Log($"[LevelChanger - Other Scene Exit] Passing EnteredFrom: {EnteredFrom}, LevelChanger Position: {transform.position.y}, LevelChanger Bounds: MinX={boxCollider2d.bounds.min.x}, MaxX={boxCollider2d.bounds.max.x} to SaveState when leaving scene: {currentSceneName} (not Town1)");
                }
                else
                {
                    Debug.LogError("BoxCollider2D is null on LevelChanger, cannot save bounds!");
                    GameManager.Instance.SaveState(currentSceneName, EnteredFrom.ToString(), transform.position, new Bounds()); // Pass default Bounds if collider missing
                }
            }

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