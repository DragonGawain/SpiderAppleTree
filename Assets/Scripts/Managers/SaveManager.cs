using UnityEngine;
using Unity.Serialization.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

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

    public bool autoIncrementId = true;
    public const int X_BOUND_LEFT = -5;
    public const int X_BOUND_RIGHT = 8;
    public const int Y_BOUND_TOP = 9;
    public const int Y_BOUND_BOTTOM = 0;

    private void Awake()
    {
        JsonSerialization.AddGlobalAdapter(new BranchAdapter());
        JsonSerialization.AddGlobalAdapter(new TrunkAdapter());
        JsonSerialization.AddGlobalAdapter(new LevelAdapter());
        JsonSerialization.AddGlobalAdapter(new CoordAdapter());
        JsonSerialization.AddGlobalAdapter(new FruitAdapter());
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
        if (id != -1 && autoIncrementId)
            id = Directory.GetFiles(levelPath).Length + 1;
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
                        // case "branch_src_R":
                        // case "branch_s_R":
                        //     // branch src
                        //     // body: 8
                        //     // DIRECTION: RIGHT
                        //     BuildBranch(loc, false);
                        //     break;
                        // case "branch_src_L":
                        // case "branch_s_L":
                        //     // alt branch src
                        //     // body: 4
                        //     // DIRECTION: LEFT
                        //     BuildBranch(loc, true);
                        //     break;
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
        string branchRegex = "^branch.*";
        // max height is 8
        for (int y = src.y; y <= Y_BOUND_TOP; y++)
        {
            if (walkablesMap_editor.HasTile(new Vector3Int(src.x, y, 0)))
            {
                // if (walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name == "trunk_bdy")
                if (
                    Regex.IsMatch(
                        walkablesMap_editor.GetTile(new Vector3Int(src.x, y, 0)).name,
                        "^trunk"
                    )
                )
                {
                    height++;

                    // scan left/right of target trunk segment for branches
                    // scan left
                    if (walkablesMap_editor.HasTile(new Vector3Int(src.x - 1, y, 0)))
                        if (
                            !newLevel.GetWalkables().ContainsKey(new(src.x - 1, y))
                            && Regex.IsMatch(
                                walkablesMap_editor.GetTile(new Vector3Int(src.x - 1, y, 0)).name,
                                branchRegex
                            )
                        )
                            BuildBranch(new(src.x - 1, y, 0), true);

                    // scan right
                    if (walkablesMap_editor.HasTile(new Vector3Int(src.x + 1, y, 0)))
                        if (
                            !newLevel.GetWalkables().ContainsKey(new(src.x + 1, y))
                            && Regex.IsMatch(
                                walkablesMap_editor.GetTile(new Vector3Int(src.x + 1, y, 0)).name,
                                branchRegex
                            )
                        )
                            BuildBranch(new(src.x + 1, y, 0), false);
                    continue;
                }
            }
            break;
        }
        // after we reach the top of the tree, we scan the top
        if (walkablesMap_editor.HasTile(new Vector3Int(src.x, height + 1, 0)))
            if (
                !newLevel.GetWalkables().ContainsKey(new(src.x, height + 1))
                && Regex.IsMatch(
                    walkablesMap_editor.GetTile(new Vector3Int(src.x, height + 1, 0)).name,
                    branchRegex
                )
            )
            {
                int xSrc = src.x;
                if (
                    Regex.IsMatch(
                        walkablesMap_editor.GetTile(new Vector3Int(src.x, height + 1, 0)).name,
                        "alt$"
                    )
                )
                {
                    for (int x = src.x + 1; x <= X_BOUND_RIGHT; x++)
                    {
                        if (walkablesMap_editor.HasTile(new Vector3Int(x, height + 1, 0)))
                        {
                            if (
                                Regex.IsMatch(
                                    walkablesMap_editor
                                        .GetTile(new Vector3Int(x, height + 1, 0))
                                        .name,
                                    "^branch.*alt$"
                                )
                            )
                            {
                                xSrc = x;
                                continue;
                            }
                        }
                        break;
                    }
                    BuildBranch(new(xSrc, height + 1), true);
                }
                else
                {
                    for (int x = src.x - 1; x >= X_BOUND_LEFT; x--)
                    {
                        if (walkablesMap_editor.HasTile(new Vector3Int(x, height + 1, 0)))
                        {
                            if (
                                Regex.IsMatch(
                                    walkablesMap_editor
                                        .GetTile(new Vector3Int(x, height + 1, 0))
                                        .name,
                                    "^branch.*_$"
                                )
                            )
                            {
                                xSrc = x;
                                continue;
                            }
                        }
                        break;
                    }
                    BuildBranch(new(xSrc, height + 1), false);
                }
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
    /// <param name="alt">The direction the branch is facing. false => right, true => left</param>
    void BuildBranch(Vector3Int src, bool alt)
    {
        Coord coord = new(src.x, src.y);
        int length = 1;
        // RIGHT
        if (!alt)
        {
            string regex = "^branch.*_$";
            for (int x = src.x + 1; x <= X_BOUND_RIGHT; x++)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    // if (
                    //     walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                    //     == "branch_b_R"
                    // )
                    if (
                        Regex.IsMatch(
                            walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name,
                            regex
                        )
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
            string regex = "^branch.*alt$";
            for (int x = src.x - 1; x >= X_BOUND_LEFT; x--)
            {
                if (walkablesMap_editor.HasTile(new Vector3Int(x, src.y, 0)))
                {
                    // if (
                    //     walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name
                    //     == "branch_b_L"
                    // )
                    if (
                        Regex.IsMatch(
                            walkablesMap_editor.GetTile(new Vector3Int(x, src.y, 0)).name,
                            regex
                        )
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
}
