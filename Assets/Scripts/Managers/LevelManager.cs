using System;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public Tilemap tilemap;

    public Tile red,
        orange,
        green,
        yellow,
        black,
        grey;

    static Level activeLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnLevelSelect += SetActiveLevel;
    }

    // Update is called once per frame
    void Update() { }

    void SetActiveLevel(int levelID)
    {
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
                tilemap.SetTile(new Vector3Int(t.coord.x, t.coord.y), red);
                for (int y = 1; y < t.height; y++)
                {
                    tilemap.SetTile(new Vector3Int(t.coord.x, t.coord.y + y), orange);
                }
            }
            else if (ile.GetType() == typeof(Branch))
            {
                b = (Branch)ile;
                if (b.direction == Direction.RIGHT)
                {
                    tilemap.SetTile(new Vector3Int(b.coord.x, b.coord.y), green);
                    for (int x = 1; x < b.length; x++)
                    {
                        tilemap.SetTile(new Vector3Int(b.coord.x + x, b.coord.y), yellow);
                    }
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(b.coord.x, b.coord.y), black);
                    for (int x = 1; x < b.length; x++)
                    {
                        tilemap.SetTile(new Vector3Int(b.coord.x - x, b.coord.y), grey);
                    }
                }
            }
        }

        Debug.Log("Finished drawing loaded level!");
        // Grab level file
        // Deserialize level file
    }

    public static Level GetActiveLevel() => activeLevel;

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
