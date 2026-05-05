using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trunk : IWalkable
{
    public Trunk(Coord coord, int height)
    {
        this.coord = coord;
        this.height = height;

        if (SaveManager.GetCreatingLevel())
            for (int i = coord.y; i < coord.y + height; i++)
                SaveManager.GetNewLevel().AddToWalkableDict(new Coord(coord.x, i), this);
        else
            for (int i = coord.y; i < coord.y + height; i++)
                LevelManager.GetActiveLevel().AddToWalkableDict(new Coord(coord.x, i), this);
    }

    public Trunk(int x, int y, int height)
        : this(new Coord(x, y), height) { }

    public readonly Coord coord;
    public readonly int height;

    // can walk hori at the BASE of trees! Connecting trunks are floating trunks with a height of 1.
    public bool CanWalkHorizontal(int y) => y == coord.y;

    public bool CanWalkVertical() => true;
}
