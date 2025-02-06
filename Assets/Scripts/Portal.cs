using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : Collidable
{
    public string sceneToLoad; // No need to serialize activeScene, it's set dynamically.
    public string enteredFrom;

    protected override void OnCollide(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            GameManager.Instance.SaveState(SceneManager.GetActiveScene().name, enteredFrom); // No need for activeScene variable

            // Use an asynchronous load operation:
            StartCoroutine(LoadSceneAsync(sceneToLoad));
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // You can optionally track the loading progress:
        // while (!asyncLoad.isDone)
        // {
        //     float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // 0.9f is used because progress usually stops there.
        //     Debug.Log("Loading scene " + sceneName + ": " + progress * 100 + "%");
        //     yield return null;
        // }

        yield return asyncLoad; // More concise way to wait until done.

        Debug.Log("Scene loaded asynchronously: " + sceneName);

        // No need to do anything else here. GameManager handles initialization.
    }
}