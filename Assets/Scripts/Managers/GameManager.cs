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
[RequireComponent(typeof(LevelEditorManager))]
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

    public static bool inWorldMap = false;

    // EVENTS
    public static event Action<int> OnLevelSelect;

    // TRACKER VARS
    // static bool snapWarning = false;
    // static Direction lastDir;

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

    /// <summary>
    /// Allows for console logging from non-monobehaviour classes
    /// </summary>
    /// <param name="msg">The message</param>
    /// <param name="logType">Log type. 0: std, 1: warn, 2: err</param>
    public static void DebugLog(string msg, LogType logType = LogType.STD)
    {
        switch (logType)
        {
            case LogType.STD:
                Debug.Log(msg);
                break;
            case LogType.WARN:
                Debug.LogWarning(msg);
                break;
            case LogType.ERROR:
                Debug.LogError(msg);
                break;
        }
    }

    public enum LogType
    {
        STD,
        WARN,
        ERROR
    }
}
