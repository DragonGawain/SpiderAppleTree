using UnityEngine;

public class LevelManager : MonoBehaviour
{
    static Level activeLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public static Level GetActiveLevel() => activeLevel;
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
