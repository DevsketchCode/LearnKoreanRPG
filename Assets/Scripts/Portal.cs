using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : Collidable
{
    public string sceneToLoad;
    public string enteredFrom;
    private BoxCollider2D boxCollider2d; // Add reference to BoxCollider2D

    protected override void Start()
    {
        base.Start(); // Call base Start if Collidable has one
        boxCollider2d = GetComponent<BoxCollider2D>(); // Get BoxCollider2D on Portal
        if (boxCollider2d == null)
        {
            Debug.LogError("Portal script needs a BoxCollider2D component!");
        }
    }


    protected override void OnCollide(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"[Portal] Player collided with portal. Current Scene: {currentSceneName}, Scene to Load: {sceneToLoad}, EnteredFrom: {enteredFrom}");

            if (sceneToLoad == "Town1") // Entering Town1
            {
                PlayerPrefs.SetString("LastEnteredFrom", enteredFrom);
                PlayerPrefs.Save();
                Debug.Log($"[Portal - Town1 Entry] Saving EnteredFrom: {enteredFrom} as LastEnteredFrom when entering Town1");
            }
            else if (currentSceneName == "Town1") // Leaving Town1 - Pass portal position and bounds
            {
                if (boxCollider2d != null)
                {
                    GameManager.Instance.SaveState(currentSceneName, enteredFrom, transform.position, boxCollider2d.bounds); // Pass portal position and bounds
                    Debug.Log($"[Portal - Town1 Exit] Passing EnteredFrom: {enteredFrom}, Portal Position: {transform.position.y}, Portal Bounds: MinX={boxCollider2d.bounds.min.x}, MaxX={boxCollider2d.bounds.max.x} to SaveState when leaving Town1");
                }
                else
                {
                    Debug.LogError("BoxCollider2D is null on Portal, cannot save bounds!");
                    GameManager.Instance.SaveState(currentSceneName, enteredFrom, transform.position, new Bounds()); // Pass default Bounds if collider missing (error case)
                }
            }
            else // Leaving other scene
            {
                if (boxCollider2d != null)
                {
                    GameManager.Instance.SaveState(currentSceneName, enteredFrom, transform.position, boxCollider2d.bounds); // Pass portal position and bounds
                    Debug.Log($"[Portal - Other Scene Exit] Passing EnteredFrom: {enteredFrom}, Portal Position: {transform.position.y}, Portal Bounds: MinX={boxCollider2d.bounds.min.x}, MaxX={boxCollider2d.bounds.max.x} to SaveState when leaving scene: {currentSceneName} (not Town1)");
                }
                else
                {
                    Debug.LogError("BoxCollider2D is null on Portal, cannot save bounds!");
                    GameManager.Instance.SaveState(currentSceneName, enteredFrom, transform.position, new Bounds()); // Pass default Bounds if collider missing
                }
            }

            // Load Scene
            StartCoroutine(LoadSceneAsync(sceneToLoad));
            Debug.Log($"[Portal] Starting LoadSceneAsync for: {sceneToLoad}");
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        Debug.Log($"[Portal - LoadSceneAsync] Async load started for: {sceneName}");
        yield return asyncLoad;
        Debug.Log($"[Portal - LoadSceneAsync] Scene loaded asynchronously: {sceneName}");
    }
}