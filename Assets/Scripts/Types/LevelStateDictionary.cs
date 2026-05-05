using System.Collections.Generic;

public class LevelStateDictionary
{
    public static Dictionary<int, LevelState> levelStates =
        new()
        {
            { 000, LevelState.FRESH },
            { 001, LevelState.FRESH },
            { 002, LevelState.FRESH },
            { 003, LevelState.FRESH },
            { 004, LevelState.FRESH },
            { 005, LevelState.FRESH },
            { 006, LevelState.FRESH },
            { 007, LevelState.FRESH },
            { 008, LevelState.FRESH },
            { 009, LevelState.FRESH },
        };

    public static void UpdateLevelState(int levelID, LevelState ls) => levelStates[levelID] = ls;
}
