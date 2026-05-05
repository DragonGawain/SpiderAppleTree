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
            creatingLevel = true;
            newLevel = new Level(id);
            TileBase tile;
            for (int x = -7; x <= 7; x++)
            {
                for (int y = 0; y <= 8; y++)
                {
                    if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                        Debug.Log(
                            "Found tile at (" + x + ", " + y + "), with identity " + tile.name + "!"
                        );
                        switch (tile.name)
                        {
                            case "squares_2":
                                // trunk src
                                // body: 5
                                BuildTrunk(new Vector3Int(x, y, 0));
                                break;
                            case "squares_6":
                                // branch src
                                // body: 8
                                // DIRECTION: RIGHT
                                BuildBranch(new Vector3Int(x, y, 0), false);
                                break;
                            case "squares_1":
                                // alt branch src
                                // body: 4
                                // DIRECTION: LEFT
                                BuildBranch(new Vector3Int(x, y, 0), true);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            Debug.Log("<color=blue>Finished building level object!</color>");
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
            if (tilemap.HasTile(new Vector3Int(src.x, y, 0)))
            {
                if (tilemap.GetTile(new Vector3Int(src.x, y, 0)).name == "squares_5")
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
                if (tilemap.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    if (tilemap.GetTile(new Vector3Int(x, src.y, 0)).name == "squares_8")
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
                if (tilemap.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    if (tilemap.GetTile(new Vector3Int(x, src.y, 0)).name == "squares_4")
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
