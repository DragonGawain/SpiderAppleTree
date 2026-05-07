using System;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    Grid grid;
    public Tilemap walkablesMap;
    public Tilemap interactablesMap;

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
    GameObject playerPrefab;

    static Level activeLevel;
    static Player playerRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnLevelSelect += SetActiveLevel;
    }

    void SetActiveLevel(int levelID)
    {
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

        Trunk t;
        Branch b;
        Fruit f;
        // Tile tile;
        foreach (ILevelElement ile in activeLevel.GetLevelElements())
        {
            if (ile.GetType() == typeof(Trunk))
            {
                t = (Trunk)ile;
                walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y), t_src);
                for (int y = 1; y < t.height; y++)
                {
                    walkablesMap.SetTile(new Vector3Int(t.coord.x, t.coord.y + y), t_bdy);
                }
            }
            else if (ile.GetType() == typeof(Branch))
            {
                b = (Branch)ile;
                if (b.direction == Direction.RIGHT)
                {
                    walkablesMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), b_src_R);
                    for (int x = 1; x < b.length; x++)
                    {
                        walkablesMap.SetTile(new Vector3Int(b.coord.x + x, b.coord.y), b_bdy_R);
                    }
                }
                else
                {
                    walkablesMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), b_src_L);
                    for (int x = 1; x < b.length; x++)
                    {
                        walkablesMap.SetTile(new Vector3Int(b.coord.x - x, b.coord.y), b_bdy_L);
                    }
                }
            }
            else if (ile.GetType() == typeof(Fruit))
            {
                f = (Fruit)ile;

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
                        _
                            => throw new Exception(
                                "ERROR: Unknown FruitType supplied when loading level"
                            )
                    }
                );
            }
        }

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

    public static Level GetActiveLevel() => activeLevel;

    public static Player GetPlayerRef() => playerRef;

    public void RemoveInteractableFromMap(Coord coord)
    {
        interactablesMap.SetTile(coord.ToVector3Int(), null);
    }

    void OnDestroy()
    {
        GameManager.OnLevelSelect -= SetActiveLevel;
    }
}

public struct Coord
{
    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public readonly int x;
    public int y;

    public readonly Vector3Int ToVector3Int() => new(x, y, 0);

    public override string ToString() => $"({x}, {y})";
}
