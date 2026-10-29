using UnityEngine;
using Unity.Serialization.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using System;

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
[RequireComponent(typeof(LevelManager))]
public class SaveManager : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime5 = new(5);
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

    [SerializeField]
    GameObject editorMaps,
        loadMaps;

    [SerializeField]
    TextMeshProUGUI errorText;

    [SerializeField]
    GameObject editorObjects;

    bool isOverwriting = false;

    // These action get fired when an interactable/support are erased in the level editor.
    // The point of them is to have the corresponding editor objects remove themselves

    public static event Action<Coord> RemoveInteractable;
    public static event Action<Coord> RemoveSupport;

    public bool autoIncrementId = true;
    public const int X_BOUND_LEFT = -5;
    public const int X_BOUND_RIGHT = 8;
    public const int Y_BOUND_TOP = 9;
    public const int Y_BOUND_BOTTOM = 0;

    // placement matrix is used to optimize the scan algorithm and prevent duplicate walkabales from being added into the level.
    int[,] placementMatrix;

    private void Awake()
    {
        JsonSerialization.AddGlobalAdapter(new BranchAdapter());
        JsonSerialization.AddGlobalAdapter(new TrunkAdapter());
        JsonSerialization.AddGlobalAdapter(new LevelAdapter());
        JsonSerialization.AddGlobalAdapter(new CoordAdapter());
        JsonSerialization.AddGlobalAdapter(new FruitAdapter());
        JsonSerialization.AddGlobalAdapter(new LevelEntryAdapater());
        // JsonSerialization.AddGlobalAdapter(new GameAdapter());
    }

    public void ToggleAutoIncrement() => autoIncrementId = !autoIncrementId;

    public void TestLevel()
    {
        try
        {
            SaveOrOverwriteLevel(-1);
        }
        catch (NonUniqueLevelElementException e)
        {
            errorText.text = e.Message;
            StartCoroutine(FlashErrorText());
            return;
        }
        editorObjects.SetActive(false);
        editorMaps.SetActive(false);
        loadMaps.SetActive(true);
        GameManager.SelectLevel(-1);
    }

    public void ContinueEditingLevel()
    {
        GetComponent<LevelManager>().ClearLevel();
        editorObjects.SetActive(true);
        loadMaps.SetActive(false);
        editorMaps.SetActive(true);
    }

    public void SaveOrOverwriteLevel(int id)
    {
        // 26-10-29: I'm reserving the first 10 level ID's for special things. Such as multiple world maps.
        if (id != -1 && autoIncrementId)
            id = Directory.GetFiles(levelPath).Length + 11;
        if (
            id != -1
            && File.Exists(new(Path.Combine(levelPath, id.ToString() + ".txt")))
            && !isOverwriting
        )
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

        bool hasSpawn = false;
        bool hasGoal = false;

        placementMatrix = new int[
            Math.Abs(X_BOUND_LEFT) + X_BOUND_RIGHT + 1,
            Math.Abs(Y_BOUND_BOTTOM) + Y_BOUND_TOP + 1
        ];

        // DEBUG:: view placement matrix (pre)
        // string pm = "";
        // for (int c = 0; c < placementMatrix.GetLength(1); c++)
        // {
        //     for (int r = 0; r < placementMatrix.GetLength(0); r++)
        //     {
        //         pm += placementMatrix[r, c] + ", ";
        //     }
        //     pm += "\n";
        // }
        // Debug.Log(placementMatrix.GetLength(0));
        // Debug.Log(placementMatrix.GetLength(1));
        // Debug.Log(pm);

        (initWeight, initWebCount, initLength, baseTrunkSupport) =
            GetComponent<LevelEditorManager>().GetInitialValues();

        // Scan left to right
        for (int x = X_BOUND_LEFT; x <= X_BOUND_RIGHT; x++)
        {
            //  Scan bottom to top
            for (int y = Y_BOUND_BOTTOM; y <= Y_BOUND_TOP; y++)
            {
                // Scan pattern is as follows:

                /*
                04  08  12  16
                03  07  11  15
                02  06  10  14
                01  05  09  13
                */

                // This scan pattern is important! It means that the first time a branch is enountered, the **leftmost**
                // branch segment will **always** be the first segment encountered!
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

                    // I've decided to use regex to scan for walkabales instead of a switch case.
                    // This is mainly for the branches. Since I have different branch types, a regex is more maintainable.
                    // Since I don't want to combine a switch with an IF stack (IF inside the default of the switch),
                    // trunks (and all walkables) will also be switched to using the regex to scan.
                    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference

                    if (
                        placementMatrix[x + Math.Abs(X_BOUND_LEFT), y + Math.Abs(Y_BOUND_BOTTOM)]
                        == 0
                    )
                    {
                        if (Regex.IsMatch(tile.name, "^trunk_s.*"))
                        {
                            placementMatrix[
                                x + Math.Abs(X_BOUND_LEFT),
                                y + Math.Abs(Y_BOUND_BOTTOM)
                            ] = 1;
                            BuildTrunk(loc);
                        }
                        else if (Regex.IsMatch(tile.name, "^branch_.*"))
                        {
                            placementMatrix[
                                x + Math.Abs(X_BOUND_LEFT),
                                y + Math.Abs(Y_BOUND_BOTTOM)
                            ] = 1;
                            BuildBranch(loc, tile.name);
                        }
                        else
                        {
                            Debug.LogWarning(
                                "Unknown walkable of name " + tile.name + " found at " + loc + "!"
                            );
                        }
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
                        case "spawn":
                            if (hasSpawn)
                                throw new NonUniqueLevelElementException("Too many spawn points!");
                            spawnPoint = new(x, y);
                            hasSpawn = true;
                            break;
                        case "goal":
                            if (hasGoal)
                                throw new NonUniqueLevelElementException("Too many goal points!");
                            goalPoint = new(x, y);
                            hasGoal = true;
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

        // DEBUG:: view placement matrix (post)
        // pm = "";
        // for (int c = placementMatrix.GetLength(1) - 1; c >= 0; c--)
        // {
        //     for (int r = 0; r < placementMatrix.GetLength(0); r++)
        //     {
        //         pm += placementMatrix[r, c] + ",\t";
        //     }
        //     pm += "\n";
        // }
        // Debug.Log(placementMatrix.GetLength(0));
        // Debug.Log(placementMatrix.GetLength(1));
        // Debug.Log(pm);

        if (!hasSpawn)
            throw new NonUniqueLevelElementException("spawn point does not exist!");
        if (!hasGoal)
            throw new NonUniqueLevelElementException("goal point does not exist!");

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
        int height = 0;
        // max height is 8
        for (int y = src.y; y <= Y_BOUND_TOP; y++)
        {
            if (walkablesMap_editor.HasTile(new Vector3Int(src.x, y, 0)))
            {
                if (
                    Regex.IsMatch(
                        walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name,
                        "^trunk"
                    )
                )
                {
                    height++;
                    placementMatrix[src.x + Math.Abs(X_BOUND_LEFT), y + Math.Abs(Y_BOUND_BOTTOM)] =
                        1;
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

    /// <summary>
    /// Build a branch
    /// </summary>
    /// <param name="src">The coordinate of the tile that is the source of the branch</param>
    /// <param name="branchName">The name of the branch. This will be parsed to determine the branch type</param>
    void BuildBranch(Vector3Int src, string branchName)
    {
        Coord coord = new(src.x, src.y);
        int length = 1;

        string regex = "^branch.*";
        for (int x = src.x + 1; x <= X_BOUND_RIGHT; x++)
        {
            if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
            {
                if (
                    Regex.IsMatch(
                        walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name,
                        regex
                    )
                )
                {
                    length++;
                    placementMatrix[x + Math.Abs(X_BOUND_LEFT), src.y + Math.Abs(Y_BOUND_BOTTOM)] =
                        1;
                    continue;
                }
            }
            break;
        }

        Debug.Log(
            "Building branch sourced at ("
                + coord.x
                + ", "
                + coord.y
                + ") with length "
                + length
                + "!"
        );
        new Branch(coord, length, false);
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

    IEnumerator FlashErrorText()
    {
        errorText.enabled = true;
        yield return _waitForSecondsRealtime5;
        errorText.enabled = false;
    }

    public static EditorElement GetEditorElementAtCoord(EditorIdentity ei, Coord crd)
    {
        if (editorElements.TryGetValue((ei, crd), out EditorElement ee))
            return ee;
        return null;
    }

    public static bool RemoveEditorElementAtCoord(EditorIdentity ei, Coord crd)
    {
        if (editorElements.Remove((ei, crd)))
        {
            // The editor elements (especially the support elements) check for null propagation.
            // Just because the entity exists, it does not mean that a corresponding GameObject exists!
            switch (ei)
            {
                case EditorIdentity.INTERACTABLE:
                    RemoveInteractable?.Invoke(crd);
                    break;
                case EditorIdentity.SUPPORT:
                    RemoveSupport?.Invoke(crd);
                    break;
            }
            return true;
        }
        return false;
    }
}
