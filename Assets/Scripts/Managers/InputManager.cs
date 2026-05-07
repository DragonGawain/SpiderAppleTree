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

        // DEBUG
#if UNITY_EDITOR
        inputs.Player.LogInfo1.performed += LogInfo1;
        inputs.Player.LogInfo2.performed += LogInfo2;
        inputs.Player.LogInfo3.performed += LogInfo3;
#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    // PLAYER MAP
    // movement
    void MoveUp(InputAction.CallbackContext ctx)
    {
        Debug.Log("Detected up!");
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCurentPos(), Direction.DOWN)
        )
        {
            Debug.Log("moving up");
            CompleteMove(Direction.UP);
        }
    }

    void MoveRight(InputAction.CallbackContext ctx)
    {
        Debug.Log("Detected right!");
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCurentPos(), Direction.LEFT)
        )
        {
            Debug.Log("moving right");
            CompleteMove(Direction.RIGHT);
        }
    }

    void MoveDown(InputAction.CallbackContext ctx)
    {
        Debug.Log("Detected down!");
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCurentPos(), Direction.UP)
        )
        {
            Debug.Log("moving down");
            CompleteMove(Direction.DOWN);
        }
    }

    void MoveLeft(InputAction.CallbackContext ctx)
    {
        Debug.Log("Detected left!");
        if (
            LevelManager
                .GetActiveLevel()
                .IsWalkable(LevelManager.GetPlayerRef().GetCurentPos(), Direction.RIGHT)
        )
        {
            Debug.Log("moving left");
            CompleteMove(Direction.LEFT);
        }
    }

    void CompleteMove(Direction dir)
    {
        LevelManager.GetPlayerRef().Move(dir);
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
