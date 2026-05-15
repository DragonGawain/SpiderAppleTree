using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Houses Coord struct.
///
/// Handles level related elements, including:
/// * Populating tilemaps (level data)
/// * Updating tilemaps (weight, webs, etc)
///
/// Exposes:
/// * playerRef
/// * activeLevel
/// * grid
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    Grid grid;
    public Tilemap walkablesMap;
    public Tilemap interactablesMap;
    public Tilemap numbersMap;

    static Grid gridRef;

    public Tile t_src,
        t_bdy,
        b_src_R,
        b_bdy_R,
        b_src_L,
        b_bdy_L,
        goal,
        empty_fruit,
        web_fruit,
        anti_web_fruit,
        light_fruit,
        heavy_fruit;

    [SerializeField]
    Grid numberPalette;

    Tile[] numberTiles;

    [SerializeField]
    GameObject playerPrefab;

    static Level activeLevel;
    static Player playerRef;

    static bool loadingLevel = false;

    public static bool GetLoadingLevel() => loadingLevel;

    void Awake()
    {
        gridRef = grid;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnLevelSelect += SetActiveLevel;

        // Get all the number tiles from the tile palette
        // I'm doing it this way to avoid needing to have MANY Tiles that I need to manually place in as SerializeFields.
        numberTiles = new Tile[19];
        Tilemap map = numberPalette.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = map.cellBounds;
        TileBase[] tiles = map.GetTilesBlock(bounds);
        int index;
        // 4 is the number of elements in a horizontal row in the tile palette
        // 17 is the total number of tiles minus 2
        // It is important that there are no empty tile in the palette!
        // Trailing null tiles in the bottom right corner are fine.
        for (int i = 0; i < 19; i += 4)
        {
            index = 17 - (i / 2);
            numberTiles[index] = (Tile)tiles[i];
            numberTiles[++index] = (Tile)tiles[i + 2];
            index -= 10;
            numberTiles[index] = (Tile)tiles[i + 1];
            numberTiles[++index] = (Tile)tiles[i + 3];
        }

        // Whole numbers 1-9 (index 0-8)
        // Half numbers 0.5-9.5 (index 9-19)
    }

    void SetActiveLevel(int levelID)
    {
        loadingLevel = true;
        // Grab level file
        // Deserialize level file
        activeLevel = new Level(levelID);
        try
        {
            using (
                StreamReader sr =
                    new(
                        Path.Combine(
                            SaveManager.levelStatePath[LevelStateDictionary.levelStates[levelID]],
                            levelID.ToString() + ".txt"
                        )
                    )
            )
            {
                string json = sr.ReadToEnd();
                JsonSerialization.FromJson<Level>(json);
            }
            Debug.Log(
                "<color=blue>Successfully finished reading level JSON file! (And therefore the level object has also been created)</color>"
            );
        }
        catch (System.Exception)
        {
            Debug.LogError("Something went wrong when READING the level with id " + levelID + "!");
            throw;
        }

        // Trunk t;
        // Branch b;
        // Fruit f;
        // Tile tile;
        // int nbIndex;
        // float support;
        IEnumerable<IGrouping<Type, ILevelElement>> levelElementsGroups = activeLevel
            .GetLevelElements()
            .GroupBy(e => e.GetType());

        List<Trunk> trunks = levelElementsGroups
            .First(g => g.Key == typeof(Trunk))
            .Cast<Trunk>()
            .ToList();
        List<Branch> branches = levelElementsGroups
            .First(g => g.Key == typeof(Branch))
            .Cast<Branch>()
            .ToList();
        List<Fruit> fruits = levelElementsGroups
            .First(g => g.Key == typeof(Fruit))
            .Cast<Fruit>()
            .ToList();
        // foreach (var group in levelElementsGroups)
        // {
        // if (group.Key == typeof(Trunk))
        // {


        foreach (Trunk t in trunks)
        {
            walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y), t_src);
            for (int y = 1; y < t.height; y++)
            {
                walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y + y), t_bdy);
            }
        }
        // }
        // else if (group.Key == typeof(Branch))
        // {
        foreach (Branch b in branches)
        {
            walkablesMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), b_src_R);
            if (b.direction == Direction.RIGHT)
            {
                // HACK:: this will not always be true. I should be doing the support calc for the src as well.
                // (This implies that some levels will start with pre-place supports/weights)
                // numbersMap.SetTile(
                //     new Vector3Int(b.coord.x, b.coord.y),
                //     numberTiles[Branch.MAX_BRANCH_SUPPORT - 1]
                // );
                // TODO:: half numbers
                // if (val % 1 != 0) => true if decimal exists
                for (int x = 1; x < b.length; x++)
                {
                    walkablesMap.SetTile(new Vector3Int(b.coord.x + x, b.coord.y), b_bdy_R);

                    // support = b.weightDeltas[x];
                    // nbIndex = Mathf.FloorToInt(support);
                    // // half numbers:
                    // // if (nbIndex == support) => T: whole number, F: has a decimal
                    // numbersMap.SetTile(
                    //     new Vector3Int(b.coord.x + x, b.coord.y),
                    //     numberTiles[nbIndex - 1]
                    // );
                }
            }
            else
            {
                walkablesMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), b_src_L);
                for (int x = 1; x < b.length; x++)
                {
                    walkablesMap.SetTile(new Vector3Int(b.coord.x - x, b.coord.y), b_bdy_L);

                    // support = b.weightDeltas[x];
                    // nbIndex = Mathf.FloorToInt(support);
                    // numbersMap.SetTile(
                    //     new Vector3Int(b.coord.x - x, b.coord.y),
                    //     numberTiles[nbIndex - 1]
                    // );
                }
            }
            // b.UpdateWeightMap();
        }
        // }
        // else if (group.Key == typeof(Fruit))
        // {
        foreach (Fruit f in fruits)
        {
            // tile =  f.fruitType switch
            // {
            //     FruitType.EMPTY => empty_fruit,
            //     FruitType.WEB => web_fruit,
            //     FruitType.ANTI_WEB => anti_web_fruit,
            //     FruitType.LIGHT => light_fruit,
            //     FruitType.HEAVY => heavy_fruit,
            //     _ => throw Exception("ERROR: Unknown FruitType supplied when loading level")
            // };

            interactablesMap.SetTile(
                f.coord.ToVector3Int(),
                f.fruitType switch
                {
                    FruitType.EMPTY => empty_fruit,
                    FruitType.WEB => web_fruit,
                    FruitType.ANTI_WEB => anti_web_fruit,
                    FruitType.LIGHT => light_fruit,
                    FruitType.HEAVY => heavy_fruit,
                    _ => throw new Exception("ERROR: Unknown FruitType supplied when loading level")
                }
            );
        }
        // }
        // }

        loadingLevel = false;
        // List<Branch> branches = activeLevel
        //     .GetLevelElements()
        //     .Where(e => e.GetType() == typeof(Branch))
        //     .Cast<Branch>()
        //     .ToList();
        foreach (Branch br in branches)
            br.RecalculateSupports();

        Debug.Log("Finished drawing loaded level!");

        // Create player at appropriate spot
        playerRef = Instantiate(
                playerPrefab,
                grid.CellToWorld(activeLevel.GetSpawnPoint().ToVector3Int())
                    + new Vector3(0.5f, 0.5f, 0),
                Quaternion.identity
            )
            .GetComponent<Player>();

        playerRef.Initialize(activeLevel.GetInitialLevelDataContainer(), grid);
        activeLevel.RefreshWalkablesDirections();
    }

    public void UpdateWeightMap(Coord coord, float value)
    {
        int nbToIndex = Mathf.FloorToInt(value) - 1;
        if (nbToIndex % 1 != 0)
            nbToIndex += 9;

        Debug.Log("Placing number of value " + value + " found at index " + nbToIndex);
        numbersMap.SetTile(coord.ToVector3Int(), numberTiles[nbToIndex]);
        numbersMap.RefreshTile(coord.ToVector3Int());
    }

    public static Level GetActiveLevel() =>
        SaveManager.GetCreatingLevel() ? SaveManager.GetNewLevel() : activeLevel;

    public static Player GetPlayerRef() => playerRef;

    public void RemoveInteractableFromMap(Coord coord)
    {
        interactablesMap.SetTile(coord.ToVector3Int(), null);
    }

    public static Coord WorldSpaceToCoord(Vector3 pos)
    {
        Vector3Int temp = gridRef.WorldToCell(pos);
        return new(temp.x, temp.y);
    }

    void OnDestroy()
    {
        GameManager.OnLevelSelect -= SetActiveLevel;
    }
}

public readonly struct Coord
{
    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public readonly int x;
    public readonly int y;

    public readonly Vector3Int ToVector3Int() => new(x, y, 0);

    public override string ToString() => $"({x}, {y})";
}
