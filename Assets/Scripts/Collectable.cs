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

    [SerializeField] private CollectionType collectionType = CollectionType.Item;
    [SerializeField] public string CollectableID;

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(CollectableID))
        {
            CollectableID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif

        // No need to call Initialize() here anymore.  GameManager handles it.
    }

    protected virtual void Start()
    {
        // No need for coroutine or any other initialization here.
    }

    protected virtual void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            OnCollect();
        }
    }

    public void Initialize()
    {
        // Now, GameManager's InitializeCollectables will have already handled this.
    }

    protected virtual void OnCollect()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.instance is NULL! Cannot collect.");
            return;
        }

        GameManager.Instance.CollectableStates[CollectableID] = true;
        PlayerPrefs.SetInt("Collectable_" + CollectableID, 1); // Save to PlayerPrefs.
        PlayerPrefs.Save();

        GameManager.Instance.SaveState(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, "");

        Destroy(gameObject);
    }
}