using UnityEngine;
using UnityEditor;
using Assets.Scripts.Collectables;

public class SetInstanceGUID : EditorWindow
{
    private GameObject targetObject; // The GameObject to assign the GUID to.

    [MenuItem("Tools/Set Instance GUID")] // Adds a menu item under "Tools"
    public static void ShowWindow()
    {
        GetWindow<SetInstanceGUID>("Set Instance GUID"); // Creates the editor window
    }

    private void OnGUI()
    {
        // Object field to select the target GameObject in the scene.
        // 'true' allows selecting scene objects, not just assets.
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Set GUID"))
        {
            if (targetObject != null)
            {
                // Get the WordsLearned component from the target object.
                // If it doesn't exist, add it.
                WordsLearned guidComponent = targetObject.GetComponent<WordsLearned>();

                if (guidComponent == null)
                {
                    guidComponent = targetObject.AddComponent<WordsLearned>();
                }

                // Generate a new GUID and assign it to the CollectableID.
                guidComponent.CollectableID = System.Guid.NewGuid().ToString();

                // Mark the component as modified so the change is saved.
                EditorUtility.SetDirty(guidComponent);

                Debug.Log("GUID set on " + targetObject.name + ": " + guidComponent.CollectableID);
            }
            else
            {
                Debug.LogError("Please select a target GameObject.");
            }
        }
    }
}