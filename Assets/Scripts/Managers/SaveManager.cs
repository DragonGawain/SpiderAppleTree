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

/// <summary>
/// Houses LevelState enum.
///
/// Handles reading and writing of files.
/// Relevant mainly in the creation/editing and loading of level files.
/// All adapter registrations happen in this script.
/// </summary>
[RequireComponent(typeof(LevelEditorManager))]
public class SaveManager : MonoBehaviour
{
    public static readonly string levelPath = Path.Combine(@"SaveData", "Levels");
    public static readonly string solutionPath = Path.Combine(@"SaveData", "Solutions");
    public static readonly string partialPath = Path.Combine(@"SaveData", "Partials");

    static bool creatingLevel = false;

    public static bool GetCreatingLevel() => creatingLevel;

    static Level newLevel;

    public static Level GetNewLevel() => newLevel;

    static Dictionary<(EditorIdentity, Coord), EditorElement> editorElements = new();

    int initWebCount,
        initWeight,
        initLength,
        baseTrunkSupport;

    public static readonly Dictionary<LevelState, string> levelStatePath =
        new()
        {
            { LevelState.FRESH, levelPath },
            { LevelState.PARTIAL, partialPath },
            { LevelState.SOLVED, solutionPath },
        };

    public Tilemap walkablesMap_editor;
    public Tilemap interactablesMap_editor;
    public Tilemap supportsMap_editor;

    bool isOverwriting = false;

    const int X_BOUND_LEFT = -5;
    const int X_BOUND_RIGHT = 8;
    const int Y_BOUND_TOP = 9;
    const int Y_BOUND_BOTTOM = 0;

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
        if (File.Exists(new(Path.Combine(levelPath, id.ToString() + ".txt"))) && !isOverwriting)
        {
            Debug.Log(
                "<color=red>A level with this id already exists! Please click the \"save level\" button again to confirm overwriting.</color>"
            );
            isOverwriting = true;
            return;
        }
        isOverwriting = false;
        creatingLevel = true;
        newLevel = new Level(id);
        Coord spawnPoint = new(0, 0);
        Coord goalPoint = new(0, 0);
        TileBase tile;
        Vector3Int loc;
        EditorElement editorElement;

        (initWeight, initWebCount, initLength, baseTrunkSupport) =
            GetComponent<LevelEditorManager>().GetInitialValues();

        for (int x = X_BOUND_LEFT; x <= X_BOUND_RIGHT; x++)
        {
            for (int y = Y_BOUND_BOTTOM; y <= Y_BOUND_TOP; y++)
            {
                loc = new Vector3Int(x, y, 0);
                // The order in which the maps are scanned is important!
                // In particular, the walkables map MUST be scanned before the supports map
                if (walkablesMap_editor.HasTile(loc))
                {
                    tile = walkablesMap_editor.GetTile(loc);
                    Debug.Log(
                        "WALKABLE: Found walkable at ("
                            + x
                            + ", "
                            + y
                            + "), with identity "
                            + tile.name
                            + "!"
                    );
                    switch (tile.name)
                    {
                        case "trunk_src":
                        case "trunk_s":
                            // trunk src
                            // body: 5
                            BuildTrunk(loc);
                            break;
                        case "branch_src_R":
                        case "branch_s_R":
                            // branch src
                            // body: 8
                            // DIRECTION: RIGHT
                            BuildBranch(loc, false);
                            break;
                        case "branch_src_L":
                        case "branch_s_L":
                            // alt branch src
                            // body: 4
                            // DIRECTION: LEFT
                            BuildBranch(loc, true);
                            break;
                        // case "web_string":
                        //     break;
                        // case "web_support":
                        //     break;
                        default:
                            Debug.LogWarning(
                                "Unknown walkable of name " + tile.name + " found at " + loc + "!"
                            );
                            break;
                    }
                }
                if (interactablesMap_editor.HasTile(loc))
                {
                    tile = interactablesMap_editor.GetTile(loc);
                    Debug.Log(
                        "INTERACTABLE: Found element at ("
                            + x
                            + ", "
                            + y
                            + "), with identity "
                            + tile.name
                            + "!"
                    );

                    switch (tile.name)
                    {
                        case "spawn_point":
                            spawnPoint = new(x, y);
                            break;
                        case "goal":
                            goalPoint = new(x, y);
                            break;
                        case "fruit":
                        case "fruit_":
                        case "invis_fruit":
                            editorElement =
                                editorElements[(EditorIdentity.INTERACTABLE, new(x, y))]
                                as FruitEditor;
                            new Fruit(
                                new(x, y),
                                ((FruitEditor)editorElement).weight,
                                ((FruitEditor)editorElement).deltaWeight,
                                ((FruitEditor)editorElement).deltaWeb,
                                ((FruitEditor)editorElement).needed
                            );
                            break;
                        default:
                            Debug.LogWarning(
                                "Unknown interactable of name "
                                    + tile.name
                                    + " found at "
                                    + loc
                                    + "!"
                            );
                            break;
                    }
                }
                if (supportsMap_editor.HasTile(loc))
                {
                    tile = supportsMap_editor.GetTile(loc);
                    Debug.Log(
                        "SUPPORT: Found element at ("
                            + x
                            + ", "
                            + y
                            + "), with identity "
                            + tile.name
                            + "!"
                    );

                    switch (tile.name)
                    {
                        // case "hidden_support":
                        // trunk support left as a sample case
                        // case "trunk_support":
                        //     editorElement = editorElements[(EditorIdentity.SUPPORT, new(x, y))];
                        //     new TrunkSupport(
                        //         new(x, y),
                        //         ((TrunkSupportEditor)editorElement).supportValue
                        //     );
                        //     break;
                        default:
                            Debug.LogWarning(
                                "Unknown support of name " + tile.name + " found at " + loc + "!"
                            );
                            break;
                    }
                }
            }
        }
        newLevel.InitializeData(
            spawnPoint,
            goalPoint,
            initWebCount,
            initWeight,
            initLength,
            baseTrunkSupport
        );
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
        for (int y = src.y + 1; y <= Y_BOUND_TOP; y++)
        {
            if (walkablesMap_editor.HasTile(new Vector3Int(src.x, y, 0)))
            {
                // if (walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name == "trunk_bdy")
                if (walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name == "trunk_b")
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
            for (int x = src.x + 1; x <= X_BOUND_RIGHT; x++)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    // if (
                    //     walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                    //     == "branch_bdy_R"
                    // )
                    if (
                        walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                        == "branch_b_R"
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
            for (int x = src.x - 1; x >= X_BOUND_LEFT; x--)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    // if (
                    //     walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                    //     == "branch_bdy_L"
                    // )
                    if (
                        walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                        == "branch_b_L"
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
        new Branch(coord, length, alt ? Direction.LEFT : Direction.RIGHT, false);
    }

    public void ClearEditor()
    {
        editorElements.Clear();
        walkablesMap_editor.ClearAllTiles();
        interactablesMap_editor.ClearAllTiles();
        supportsMap_editor.ClearAllTiles();
    }

    public static void RegisterEditorElement(EditorIdentity ei, Coord crd, EditorElement ee)
    {
        editorElements.Add((ei, crd), ee);
    }
}
