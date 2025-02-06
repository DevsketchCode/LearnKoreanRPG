using UnityEngine;

public class TextColliderManager : MonoBehaviour
{
    private GameObject popupCanvasGO; // Store the GameObject, not the Canvas component

    private void Awake()
    {
        Transform parent = transform.parent;

        if (parent != null)
        {
            // Find the PopupCanvas GameObject by name (or tag, if preferred)
            popupCanvasGO = parent.Find("PopupCanvas").gameObject; // Get the GameObject

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
        if (popupCanvasGO != null && collision.CompareTag("Player"))
        {
            Debug.Log("TRIGGER IS ALIVE");
            popupCanvasGO.SetActive(true); // Enable the GameObject
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (popupCanvasGO != null && collision.CompareTag("Player"))
        {
            Debug.Log("TRIGGER IS GOODBYE");
            popupCanvasGO.SetActive(false); // Disable the GameObject
        }
    }
}