using System;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    static Level activeLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.OnLevelSelect += SetActiveLevel;
    }

    // Update is called once per frame
    void Update() { }

    static void SetActiveLevel(int levelID)
    {
        activeLevel = new Level(levelID);
        try
        {
            using (
                StreamReader sr =
                    new(
                        Path.Combine(
                            SaveManager.levelStatePath[LevelStateDictionary.levelStates[levelID]],
                            levelID.ToString()
                        )
                    )
            )
            {
                string json = sr.ReadToEnd();
                JsonSerialization.FromJson<Level>(json);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("Something went wrong when READING the level with id " + levelID + "!");
            throw;
        }

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
