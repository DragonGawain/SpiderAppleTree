using System;
using System.Collections.Generic;
using UnityEngine;

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

    // EVENTS
    public static event Action<int> OnLevelSelect;

    // TRACKER VARS

    // GENERAL (??? what does that even mean? "Stuff that doesn't fit anywhere else"?)
    GameState gameState = GameState.MAIN_MENU;

    static GameManager singleton;

    private void Awake()
    {
        // singleton pattern
        if (singleton == null)
            singleton = this;
        if (singleton != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void SelectLevel(int levelID)
    {
        OnLevelSelect.Invoke(levelID);
    }
}
