using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Houses GameState enum.
/// Enforces presence of all other manager scripts (RequireComponent).
/// Singleton pattern. DontDestroyOnLoad.
/// Handles game state, mid-processing of certain inputs, and any other miscellaneous task.
///
/// This should effectively be static.
/// All fields should be static if possible.
/// All methods should be static if possible.
/// </summary>
[RequireComponent(typeof(LevelManager))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(SaveManager))]
[RequireComponent(typeof(UIManager))]
public class GameManager : MonoBehaviour
{
    enum GameState
    {
        MAIN_MENU,
        PAUSED,
        WORLD_MAP,
        LEVEL
    }

    static GameManager singleton;

    // EVENTS
    public static event Action<int> OnLevelSelect;

    // TRACKER VARS
    static bool snapWarning = false;
    static Direction lastDir;

    private void Awake()
    {
        // singleton pattern
        if (singleton == null)
            singleton = this;
        if (singleton != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public static GameManager GetManagerSingleton() => singleton;

    public static void SelectLevel(int levelID)
    {
        OnLevelSelect.Invoke(levelID);
    }

    public static void OnLegalMove(Direction dir)
    {
        // TODO:: check if move will cause a branch to snap
        // This branch snap checking should only be based on player weight
        // (i.e. not calculate how much the player weight will change if eating a fruit,
        // and also not counting the change in weight of a fruit no longer being on the branch.)

        // This is really only valuable if I don't create an undo stack.
    }
}
