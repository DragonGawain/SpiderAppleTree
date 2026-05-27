using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level
{
    // This was <(int, int), int>, with the tuple being x,y coords, and the value being the number of walkables at the loc.
    // This dealt with overlapping walkables.

    // This is a dictionary and not a set to make searching by coordinate O(1) instead of O(n)
    HashSet<ILevelElement> uniqueLevelElements = new();
    Dictionary<Coord, IWalkable> walkables = new();
    Dictionary<Coord, IInteractable> interactables = new();
    HashSet<Fruit> neededFruits = new();
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
        int initLength,
        int baseTrunkSupport
    ) => ldc = new(spawnPoint, goalPoint, initWebCount, initWeight, initLength, baseTrunkSupport);

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

    // Alias: OnBranchSnap
    public void RemoveWalkable(IWalkable walkabale)
    {
        (Coord, Coord) bounds = walkabale.GetBounds();
        uniqueLevelElements.Remove(walkables[bounds.Item1]);

        if (bounds.Item1.x < bounds.Item2.x)
            for (int x = bounds.Item1.x; x <= bounds.Item2.x; x++)
                walkables.Remove(new(x, bounds.Item1.y));
        else if (bounds.Item1.x > bounds.Item2.x)
            for (int x = bounds.Item2.x; x <= bounds.Item1.x; x++)
                walkables.Remove(new(x, bounds.Item1.y));
        else if (bounds.Item1.y < bounds.Item2.y)
            for (int y = bounds.Item1.y; y <= bounds.Item2.y; y++)
                walkables.Remove(new(bounds.Item1.x, y));
        else if (bounds.Item1.y > bounds.Item2.y)
            for (int y = bounds.Item2.y; y <= bounds.Item1.y; y++)
                walkables.Remove(new(bounds.Item1.x, y));
    }

    public void RefreshWalkablesDirections()
    {
        /* Trunks:: Segment is HORI walkable on side iff adjacent to:
        1. other trunk src
        2. branch
        */
        int x,
            y;
        IWalkable w;
        List<Trunk> trunks = uniqueLevelElements.Where(e => e is Trunk).Cast<Trunk>().ToList();
        foreach (Trunk t in trunks)
        {
            x = t.coord.x;
            t.ClearHorizontalConnections();
            for (y = t.coord.y + 1; y < t.coord.y + t.height; y++)
            {
                // LEFT
                if (walkables.TryGetValue(new Coord(x - 1, y), out w))
                {
                    if (w.GetType() == typeof(Branch))
                        t.AddHorizontalConnection(y, Direction.LEFT);
                    else if (w.GetType() == typeof(Trunk))
                        if (((Trunk)w).coord.y == y)
                            t.AddHorizontalConnection(y, Direction.LEFT);
                }

                // RIGHT
                if (walkables.TryGetValue(new Coord(x + 1, y), out w))
                {
                    if (w.GetType() == typeof(Branch))
                        t.AddHorizontalConnection(y, Direction.RIGHT);
                    else if (w.GetType() == typeof(Trunk))
                        if (((Trunk)w).coord.y == y)
                            t.AddHorizontalConnection(y, Direction.RIGHT);
                }
            }
        }

        /* Branches:: Segment is VERT walkable on side if adjacent to:
        1. web string (not made yet)
        */
    }

    /// <summary>
    /// Determines if a target cell location (grid space) can be walked on
    /// </summary>
    /// <param name="currentPos">The coordinate that the player is currently in</param>
    /// <param name="dir">The direction that the player is coming from.
    /// This means that this input should be the opposite of the direction the player is moving (RIGHT if player is moving LEFT)</param>
    /// <returns>True if the player can move into that space, false otherwise</returns>
    public bool IsWalkable(Coord currentPos, Direction dir)
    {
        // TODO:: if webrella is active, ALL cells are walkable.
        // Webrella auto-deactivates if you land on a walkable
        Coord targetCoord = dir switch
        {
            Direction.UP => new(currentPos.x, currentPos.y - 1),
            Direction.RIGHT => new(currentPos.x - 1, currentPos.y),
            Direction.DOWN => new(currentPos.x, currentPos.y + 1),
            Direction.LEFT => new(currentPos.x + 1, currentPos.y),
            _
                => throw new Exception(
                    "ERROR: Attempted to move the player in an undefined Direction"
                )
        };

        // Debug.Log("Walkable at target? " + walkables.ContainsKey(targetCoord));

        if (!walkables.ContainsKey(targetCoord))
            return false;

        Debug.Log("Moving from " + currentPos + " to " + targetCoord);
        // fetch entity at coord
        IWalkable walkable = walkables[targetCoord];

        // Debug.Log("walkable type: " + walkable.GetType());

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

    public Dictionary<Coord, IInteractable> GetInteractables() => interactables;

    public void AddNewElementToLevel(ILevelElement ile) => uniqueLevelElements.Add(ile);

    public void RemoveElementFromLevel(ILevelElement ile) => uniqueLevelElements.Remove(ile);

    public void RegisterNeededFruit(Fruit fruit) => neededFruits.Add(fruit);

    public void ConsumeNeededFruit(Fruit fruit)
    {
        neededFruits.Remove(fruit);
        if (AllFruitsConsumed())
            LevelManager.goalRef.Activate();
    }

    public bool AllFruitsConsumed() => neededFruits.Count == 0;

    public HashSet<ILevelElement> GetLevelElements() => uniqueLevelElements;

    public Coord GetSpawnPoint() => ldc.spawnPoint;

    public InitialLevelDataContainer GetInitialLevelDataContainer() => ldc;

    public int GetLevelID() => levelID;

    // DEBUG

    public void AnalyzeUniqueElements()
    {
        Debug.Log("<color=yellow>Analyzing unique elements in the level!</color>");
        foreach (ILevelElement ile in uniqueLevelElements)
        {
            Debug.Log("ile type: " + ile.GetType());
            Debug.Log("is IWalkable?: " + (ile is IWalkable)); // TRUE!
            Debug.Log("is type of IWalkable?: " + (ile.GetType() == typeof(IWalkable))); // false :(
        }
    }

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
        initLength,
        initWeight,
        baseTrunkSupport;

    public InitialLevelDataContainer(
        Coord spawnPoint,
        Coord goalPoint,
        int initWebCount,
        int initWeight,
        int initLength,
        int baseTrunkSupport
    )
    {
        this.spawnPoint = spawnPoint;
        this.goalPoint = goalPoint;
        this.initWebCount = initWebCount;
        this.initWeight = initWeight;
        this.initLength = initLength;
        this.baseTrunkSupport = baseTrunkSupport;
    }
}
