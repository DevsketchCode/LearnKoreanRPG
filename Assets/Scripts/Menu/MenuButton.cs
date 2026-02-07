using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Menu
{
    public class MenuButton : MonoBehaviour
    {
        public void OnClick()
        {
            Debug.Log("ButtonClicked: " + this.name);
            string buttonName = this.name;

            switch (buttonName)
            {
                case "Button-StartGame":
                    HandleStartGame();
                    break;

                case "Button-Settings":
                    HandleSettings();
                    break;

                case "Button-MainMenu":
                    HandleMainMenu();
                    break;

                case "Button-Quit":
                    HandleQuit();
                    break;
            }
        }

        private void HandleStartGame()
        {
            if (GameManager.Instance != null)
            {
                string prefix = GameManager.Instance.currentLanguage + "_";
                GameManager.Instance.loadingFromMenu = true;
                string lastScene = PlayerPrefs.GetString(prefix + "LastScene", "Scene_Tutorial"); // Default if no save
                SceneManager.LoadScene(lastScene);
            }
        }

        private void HandleSettings()
        {
            // If we are already in Settings, don't do anything
            if (SceneManager.GetActiveScene().name == "_Settings") return;

            // If we are in-game, we might want to tell the GameManager to remember where we came from
            // so we can "Return to Game" later.
            SceneManager.LoadScene("_Settings");
        }

        private void HandleMainMenu()
        {
            // If we are in the Settings scene, we just go back to Main Menu
            // If we are in a Game Level, we save the player's state first!
            if (SceneManager.GetActiveScene().name == "_Settings")
            {
                SceneManager.LoadScene("_MainMenu");
            }
            else
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveAndGoToMainMenu();
                else
                    SceneManager.LoadScene("_MainMenu");
            }
        }

        private void HandleQuit()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.QuitGame();
            else
                Application.Quit();
        }
    }
}