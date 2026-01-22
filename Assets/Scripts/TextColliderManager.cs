using Assets.Scripts.Collectables;
using UnityEngine;

public class TextColliderManager : MonoBehaviour
{
    private GameObject popupCanvasGO; // Store the GameObject
    public GameObject initiateInteractionGO;  // Store the GameObject

    private void Awake()
    {
        Transform parent = transform.parent;

        if (parent != null)
        {

            // Find the PopupCanvas GameObject by name (or tag, if preferred)
            popupCanvasGO = parent.Find("PopupCanvas").gameObject; // Get the GameObject

            if (initiateInteractionGO == null)
            {
                Debug.LogError("InitiateInteractiveCanvas GameObject not found as a sibling of TextTrigger!");
            }

            if (popupCanvasGO == null)
            {
                Debug.LogError("PopupCanvas GameObject not found as a sibling of TextTrigger!");
            }
        }
        else
        {
            Debug.LogError("TextTrigger has no parent!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Reach into the parent to find the WordsLearned script
            // The 'true' argument tells Unity to find the script even if the GameObject is disabled
            WordsLearned wordsLearned = transform.parent.GetComponentInChildren<WordsLearned>(true);

            if (wordsLearned != null)
            {
                // Force the word to re-check the GameManager dictionary right now
                // We will need to make this method public in the next step
                wordsLearned.UpdateKnowledgeLevelFromDictionary();

                // FORCE the UI to refresh its colors right now
                // I'm adding a call to the color update method here
                wordsLearned.UpdateKnowledgeLevelButtonColor();

                Debug.Log($"[TextColliderManager] Refreshed data for {wordsLearned.CollectableID}.");
            }
            else
            {
                Debug.LogError($"[COLLIDER] Could not find WordsLearned script in parent of {gameObject.name}!");
            }

            if (initiateInteractionGO != null)
            {
                //Debug.Log("TRIGGER IS ALIVE");
                initiateInteractionGO.SetActive(true); // Enable the GameObject
                popupCanvasGO.SetActive(false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (initiateInteractionGO != null && collision.CompareTag("Player"))
        {
            //Debug.Log("TRIGGER IS GOODBYE");
            initiateInteractionGO.SetActive(false); // Disable the GameObject
        }

        if (popupCanvasGO != null && collision.CompareTag("Player"))
        {
            //Debug.Log("TRIGGER IS GOODBYE");
            popupCanvasGO.SetActive(false); // Disable the GameObject
        }
    }
}