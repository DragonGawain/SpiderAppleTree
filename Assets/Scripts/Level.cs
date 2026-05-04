using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Level : MonoBehaviour
{
    // public Dictionary<int, Trunk> trees = new();

    // This was <(int, int), int>, with the tuple being x,y coords, and the value being the number of walkables at the loc.
    // This dealt with overlapping walkables.
    Dictionary<Coord, IWalkable> isWalkable = new();

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
        // fetch entity at coord
        IWalkable walkable = isWalkable[coord];
        // ~~check if player is moving hori/vert~~

        // if entity at TARGET LOC allows that dir of movement, ret true
        // else ret false.
        return isVertical ? walkable.CanWalkVertical() : walkable.CanWalkHorizontal(coord.y);
    }
}
