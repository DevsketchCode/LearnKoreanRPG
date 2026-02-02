using Assets.Scripts.Collectables;
using UnityEngine;

public class TextColliderManager : MonoBehaviour
{
    public GameObject initiateInteractionGO;  // Store the GameObject

    private void Awake()
    {
        Transform parent = transform.parent;

        if (parent != null)
        {
            if (initiateInteractionGO == null)
            {
                Transform interactionTransform = parent.Find("InitiateInteractionCanvas");
                if (interactionTransform != null)
                {
                    initiateInteractionGO = interactionTransform.gameObject;
                }
                else
                {
                    Debug.LogError("InitiateInteractiveCanvas GameObject not found as a sibling of TextTrigger!");
                }
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
            // 1. Show the "..." interaction prompt
            if (initiateInteractionGO != null) initiateInteractionGO.SetActive(true);

            // 2. Get the word script on THIS object
            WordsLearned wordsLearned = transform.parent.GetComponentInChildren<WordsLearned>(true);

            if (wordsLearned != null)
            {
                // Update visuals ONLY
                wordsLearned.UpdateKnowledgeLevelFromDictionary();
                wordsLearned.UpdateKnowledgeLevelButtonColor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (initiateInteractionGO != null)
            {
                initiateInteractionGO.SetActive(false);
            }

            if (UIManager.Instance != null)
            {
                // IMPORTANT: Only clear the lock if the window is NOT currently open.
                // If the window is open, let the WordButton handle the unlock.
                if (!UIManager.Instance.IsStudySessionActive)
                {
                    // Only clear if THIS word was the one being tracked
                    WordsLearned wordsLearned = transform.parent.GetComponentInChildren<WordsLearned>(true);
                    if (UIManager.Instance.activeWordScript == wordsLearned)
                    {
                        UIManager.Instance.activeWordScript = null;
                    }
                }

                // If the player walks away, hide the prompt, but don't force-close 
                // the translation window if they are still clicking buttons.
            }
        }
    }
}