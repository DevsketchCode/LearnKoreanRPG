using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Collectables
{
    class InteractionButton : MonoBehaviour
    {
        public enum ButtonAction
        {
            None,
            Show,
            Hide,
            ToggleShowHide
        }

        public UIManager uiManager;

        public GameObject GOToInteractWith1; // Assign the GameObject

        public ButtonAction buttonAction1; // Assign the button action

        public GameObject GOToInteractWith2; // Assign the GameObject

        public ButtonAction buttonAction2; // Assign the button action

        public GameObject GOToInteractWith3; // Assign the GameObject

        public ButtonAction buttonAction3; // Assign the button action


        public void Start()
        {
            // UPDATED: Using Singleton Instance to find the UIManager.
            // This is safer and more performant than FindObjectOfType.
            if (uiManager == null) uiManager = UIManager.Instance;

            if (uiManager != null && uiManager.popupTranslationCanvas != null)
            {
                GOToInteractWith1 = uiManager.popupTranslationCanvas;
            }
        }
        public void OnButtonClicked() // Call this function when the button is clicked (set in Button's OnClick event in Inspector)
        {
            // Get the active Game Object if there is one, this is crucial for closing the window
            GOToInteractWith2 = ActiveTranslationManager.Instance.ActiveGameObject;

            // 1. Get the WordsLearned script from the parent of the button
            // Try to find the script on the root of this object's hierarchy
            // (This works better if the button is a child of the Tree/Woman object)
            WordsLearned localWordScript = transform.root.GetComponentInChildren<WordsLearned>();
            if (localWordScript != null)
            {
                Debug.Log($"[InteractionButton] Button Clicked for: {localWordScript.gameObject.name}");

                // This call MUST happen to package the data and trigger the Manager's StartSession
                localWordScript.OnCollect();
            }


            Debug.Log("Button Clicked: Interaction GameObject: " + GOToInteractWith1.name + ", Action1: " + buttonAction1 + ", Action2: " + buttonAction2);
            if (GOToInteractWith1 != null)
            {
                ShowHideGO(GOToInteractWith1, buttonAction1);
            }

            if (GOToInteractWith2 == null) 
            {
                // Get the InitiateInteraction object if the active object was found
                if (GOToInteractWith2 != null && GOToInteractWith2.name == "CollectionTrigger")
                {
                    // Get the sibling of CollectionTrigger
                    GOToInteractWith2 = GOToInteractWith2.transform.parent.Find("InitiateInteractionCanvas").gameObject;
                }
            }

            if (GOToInteractWith2 != null)
            {
                ShowHideGO(GOToInteractWith2, buttonAction2);
            } 

            if (GOToInteractWith3 != null)
            {
                ShowHideGO(GOToInteractWith3, buttonAction3);
            }
        }

        public void ShowHideGO(GameObject go, ButtonAction btnAction)
        {
            // 1. Find the juice script on the child (Panel_Background)
            UIJuice juice = go.GetComponentInChildren<UIJuice>(true); // 'true' finds it even if inactive
            Debug.Log("[ALERT] UIJuice GameObject: " + go.name);

            if (go.activeSelf && (btnAction == ButtonAction.ToggleShowHide || btnAction == ButtonAction.Hide))
            {
                // Clear session ONLY if we are actually Hiding
                if (UIManager.Instance != null && go == UIManager.Instance.popupTranslationCanvas)
                {
                    ActiveTranslationManager.Instance.ClearSession();
                }

                if (juice != null && go.activeInHierarchy) // Check activeInHierarchy to prevent Coroutine error
                {
                    juice.PlayExit(() => go.SetActive(false));
                }
                else
                {
                    go.SetActive(false);
                }
            }
            else if (btnAction == ButtonAction.Show || btnAction == ButtonAction.ToggleShowHide)
            {
                // Even if 'go' is already active, we force the scale reset here.
                if (juice != null)
                {
                    // Reset the specific child that was shrunk to 0
                    juice.gameObject.transform.localScale = Vector3.one;

                    // If the juice script added a CanvasGroup, reset that too
                    if (juice.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
                    {
                        cg.alpha = 1f;
                    }
                }

                // Ensure the parent is active
                go.SetActive(true);

                // Play the entrance (which will start from 0 because of the internal code)
                if (juice != null)
                {
                    juice.PlayEntrance();
                }
            }
        }
    }
}