using UnityEngine;

namespace Assets.Scripts.Collectables
{
    public class WordsLearned : Collectable
    {
        [SerializeField] private int experience = 0;

        protected override void OnCollect()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.instance is NULL! Cannot collect word.");
                return;
            }

            // Call the base OnCollect() to handle state and disabling.
            base.OnCollect();  // This is CRUCIAL - it handles the Destroy() call.

            GameManager.Instance.WordsLearned++;
            GameManager.Instance.Experience += experience;

            Debug.Log($"WordsLearned incremented. New value: {GameManager.Instance.WordsLearned} \nTotal Experience: {GameManager.Instance.Experience}");

            // The Collectable.OnCollect() method already handles everything else (destroying the object etc.)
        }
    }
}