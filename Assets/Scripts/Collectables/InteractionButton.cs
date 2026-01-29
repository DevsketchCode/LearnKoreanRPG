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
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();

            if (uiManager != null && uiManager.popupTranslationCanvas != null)
            {
                GOToInteractWith1 = uiManager.popupTranslationCanvas;
            }
        }
        public void OnButtonClicked() // Call this function when the button is clicked (set in Button's OnClick event in Inspector)
        {
            Debug.Log("Button Clicked");
            if (GOToInteractWith1 != null)
            {
                ShowHideGO(GOToInteractWith1, buttonAction1);
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
            UIJuice juice = go.GetComponentInChildren<UIJuice>();

            if (go.activeSelf && (btnAction == ButtonAction.ToggleShowHide || btnAction == ButtonAction.Hide))
            {
                if (juice != null)
                {
                    // Play the exit, then set active to false when done
                    juice.PlayExit(() => go.SetActive(false));
                }
                else
                {
                    go.SetActive(false);
                }
            }
            else if (!go.activeSelf && (btnAction == ButtonAction.Show))
            {
                go.SetActive(true);

                // If the object we just turned on has the Juice script, play it!
                if (juice != null)
                {
                    juice.PlayEntrance();
                }
            }
        }
    }
}
