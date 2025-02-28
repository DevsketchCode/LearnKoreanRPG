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
        enum ButtonAction
        {
            None,
            Show,
            Hide,
            ToggleShowHide
        }

        [SerializeField]
        private GameObject GOToInteractWith; // Assign the GameObject

        [SerializeField]
        private ButtonAction buttonAction; // Assign the button action

        public void OnButtonClicked() // Call this function when the button is clicked (set in Button's OnClick event in Inspector)
        {
            Debug.Log("Button Clicked");
            ShowHideGO(GOToInteractWith, buttonAction);
        }

        private void ShowHideGO(GameObject go, ButtonAction btnAction)
        {
            if(go.activeSelf && (btnAction == ButtonAction.ToggleShowHide || btnAction == ButtonAction.Hide))
            {
                go.SetActive(false);
            }
            else if (!go.activeSelf && (btnAction == ButtonAction.Show))
            {
                go.SetActive(true);
            }
        }
    }
}
