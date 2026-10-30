using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.Serialization.Json;
using Unity.VisualScripting;
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
    [Header("Tilemaps")]
    [SerializeField]
    Grid grid;

    [SerializeField]
    Tilemap walkablesMap,
        interactablesMap,
        numbersMap;

    static Grid gridRef;

    [Header("Tiles")]
    [SerializeField]
    Grid numberPalette;

    [SerializeField]
    Tile t_src,
        t_bdy,
        b_src_R,
        b_bdy_R,
        b_con_R,
        b_src_L,
        b_bdy_L,
        b_con_L,
        goal,
        fruit,
        levelEntry;

    Tile[] numberTiles;

    [Header("Entities")]
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    GameObject goalPrefab;

    // REFERENCES

    static Level activeLevel;
    static Player playerRef;
    public static Goal goalRef;

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
        ClearLevel();
        loadingLevel = true;
        // Grab level file
        // Deserialize level file
        activeLevel = new Level(levelID);
        try
        {
            using (
                StreamReader sr = new(
                    Path.Combine(SaveManager.levelPath, levelID.ToString() + ".txt")
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
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            Debug.LogError("Something went wrong when READING the level with id " + levelID + "!");
            throw;
        }

        // Create player at appropriate spot
        playerRef = Instantiate(
                playerPrefab,
                grid.CellToWorld(activeLevel.GetSpawnPoint().ToVector3Int())
                    + new Vector3(0.5f, 0.5f, 0),
                Quaternion.identity
            )
            .GetComponent<Player>();

        // NOTE:: ok, so, here's the dealio. This is flaky. Is a branch snaps in level creation,
        // depending on if the tile that the player is under has been placed or not, the player may instantly die.
        // Since trunks are loaded in before branches, if the player is on a trunk, this will never happen.
        // I just need to be aware of this.

        // HOWEVER! If a branch snaps and the player has not yet been initialized, that will cause a null ref error
        // because the branch snapping invokes the OnGlobalBranchSnap event which only the player is subscribed to,
        // and the player should always be subscribed to that. The error throwing is good in most cases, just not
        // really in the initialization of the level.

        // The above flakiness has been supressed.
        // The player no longer interacts with branch snapping while the level is being loaded.
        // Post load, we simulate a branch snapping (by just telling the player that it happened) and allow the player
        // to perform standard branch snapping checks at the start of the level. [26-10-27]
        playerRef.Initialize(activeLevel.GetInitialLevelDataContainer(), grid);

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

        // HACK:: For any level element that may not exist, it is important that we check for existance!
        List<Fruit> fruits = new();
        if (levelElementsGroups.Any(g => g.Key == typeof(Fruit)))
            fruits = levelElementsGroups.First(g => g.Key == typeof(Fruit)).Cast<Fruit>().ToList();
        List<LevelEntry> levels = new();
        if (levelElementsGroups.Any(g => g.Key == typeof(LevelEntry)))
            levels = levelElementsGroups
                .First(g => g.Key == typeof(LevelEntry))
                .Cast<LevelEntry>()
                .ToList();

        foreach (Trunk t in trunks)
        {
            walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y), t_src);
            for (int y = 1; y < t.height; y++)
                walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y + y), t_bdy);
        }

        foreach (Branch b in branches)
        {
            walkablesMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), b_src_R);
            for (int x = 1; x < b.length; x++)
                walkablesMap.SetTile(new Vector3Int(b.coord.x + x, b.coord.y), b_bdy_R);

            b.FindTrunkSupports(true);
        }

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

            interactablesMap.SetTile(f.coord.ToVector3Int(), fruit);
        }

        foreach (LevelEntry le in levels)
        {
            interactablesMap.SetTile(le.coord.ToVector3Int(), levelEntry);
        }

        // The world map should not have a goal point, but the save file needs it.
        // Solution: we just won't spawn in the goal!
        // However, since the win check is position based, we need to ensure that the goal point can never be reached.
        // This simply means giving it a negative y value!
        if (levelID != 0)
            goalRef = Instantiate(
                    goalPrefab,
                    grid.CellToWorld(
                        activeLevel.GetInitialLevelDataContainer().goalPoint.ToVector3Int()
                    ) + new Vector3(0.5f, 0.5f, 0),
                    quaternion.identity
                )
                .GetComponent<Goal>();

        loadingLevel = false;

        // Recalculating supports is going overboard. I should really only need to update the weight map,
        // but I'm recalculating supports as a safety net.
        foreach (Branch br in branches)
            br.RecalculateSupports();

        Debug.Log("Finished drawing loaded level!");

        // TODO:: Check if the player spawned on a branch and alter branch weight here
        // (the player used to be initialized here - 16-10-2026 update)

        activeLevel.RefreshWalkablesDirections();

        if (levelID == 0)
        {
            InputManager.EnableWorldInputs();
            GameManager.inWorldMap = true;
        }
        else
        {
            InputManager.EnableMovementInputs();
            GameManager.inWorldMap = false;
        }

        // After the level is done being loaded, we call OnBranchSnap on the player as it was supressed beforehand.
        // This will allow us to have levels where the player starts with the webrella deplyed!
        playerRef.OnBranchSnap();
    }

    public void ClearLevel()
    {
        walkablesMap.ClearAllTiles();
        interactablesMap.ClearAllTiles();
        numbersMap.ClearAllTiles();
        if (playerRef != null)
        {
            playerRef.ManuallyUnsubscribeEvents();
            Destroy(playerRef.gameObject);
        }
        playerRef = null;
        if (goalRef != null)
            Destroy(goalRef.gameObject);
        goalRef = null;
        activeLevel = null;
    }

    public void ClearBranch(Branch branch)
    {
        for (int i = 0; i < branch.length; i++)
        {
            walkablesMap.SetTile(new Vector3Int(branch.coord.x + i, branch.coord.y, 0), null);
            numbersMap.SetTile(new Vector3Int(branch.coord.x + i, branch.coord.y, 0), null);
        }
    }

    public void RefreshBranch(Branch branch)
    {
        ClearBranch(branch);

        walkablesMap.SetTile(new Vector3Int(branch.coord.x, branch.coord.y), b_src_R);
        for (int x = 1; x < branch.length; x++)
            walkablesMap.SetTile(new Vector3Int(branch.coord.x + x, branch.coord.y), b_bdy_R);

        branch.RecalculateSupports();
    }

    public void UpdateWeightMap(Coord coord, float value)
    {
        int nbToIndex = Mathf.FloorToInt(value) - 1;
        if (value % 1 != 0)
            nbToIndex += 10;

        Debug.Log("Placing number of value " + value);
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

    public Coord(Vector3Int v)
    {
        x = v.x;
        y = v.y;
    }

    public readonly int x;
    public readonly int y;

    public readonly Vector3Int ToVector3Int() => new(x, y, 0);

    public Vector3 ToWorldSpace() => new(x + 0.5f, y + 0.5f, 0);

    public override string ToString() => $"({x}, {y})";

    // override object.Equals
    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //

        if (obj == null || GetType() != obj.GetType())
            return false;

        // TODO: write your implementation of Equals() here
        return GetHashCode() == ((Coord)obj).GetHashCode();
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        // This assumes that I'llnever have a level that is 100 tiles tall.
        // This is a reasonable assumption.
        return x * 100 + y;
    }
}
