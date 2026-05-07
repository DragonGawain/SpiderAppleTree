using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trunk : IWalkable
{
    public readonly Coord coord;
    public readonly int height;

    HashSet<(int, Direction)> horizontalWalkables = new();

    public Trunk(Coord coord, int height)
    {
        this.coord = coord;
        this.height = height;

        if (SaveManager.GetCreatingLevel())
        {
            SaveManager.GetNewLevel().AddNewElementToLevel(this);
            for (int i = coord.y; i < coord.y + height; i++)
                SaveManager.GetNewLevel().AddWalkable(new Coord(coord.x, i), this);
        }
        else
        {
            LevelManager.GetActiveLevel().AddNewElementToLevel(this);
            for (int i = coord.y; i < coord.y + height; i++)
                LevelManager.GetActiveLevel().AddWalkable(new Coord(coord.x, i), this);
        }

        horizontalWalkables.Add((coord.y, Direction.LEFT));
        horizontalWalkables.Add((coord.y, Direction.RIGHT));

        // TODO:: Have this constructor spawn in a prefab at the desired location
        // (prefab will be used to animate the image of the entity)
        // https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@8.88/manual/AnimatedTile.html
        // Animated tiles exist, so maybe prefab won't be needed?
    }

    public Trunk(int x, int y, int height)
        : this(new Coord(x, y), height) { }

    // can walk hori at the BASE of trees! Connecting trunks are floating trunks with a height of 1.
    public bool CanWalkHorizontal(int y, Direction d) => horizontalWalkables.Contains((y, d));

    public bool CanWalkVertical(int x, Direction d) => true;

    public void AddHorizontalConnection(int y, Direction d) => horizontalWalkables.Add((y, d));

    public void RemoveHorizontalConnection(int y, Direction d) =>
        horizontalWalkables.Remove((y, d));

    public void ClearHorizontalConnections()
    {
        horizontalWalkables.Clear();
        horizontalWalkables.Add((coord.y, Direction.LEFT));
        horizontalWalkables.Add((coord.y, Direction.RIGHT));
    }
}
