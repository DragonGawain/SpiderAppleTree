using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Web menu")]
    [SerializeField]
    GameObject webMenu;

    [SerializeField]
    WebMenuButton defaultWebButton;

    WebMenuButton selectedWebButton;

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
        selectedWebButton.GetComponent<Image>().color = selectedWebButton
            .GetComponent<Button>()
            .colors.normalColor;
        webMenu.SetActive(false);
        InputManager.EnableMovementInputs();
    }
}
