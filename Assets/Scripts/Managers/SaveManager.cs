using UnityEngine;
using Unity.Serialization.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;

public enum LevelState
{
    FRESH,
    PARTIAL,
    SOLVED
}

public class SaveManager : MonoBehaviour
{
    public static readonly string levelPath = Path.Combine(@"..\..\..\SaveData", "Levels");
    public static readonly string solutionPath = Path.Combine(@"..\..\..\SaveData", "Solutions");
    public static readonly string partialPath = Path.Combine(@"..\..\..\SaveData", "Partials");

    public static readonly Dictionary<LevelState, string> levelStatePath =
        new()
        {
            { LevelState.FRESH, levelPath },
            { LevelState.PARTIAL, partialPath },
            { LevelState.SOLVED, solutionPath },
        };

    public Tilemap tilemap;
    public Grid grid;

    private void Awake()
    {
        JsonSerialization.AddGlobalAdapter(new BranchAdapter());
        JsonSerialization.AddGlobalAdapter(new TrunkAdapter());
        JsonSerialization.AddGlobalAdapter(new LevelAdapter());
        JsonSerialization.AddGlobalAdapter(new CoordAdapter());
        // JsonSerialization.AddGlobalAdapter(new GameAdapter());
    }

    public void SaveOrOverwriteLevel(int id)
    {
        try
        {
            using (StreamWriter sw = new(Path.Combine(levelPath, id.ToString())))
            {
                for (int x = -7; x <= 7; x++)
                {
                    for (int y = 0; y <= 8; y++)
                    {
                        //
                    }
                }
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("Something went wrong when WRITING the level with id " + id + "!");
            throw;
        }
    }
}

/*
Tilemap bounds (in my sample at least):
BL: (-7, 0)
BR: (7, 0)
TL: (-7, 8)
TR: (7, 8)
*/
