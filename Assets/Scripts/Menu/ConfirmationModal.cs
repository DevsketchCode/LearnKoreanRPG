using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ConfirmationModal : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public Image confirmButtonImage;
    public TMP_Text confirmButtonText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Default Theme")]
    [Tooltip("Default: White Background, Black Text")]
    public bool useCustomColors = false;
    public bool isAlert = false;
    public Color customAlertBgColor = Color.firebrick;
    public Color customAlertTxtColor = Color.lemonChiffon;

    public Color customBtnBgColor = Color.white;
    public Color customBtnTxtColor = Color.black;

    private Color defaultBgColor = Color.white;
    private Color defaultTxtColor = Color.black;

    // We use an Action (callback) so the modal doesn't need to know 
    // WHICH script called it or WHAT that script is doing.
    private Action onConfirmCallback;


    // call the default colors by using: confirmationModal.Show("Save your progress?", () => SaveGame());

    public void Show(string message, Action onConfirm, bool isAlert)
    {
        titleText.text = message;
        onConfirmCallback = onConfirm;

        // Determine which colors to use: Custom or Inspector Defaults
        // If custom is null, use the default color from the inspector
        Color finalBg = (useCustomColors)? ((isAlert)? customAlertBgColor : customBtnBgColor) : defaultBgColor;
        Color finalText = (useCustomColors)? ((isAlert)? customAlertTxtColor : customBtnTxtColor) : defaultTxtColor;

        ApplySkin(finalBg, finalText);

        // Reset the button listeners so they don't stack up
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // Add the new listeners
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(Hide);

        gameObject.SetActive(true);
    }

    private void HandleConfirm()
    {
        onConfirmCallback?.Invoke(); // Execute the logic passed from the other script
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ApplySkin(Color bgColor, Color textColor)
    {
        confirmButtonImage.color = bgColor;
        confirmButtonText.color = textColor;
    }
}