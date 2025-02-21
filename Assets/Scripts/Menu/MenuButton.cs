using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Menu
{
    class MenuButton : MonoBehaviour
    {
        enum ButtonActions
        {
            New,
            Continue,
            MainMenu,
            StartGameScene, // Renamed from Load
            Save,
            Settings,
            Exit
        }

        private void Action(ButtonActions buttonAction, string sceneName) // Keep name as 'Action' if you intend to add more actions later
        {
            Debug.Log("Action Accessed: " + buttonAction.ToString() + " SCENE: " + sceneName);
            switch (buttonAction)
            {
                case ButtonActions.New: // Handle 'New' action (scene loading in this case)
                case ButtonActions.StartGameScene: // Also handle 'StartGameScene' (renamed from Load) the same way
                case ButtonActions.MainMenu: // Also handle 'MainMenu' the same way
                    SceneManager.LoadScene(sceneName);
                    break;
                case ButtonActions.Continue:
                    // Implement Continue game logic here
                    Debug.Log("Continue game action");
                    break;

                case ButtonActions.Save:
                    // Implement Save game logic here
                    Debug.Log("Save game action");
                    break;
                case ButtonActions.Settings:
                    // Implement Settings menu logic here
                    Debug.Log("Settings menu action");
                    break;
                case ButtonActions.Exit:
                    // Exit is already handled in QuitGame()
                    break;
                default:
                    Debug.LogWarning($"Unknown ButtonAction: {buttonAction}");
                    break;
            }
        }

        public void OnClick()
        {
            Debug.Log("ButtonClicked: " + this.name);
            string buttonName = this.name;

            switch (buttonName)
            {
                case "Button-NewGame":
                    this.Action(ButtonActions.New, "Town1"); // Now passing ButtonActions.New to 'Action' makes sense
                    break;
                case "Button-MainMenu":
                    this.Action(ButtonActions.MainMenu, "MainMenu");
                    break;
                case "Button-Continue":
                    this.Action(ButtonActions.Continue, ""); // No scene name needed for 'Continue' action (yet)
                    break;
                case "Button-Settings":
                    this.Action(ButtonActions.Settings, ""); // No scene name needed for 'Settings' (yet)
                    break;
                case "Button-Exit":
                    QuitGame();
                    break;
            }
        }

        public void QuitGame() // Public function that can be called by a button
        {
            Debug.Log("Quitting Application..."); // Optional: Debug log before quitting

#if UNITY_EDITOR
            // If running in the Unity Editor, stop play mode instead of quitting the application
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // In a standalone build, quit the application
            Application.Quit();
#endif
        }
    }
}
