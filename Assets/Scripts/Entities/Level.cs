using System;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    // This was <(int, int), int>, with the tuple being x,y coords, and the value being the number of walkables at the loc.
    // This dealt with overlapping walkables.

    // This is a dictionary and not a set to make searching by coordinate O(1) instead of O(n)
    HashSet<ILevelElement> uniqueLevelElements = new();
    Dictionary<Coord, IWalkable> walkables = new();
    Dictionary<Coord, IInteractable> interactables = new();
    public readonly int levelID;
    InitialLevelDataContainer ldc;

    public Level(int id)
    {
        this.levelID = id;
    }

    public void InitializeData(
        Coord spawnPoint,
        Coord goalPoint,
        int initWebCount,
        int initWeight,
        int initLength
    ) => ldc = new(spawnPoint, goalPoint, initWebCount, initWeight, initLength);

    public void AddWalkable(Coord coord, IWalkable walkable)
    {
        // TODO:: Collision checking (multiple walkables at a single loc).
        // Maybe a list of IWalkables?
        // Maybe I just enforce that there's never any overlap? (seems bad)
        walkables.Add(coord, walkable);
    }

    public void ModifyIsWalkable(Coord oldC, Coord newC, IWalkable walkable)
    {
        walkables.Remove(oldC);
        walkables.Add(newC, walkable);
    }

    public void AddInteractable(Coord coord, IInteractable interactable)
    {
        interactables.Add(coord, interactable);
        uniqueLevelElements.Add(interactable);
    }

    public void RemoveInteractable(Coord coord)
    {
        uniqueLevelElements.Remove(interactables[coord]);
        interactables.Remove(coord);
    }

    /// <summary>
    /// Determines if a target cell location (grid space) can be walked on
    /// </summary>
    /// <param name="targetCoord">The coordinate that the player is attempting to move into</param>
    /// <param name="dir">The direction that the player is coming from.
    /// This means that this input should be the opposite of the direction the player is moving (RIGHT if player is moving LEFT)</param>
    /// <returns>True if the player can move into that space, false otherwise</returns>
    public bool IsWalkable(Coord targetCoord, Direction dir)
    {
        if (!walkables.ContainsKey(targetCoord))
            return false;
        // verify that the element is walkable (unwalkable elements include we reinforcement)
        if (walkables[targetCoord].GetType() is not IWalkable)
            return false;
        // fetch entity at coord
        IWalkable walkable = walkables[targetCoord];

        // if entity at TARGET LOC allows that dir of movement, ret true
        // else ret false.
        return (dir == Direction.UP || dir == Direction.DOWN)
            ? walkable.CanWalkVertical(targetCoord.x, dir)
            : walkable.CanWalkHorizontal(targetCoord.y, dir);
    }

    /// <summary>
    /// Series of checks to be done after a successful move. Namely:
    /// * Collected fruit?
    /// * Reached activated goal?
    /// * Branch stable? (Did a branch fall)
    /// </summary>
    /// <param name="loc">The current location of the player</param>
    public void PostMove(Coord loc)
    {
        // TODO:: check for branch stability
        if (interactables.ContainsKey(loc))
            interactables[loc].Interact();
    }

    public void PostAction() { }

    public Dictionary<Coord, IWalkable> GetWalkables() => walkables;

    public void AddNewElementToLevel(ILevelElement ile) => uniqueLevelElements.Add(ile);

    public void RemoveElementFromLevel(ILevelElement ile) => uniqueLevelElements.Remove(ile);

    public HashSet<ILevelElement> GetLevelElements() => uniqueLevelElements;

    public Coord GetSpawnPoint() => ldc.spawnPoint;

    public Coord GetGoalPoint() => ldc.goalPoint;

    public int GetInitWebCount() => ldc.initWebCount;

    public int GetInitWeight() => ldc.initWeight;

    public int GetInitLenght() => ldc.initLength;

    public InitialLevelDataContainer GetInitialLevelDataContainer() => ldc;

    public int GetLevelID() => levelID;

    class WindController
    {
        //
    }
}

public readonly struct InitialLevelDataContainer
{
    public readonly Coord spawnPoint,
        goalPoint;
    public readonly int initWebCount,
        initWeight,
        initLength;

    public InitialLevelDataContainer(
        Coord spawnPoint,
        Coord goalPoint,
        int initWebCount,
        int initWeight,
        int initLength
    )
    {
        this.spawnPoint = spawnPoint;
        this.goalPoint = goalPoint;
        this.initWebCount = initWebCount;
        this.initWeight = initWeight;
        this.initLength = initLength;
    }
}
