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
        if (initiateInteractionGO != null && collision.CompareTag("Player"))
        {
            //Debug.Log("TRIGGER IS ALIVE");
            initiateInteractionGO.SetActive(true); // Enable the GameObject

            popupCanvasGO.SetActive(false); // Enable the GameObject
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