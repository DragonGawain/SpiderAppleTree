using UnityEngine;
using System;
using UnityEngine.InputSystem;

public enum InputMap
{
    PLAYER,
    WEB_MENU
}

/// <summary>
/// Handles reading inputs.
/// Does not handle processing of inputs.
///
/// Exposes methods to change the current active input action map.
/// All other maps are auto-disabled when a map enable is called.
/// </summary>
[RequireComponent(typeof(UIManager))]
[RequireComponent(typeof(LevelEditorManager))]
public class InputManager : MonoBehaviour
{
    // public static event Action<Coord> OnMoveAttempt;
    static Inputs inputs;

    static UIManager uiManagerRef;
    static LevelEditorManager levelEditorManagerRef;

    private void Awake()
    {
        uiManagerRef = GetComponent<UIManager>();
        levelEditorManagerRef = GetComponent<LevelEditorManager>();

        inputs = new Inputs();
        // global inputs should ALWAYS be enabled. It's stuff like opening the pause menu.
        inputs.Global.Enable();
        // inputs.Player.Enable();
        inputs.LevelEditor.Enable();

        // global
        inputs.Global.Pause.performed += Pause;

        // player
        inputs.Player.MoveUp.performed += MoveUp;
        inputs.Player.MoveRight.performed += MoveRight;
        inputs.Player.MoveDown.performed += MoveDown;
        inputs.Player.MoveLeft.performed += MoveLeft;
        inputs.Player.WebMenu.performed += OpenWebMenu;

        // web menu
        inputs.WebMenu.Up.performed += CycleUp;
        inputs.WebMenu.Right.performed += CycleRight;
        inputs.WebMenu.Down.performed += CycleDown;
        inputs.WebMenu.Left.performed += CycleLeft;
        inputs.WebMenu.Select.performed += PerformWebAction;
        inputs.WebMenu.Cancel.performed += CloseWebMenu;

        // level editor
        // this triggers on started so that it is where the mouse is when you click, as opposed to where the mouse is when you let go of the click
        inputs.LevelEditor.Click.started += LevelEditClick;

        // DEBUG
#if UNITY_EDITOR
        inputs.Player.LogInfo1.performed += LogInfo1;
        inputs.Player.LogInfo2.performed += LogInfo2;
        inputs.Player.LogInfo3.performed += LogInfo3;
#endif
    }

    #region global
    void Pause(InputAction.CallbackContext ctx) { }

    #endregion

    #region player map
    // movement
    void MoveUp(InputAction.CallbackContext ctx)
    {
        VerifyAndCompleteMove(Direction.UP);
    }

    void MoveRight(InputAction.CallbackContext ctx)
    {
        VerifyAndCompleteMove(Direction.RIGHT);
    }

    void MoveDown(InputAction.CallbackContext ctx)
    {
        VerifyAndCompleteMove(Direction.DOWN);
    }

    void MoveLeft(InputAction.CallbackContext ctx)
    {
        VerifyAndCompleteMove(Direction.LEFT);
    }

    void VerifyAndCompleteMove(Direction dir)
    {
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(
                    LevelManager.GetPlayerRef().GetCurentPos(),
                    (Direction)(((int)dir + 2) % 4)
                )
        )
        {
            LevelManager.GetPlayerRef().Move(dir);
        }
    }

    // menus
    void OpenWebMenu(InputAction.CallbackContext ctx)
    {
        uiManagerRef.OpenWebMenu();
    }
    #endregion



    #region web menu
    // cycle
    void CycleUp(InputAction.CallbackContext ctx)
    {
        uiManagerRef.SetWebMenuButton(uiManagerRef.GetSelectedButton().GetUpButton());
    }

    void CycleRight(InputAction.CallbackContext ctx)
    {
        uiManagerRef.SetWebMenuButton(uiManagerRef.GetSelectedButton().GetRightButton());
    }

    void CycleDown(InputAction.CallbackContext ctx)
    {
        uiManagerRef.SetWebMenuButton(uiManagerRef.GetSelectedButton().GetDownButton());
    }

    void CycleLeft(InputAction.CallbackContext ctx)
    {
        uiManagerRef.SetWebMenuButton(uiManagerRef.GetSelectedButton().GetLeftButton());
    }

    void PerformWebAction(InputAction.CallbackContext ctx)
    {
        uiManagerRef.SelectWebMenuOption();
        CloseWebMenu(ctx);
    }

    void CloseWebMenu(InputAction.CallbackContext ctx)
    {
        uiManagerRef.CloseWebMenu();
    }

    #endregion

    #region level editor
    void LevelEditClick(InputAction.CallbackContext ctx)
    {
        levelEditorManagerRef.OnClick();
    }
    #endregion

    public static void DisableAllInputs()
    {
        inputs.Player.Disable();
        inputs.WebMenu.Disable();
        inputs.LevelEditor.Disable();
    }

    public static void EnableInputMap(InputMap im)
    {
        DisableAllInputs();
        switch (im)
        {
            case InputMap.PLAYER:
                if (Player.Alive)
                    inputs.Player.Enable();
                break;
            case InputMap.WEB_MENU:
                inputs.WebMenu.Enable();
                break;
        }
    }

    public static void EnableMovementInputs()
    {
        DisableAllInputs();
        if (Player.Alive)
            inputs.Player.Enable();
    }

    public static void EnableWebMenuInputs()
    {
        DisableAllInputs();
        inputs.WebMenu.Enable();
    }

    public static void EnableEditorInputs()
    {
        DisableAllInputs();
        inputs.LevelEditor.Enable();
    }

    void LogInfo1(InputAction.CallbackContext ctx)
    {
        LevelManager.GetActiveLevel().AnalyzeUniqueElements();
    }

    void LogInfo2(InputAction.CallbackContext ctx) { }

    void LogInfo3(InputAction.CallbackContext ctx) { }

    void OnDestroy()
    {
        // global
        inputs.Global.Pause.performed -= Pause;

        // player
        inputs.Player.MoveUp.performed -= MoveUp;
        inputs.Player.MoveRight.performed -= MoveRight;
        inputs.Player.MoveDown.performed -= MoveDown;
        inputs.Player.MoveLeft.performed -= MoveLeft;
        inputs.Player.WebMenu.performed -= OpenWebMenu;

        // web menu
        inputs.WebMenu.Up.performed -= CycleUp;
        inputs.WebMenu.Right.performed -= CycleRight;
        inputs.WebMenu.Down.performed -= CycleDown;
        inputs.WebMenu.Left.performed -= CycleLeft;
        inputs.WebMenu.Select.performed -= PerformWebAction;
        inputs.WebMenu.Cancel.performed -= CloseWebMenu;

        // level editor
        inputs.LevelEditor.Click.started -= LevelEditClick;

        // DEBUG
#if UNITY_EDITOR
        inputs.Player.LogInfo1.performed -= LogInfo1;
        inputs.Player.LogInfo2.performed -= LogInfo2;
        inputs.Player.LogInfo3.performed -= LogInfo3;
#endif
    }
}
