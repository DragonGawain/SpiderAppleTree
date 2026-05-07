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
    public static readonly string levelPath = Path.Combine(@"SaveData", "Levels");
    public static readonly string solutionPath = Path.Combine(@"SaveData", "Solutions");
    public static readonly string partialPath = Path.Combine(@"SaveData", "Partials");

    static bool creatingLevel = false;

    public static bool GetCreatingLevel() => creatingLevel;

    static Level newLevel;

    public static Level GetNewLevel() => newLevel;

    [Header("Initial values")]
    [SerializeField]
    int initWebCount,
        initWeight,
        initLength;

    public static readonly Dictionary<LevelState, string> levelStatePath =
        new()
        {
            { LevelState.FRESH, levelPath },
            { LevelState.PARTIAL, partialPath },
            { LevelState.SOLVED, solutionPath },
        };

    public Tilemap walkablesMap_editor;
    public Tilemap interactablesMap_editor;

    private void Awake()
    {
        JsonSerialization.AddGlobalAdapter(new BranchAdapter());
        JsonSerialization.AddGlobalAdapter(new TrunkAdapter());
        JsonSerialization.AddGlobalAdapter(new LevelAdapter());
        JsonSerialization.AddGlobalAdapter(new CoordAdapter());
        JsonSerialization.AddGlobalAdapter(new FruitAdapter());
        // JsonSerialization.AddGlobalAdapter(new GameAdapter());
    }

    public void SaveOrOverwriteLevel(int id)
    {
        creatingLevel = true;
        newLevel = new Level(id);
        Coord spawnPoint = new(0, 0);
        Coord goalPoint = new(0, 0);
        TileBase tile;
        Vector3Int loc;
        for (int x = -7; x <= 7; x++)
        {
            for (int y = 0; y <= 8; y++)
            {
                loc = new Vector3Int(x, y, 0);
                if (walkablesMap_editor.HasTile(loc))
                {
                    tile = walkablesMap_editor.GetTile(loc);
                    Debug.Log(
                        "Found walkable at (" + x + ", " + y + "), with identity " + tile.name + "!"
                    );
                    switch (tile.name)
                    {
                        case "trunk_src":
                            // trunk src
                            // body: 5
                            BuildTrunk(loc);
                            break;
                        case "branch_src_R":
                            // branch src
                            // body: 8
                            // DIRECTION: RIGHT
                            BuildBranch(loc, false);
                            break;
                        case "branch_src_L":
                            // alt branch src
                            // body: 4
                            // DIRECTION: LEFT
                            BuildBranch(loc, true);
                            break;
                        case "web_string":
                            break;
                        case "web_support":
                            break;
                    }
                }
                if (interactablesMap_editor.HasTile(loc))
                {
                    tile = interactablesMap_editor.GetTile(loc);
                    Debug.Log(
                        "Found element at (" + x + ", " + y + "), with identity " + tile.name + "!"
                    );

                    switch (tile.name)
                    {
                        case "spawn":
                            spawnPoint = new(x, y);
                            break;
                        case "goal":
                            goalPoint = new(x, y);
                            break;
                        case "empty_fruit":
                            new Fruit(new(x, y), FruitType.EMPTY);
                            break;
                        case "web_fruit":
                            new Fruit(new(x, y), FruitType.WEB);
                            break;
                        case "anti_web_fruit":
                            new Fruit(new(x, y), FruitType.ANTI_WEB);
                            break;
                        case "light_fruit":
                            new Fruit(new(x, y), FruitType.LIGHT);
                            break;
                        case "heavy_fruit":
                            new Fruit(new(x, y), FruitType.HEAVY);
                            break;
                    }
                }
            }
        }
        newLevel.InitializeData(spawnPoint, goalPoint, initWebCount, initWeight, initLength);
        Debug.Log("<color=blue>Finished building level object!</color>");
        try
        {
            using (StreamWriter sw = new(Path.Combine(levelPath, id.ToString() + ".txt")))
            {
                sw.WriteLine(JsonSerialization.ToJson(newLevel));
            }
            Debug.Log("Successfully wrote new level to file!");
        }
        catch (System.Exception)
        {
            Debug.LogError("Something went wrong when WRITING the level with id " + id + "!");
            throw;
        }
        finally
        {
            creatingLevel = false;
        }
    }

    void BuildTrunk(Vector3Int src)
    {
        Coord coord = new(src.x, src.y);
        int height = 1;
        // max height is 8
        for (int y = src.y + 1; y <= 8; y++)
        {
            if (walkablesMap_editor.HasTile(new Vector3Int(src.x, y, 0)))
            {
                if (walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name == "trunk_bdy")
                {
                    height++;
                    continue;
                }
            }
            break;
        }
        Debug.Log(
            "Building trunk sourced at ("
                + coord.x
                + ", "
                + coord.y
                + ") with height "
                + height
                + "!"
        );
        new Trunk(coord, height);
    }

    void BuildBranch(Vector3Int src, bool alt)
    {
        Coord coord = new(src.x, src.y);
        int length = 1;
        // RIGHT
        if (!alt)
        {
            for (int x = src.x + 1; x <= 7; x++)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    if (
                        walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                        == "branch_bdy_R"
                    )
                    {
                        length++;
                        continue;
                    }
                }
                break;
            }
        }
        // LEFT
        else
        {
            for (int x = src.x - 1; x >= -7; x--)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    if (
                        walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                        == "branch_bdy_L"
                    )
                    {
                        length++;
                        continue;
                    }
                }
                break;
            }
        }

        Debug.Log(
            "Building branch sourced at ("
                + coord.x
                + ", "
                + coord.y
                + ") with length "
                + length
                + " facing "
                + (alt ? "left" : "right")
                + "!"
        );
        new Branch(coord, length, alt ? Direction.LEFT : Direction.RIGHT, 2);
    }
}

/*
Tilemap bounds (in my sample at least):
BL: (-7, 0)
BR: (7, 0)
TL: (-7, 8)
TR: (7, 8)
*/
