using UnityEngine;

public class Collectable : MonoBehaviour
{
    public enum CollectionType
    {
        Item,
        Currency,
        OnlyExperience,
        WordsLearned
    }

    public enum ActionOnCollect
    {
        None,
        Deactivate,
        Destroy,
        HideVisuals // Keep the script alive but hide the object
    }

    [SerializeField] private CollectionType collectionType = CollectionType.Item;
    public string CollectableID;

    public ActionOnCollect actionOnCollect;

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(CollectableID))
        {
            CollectableID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
    }

    protected virtual void Start() { }

    protected virtual void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            OnCollect();
        }
    }

    public void Initialize() { }

    protected virtual void OnCollect()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.instance is NULL! Cannot collect.");
            return;
        }

        if (collectionType == CollectionType.WordsLearned)
        {
            var wordDisplay = GetComponent<WordDisplay>();
            if (wordDisplay != null)
            {
                CollectableID = wordDisplay.wordID;
            }
        }

        if (string.IsNullOrEmpty(CollectableID))
        {
            Debug.LogError($"Collectable on {gameObject.name} has no ID!");
            return;
        }

        GameManager.Instance.CollectableStates[CollectableID] = true;
        PlayerPrefs.SetInt("Collectable_" + CollectableID, 1);
        PlayerPrefs.Save();

        GameManager.Instance.SaveState(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, "", Vector3.zero, new Bounds());

        ShouldDeactivateOnCollect();
    }

    protected void ShouldDeactivateOnCollect()
    {
        if (actionOnCollect == ActionOnCollect.Deactivate)
        {
            gameObject.SetActive(false);
        }
        else if (actionOnCollect == ActionOnCollect.Destroy)
        {
            Destroy(gameObject);
        }
        else if (actionOnCollect == ActionOnCollect.HideVisuals)
        {
            // Disable only collider and renderer so scripts stay active
            if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;
            if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;

            // Also check children for renderers (like graphics/sprites)
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;

            Debug.Log($"[Collectable] {gameObject.name} hidden via HideVisuals. Script is still alive.");
        }
    }
}