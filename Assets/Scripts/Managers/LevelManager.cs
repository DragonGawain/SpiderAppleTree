using System;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    Grid grid;
    public Tilemap walkableMap;
    public Tilemap elementsMap;

    public Tile red,
        orange,
        green,
        yellow,
        black,
        grey;

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
        foreach (ILevelElement ile in activeLevel.GetLevelElements())
        {
            if (ile.GetType() == typeof(Trunk))
            {
                t = (Trunk)ile;
                walkableMap.SetTile(new Vector3Int(t.coord.x, t.coord.y), red);
                for (int y = 1; y < t.height; y++)
                {
                    walkableMap.SetTile(new Vector3Int(t.coord.x, t.coord.y + y), orange);
                }
            }
            else if (ile.GetType() == typeof(Branch))
            {
                b = (Branch)ile;
                if (b.direction == Direction.RIGHT)
                {
                    walkableMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), green);
                    for (int x = 1; x < b.length; x++)
                    {
                        walkableMap.SetTile(new Vector3Int(b.coord.x + x, b.coord.y), yellow);
                    }
                }
                else
                {
                    walkableMap.SetTile(new Vector3Int(b.coord.x, b.coord.y), black);
                    for (int x = 1; x < b.length; x++)
                    {
                        walkableMap.SetTile(new Vector3Int(b.coord.x - x, b.coord.y), grey);
                    }
                }
            }
        }

        Debug.Log("Finished drawing loaded level!");

        // Create player at appropriate spot
        playerRef = Instantiate(
                playerPrefab,
                grid.CellToWorld(activeLevel.GetSpawnPointAsV3I()),
                Quaternion.identity
            )
            .GetComponent<Player>();

        playerRef.Initialize(activeLevel.GetInitialLevelDataContainer(), grid);
    }

    public static Level GetActiveLevel() => activeLevel;

    public static Player GetPlayerRef() => playerRef;

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
}
