using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Menu
{
    class MenuButton : MonoBehaviour
    {
        enum ButtonActions
        {
            MainMenu,
            StartGameScene, // Renamed from Load
        }

        public void OnClick()
        {
            Debug.Log("ButtonClicked: " + this.name);
            string buttonName = this.name;

            switch (buttonName)
            {
                case "Button-StartGame": // The "Continue" or "Play" button
                    if (GameManager.Instance != null)
                    {
                        // We let GameManager handle the scene loading logic 
                        // because it knows the prefix and the LastScene
                        string prefix = GameManager.Instance.currentLanguage + "_";

                        // 1. Tell GameManager we are loading the absolute save
                        GameManager.Instance.loadingFromMenu = true;

                        string lastScene = PlayerPrefs.GetString(prefix + "LastScene");
                        SceneManager.LoadScene(lastScene);
                    }
                    break;

                case "Button-MainMenu":
                    if (GameManager.Instance != null)
                    {
                        // GameManager handles the snapshot save and the scene load
                        GameManager.Instance.SaveAndGoToMainMenu();
                    }
                    else
                    {
                        // Fallback: If for some reason GameManager is missing, just load the scene
                        SceneManager.LoadScene("_MainMenu");
                    }
                    break;

                case "Button-Quit":
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.QuitGame();
                    }
                    else
                    {
                        // Fallback for Title Screen if GameManager isn't awake yet
                        Application.Quit();
                    }
                    break;
            }
        }
    }
}
