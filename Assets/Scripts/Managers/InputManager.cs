using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    static Inputs inputs;

    public static event Action<Coord> OnMoveAttempt;

    private void Awake()
    {
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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    // PLAYER MAP
    // movement
    void MoveUp(InputAction.CallbackContext ctx)
    {
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCuurentPos(), Direction.DOWN)
        )
            LevelManager.GetPlayerRef().Move(Direction.UP);
    }

    void MoveRight(InputAction.CallbackContext ctx)
    {
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCuurentPos(), Direction.LEFT)
        )
            LevelManager.GetPlayerRef().Move(Direction.RIGHT);
    }

    void MoveDown(InputAction.CallbackContext ctx)
    {
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCuurentPos(), Direction.UP)
        )
            LevelManager.GetPlayerRef().Move(Direction.DOWN);
    }

    void MoveLeft(InputAction.CallbackContext ctx)
    {
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCuurentPos(), Direction.RIGHT)
        )
            LevelManager.GetPlayerRef().Move(Direction.LEFT);
    }

    // menus
    void OpenWebMenu(InputAction.CallbackContext ctx) { }

    void Pause(InputAction.CallbackContext ctx) { }

    // WEB MENU
    // cycle
    void CycleUp(InputAction.CallbackContext ctx) { }

    void CycleRight(InputAction.CallbackContext ctx) { }

    void CycleDown(InputAction.CallbackContext ctx) { }

    void CycleLeft(InputAction.CallbackContext ctx) { }

    void PerformWebAction(InputAction.CallbackContext ctx)
    {
        CloseWebMenu(ctx);
    }

    void CloseWebMenu(InputAction.CallbackContext ctx) { }

    public static void EnableMovementInputs() => inputs.Player.Enable();

    public static void DisableMovementInputs() => inputs.Player.Disable();

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
    }
}
