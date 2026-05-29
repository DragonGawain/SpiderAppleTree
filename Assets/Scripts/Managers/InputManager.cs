using UnityEngine;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// Handles reading inputs.
/// Does not handle processing of inputs.
///
/// Exposes methods to change the current active input action map.
/// All other maps are auto-disabled when a map enable is called.
/// </summary>
[RequireComponent(typeof(UIManager))]
public class InputManager : MonoBehaviour
{
    // public static event Action<Coord> OnMoveAttempt;
    static Inputs inputs;

    static UIManager uiManagerRef;

    private void Awake()
    {
        uiManagerRef = GetComponent<UIManager>();

        inputs = new Inputs();
        inputs.Player.Enable();
        inputs.Player.MoveUp.performed += MoveUp;
        inputs.Player.MoveRight.performed += MoveRight;
        inputs.Player.MoveDown.performed += MoveDown;
        inputs.Player.MoveLeft.performed += MoveLeft;
        inputs.Player.WebMenu.performed += OpenWebMenu;
        inputs.Player.Pause.performed += Pause;

        inputs.WebMenu.Up.performed += CycleUp;
        inputs.WebMenu.Right.performed += CycleRight;
        inputs.WebMenu.Down.performed += CycleDown;
        inputs.WebMenu.Left.performed += CycleLeft;
        inputs.WebMenu.Select.performed += PerformWebAction;
        inputs.WebMenu.Cancel.performed += CloseWebMenu;

        // DEBUG
#if UNITY_EDITOR
        inputs.Player.LogInfo1.performed += LogInfo1;
        inputs.Player.LogInfo2.performed += LogInfo2;
        inputs.Player.LogInfo3.performed += LogInfo3;
#endif
    }

    // PLAYER MAP
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

    void Pause(InputAction.CallbackContext ctx) { }

    // WEB MENU
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

    public static void DisableAllInputs()
    {
        inputs.Player.Disable();
        inputs.WebMenu.Disable();
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

    void LogInfo1(InputAction.CallbackContext ctx)
    {
        LevelManager.GetActiveLevel().AnalyzeUniqueElements();
    }

    void LogInfo2(InputAction.CallbackContext ctx) { }

    void LogInfo3(InputAction.CallbackContext ctx) { }

    void OnDestroy()
    {
        inputs.Player.MoveUp.performed -= MoveUp;
        inputs.Player.MoveRight.performed -= MoveRight;
        inputs.Player.MoveDown.performed -= MoveDown;
        inputs.Player.MoveLeft.performed -= MoveLeft;
        inputs.Player.WebMenu.performed -= OpenWebMenu;
        inputs.Player.Pause.performed -= Pause;

        inputs.WebMenu.Up.performed -= CycleUp;
        inputs.WebMenu.Right.performed -= CycleRight;
        inputs.WebMenu.Down.performed -= CycleDown;
        inputs.WebMenu.Left.performed -= CycleLeft;
        inputs.WebMenu.Select.performed -= PerformWebAction;
        inputs.WebMenu.Cancel.performed -= CloseWebMenu;

        // DEBUG
#if UNITY_EDITOR
        inputs.Player.LogInfo1.performed -= LogInfo1;
        inputs.Player.LogInfo2.performed -= LogInfo2;
        inputs.Player.LogInfo3.performed -= LogInfo3;
#endif
    }
}
