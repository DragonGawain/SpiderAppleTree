using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI related tasks, such as:
/// * Showing/hiding menus (web menu, main menu, pause menu, etc)
/// * Updating the HUD (web count, weight, etc)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Web menu")]
    [SerializeField]
    GameObject webMenu;

    [SerializeField]
    WebMenuButton defaultWebButton;
    public WebMenuButton selectedWebButton;

    [Header("MainMenu")]
    [SerializeField]
    GameObject mainMenuGroup;

    [SerializeField]
    GameObject settingsGroup;

    [SerializeField]
    GameObject creditsGroup;

    [SerializeField]
    GameObject mainMenuBG;

    [Header("Tilemaps")]
    [SerializeField]
    GameObject loadMaps;

    [SerializeField]
    GameObject editorMaps;

    #region Web menu
    public void SetWebMenuButton(WebMenuButton webButton)
    {
        // selectedWebButton.gameObject.GetComponentInChildren<TMP
        selectedWebButton.GetComponent<Image>().color = selectedWebButton
            .GetComponent<Button>()
            .colors.normalColor;
        selectedWebButton = webButton;
        selectedWebButton.GetComponent<Image>().color = selectedWebButton
            .GetComponent<Button>()
            .colors.selectedColor;
    }

    public void SelectWebMenuOption()
    {
        selectedWebButton.OnClick();
        CloseWebMenu();
    }

    public WebMenuButton GetSelectedButton() => selectedWebButton;

    public void OpenWebMenu()
    {
        webMenu.SetActive(true);
        selectedWebButton = defaultWebButton;
        SetWebMenuButton(defaultWebButton);
        InputManager.EnableWebMenuInputs();
    }

    public void CloseWebMenu()
    {
        webMenu.SetActive(false);
        selectedWebButton.GetComponent<Image>().color = selectedWebButton
            .GetComponent<Button>()
            .colors.normalColor;
        InputManager.EnableMovementInputs();
    }
    #endregion

    #region Main menu

    public void DisableAllGroups()
    {
        mainMenuGroup.SetActive(false);
        settingsGroup.SetActive(false);
        creditsGroup.SetActive(false);
    }

    public void OpenMainMenu()
    {
        DisableAllGroups();
        mainMenuBG.SetActive(true);
        mainMenuGroup.SetActive(true);
    }

    public void OpenSettings()
    {
        DisableAllGroups();
        settingsGroup.SetActive(true);
    }

    public void OpenCredits()
    {
        DisableAllGroups();
        mainMenuBG.SetActive(true);
        creditsGroup.SetActive(true);
    }

    public void GoToLevelEditor() { }

    public void PlayGame()
    {
        DisableAllGroups();
        mainMenuBG.SetActive(false);
        loadMaps.SetActive(true);
        editorMaps.SetActive(false);
        GameManager.SelectLevel(0);
    }

    // This menu is for the in-game pause menu. It should have:
    // Continue (unpause)
    // Access to settings (no menu BG)
    // Return to world map (exit level)
    // Return to main menu
    public void OpenPauseMenu() { }

    #endregion
}
