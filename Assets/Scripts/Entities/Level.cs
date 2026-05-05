using System;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    // This was <(int, int), int>, with the tuple being x,y coords, and the value being the number of walkables at the loc.
    // This dealt with overlapping walkables.
    Dictionary<Coord, ILevelElement> isWalkable = new();
    readonly int levelID;

    public Level(int id)
    {
        this.levelID = id;
    }

    public void AddToWalkableDict(Coord coord, IWalkable walkable)
    {
        // TODO:: Collision checking (multiple walkables at a single loc).
        // Maybe a list of IWalkables?
        // Maybe I just enforce that there's never any overlap? (seems bad)
        isWalkable.Add(coord, walkable);
    }

    public void ModifyIsWalkable(Coord oldC, Coord newC, IWalkable walkable)
    {
        isWalkable.Remove(oldC);
        isWalkable.Add(newC, walkable);
    }

    public bool IsWalkable(Coord coord, bool isVertical)
    {
        if (!isWalkable.ContainsKey(coord))
            return false;
        // verify that the element is walkable (unwalkable elements include we reinforcement)
        if (isWalkable[coord].GetType() is not IWalkable)
            return false;
        // fetch entity at coord
        IWalkable walkable = (IWalkable)isWalkable[coord];
        // ~~check if player is moving hori/vert~~

        // if entity at TARGET LOC allows that dir of movement, ret true
        // else ret false.
        return isVertical ? walkable.CanWalkVertical() : walkable.CanWalkHorizontal(coord.y);
    }

    public Dictionary<Coord, ILevelElement> GetIsWalkable() => isWalkable;

    public int GetLevelID() => levelID;
}
