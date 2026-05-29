using System;
using System.Collections.Generic;
using System.Linq;

// It is important that trunks get loaded before branches.
// This is because when a branch is created, it will check to see if its source is next to a trunk.
// If it is, then it sets that trunk segment to be horizontally walkable.


public class Branch : IWalkable
{
    public event Action<Coord, Coord, Branch> OnBranchSnap;
    public static event Action OnGlobalBranchSnap;

    // Y is NOT readonly! Branches can fall!
    // Y IS in fact readonly. If a branch falls, I will create a new branch instance.
    public Coord coord;
    public readonly int length;

    // A branch is a single object. The direction determines in what direction it extends.
    public readonly Direction direction;

    // Supports prevent the branch from snapping.
    // They will most often be positive, otherwise they will be pulling the branch down
    // (which can happen! A string support will drag the upper branch down a bit)
    // They can be additive (source support by trunk)
    // They can be multiplicative (string support)
    // A negative support is the same as a positive weight, but weights can't support multiplicative values,
    // so we can have negative multiplicative supports.

    // ** IN FACT, THE ONLY TIME WE'LL HAVE A NEGATIVE SUPPORT IS IF IT'S MULTIPLICATIVE! **
    int[] rawSupports;

    // List of all the weight values on the branch, assuming no load.
    int[] calculatededSupports;

    // A weightDelta is the weight an entity is exerting on a branch.
    // This will typically be the player's weight or the weight of a fruit.
    // Weight deltas are ALWAYS additive
    // A negative weight is the same as a positive support. So, negative weights (i.e. flying fruit) should be considered supports.

    // List of all the weights applied to the branch, irrespective of the support that that segment can hold.
    // This is stritcly the forces being applied to the branch.
    int[] weightDeltas;

    // The actual amount of weight that each branch segment can hold, after all calculations.
    // These values are what get displayed in the game.
    public readonly float[] actualWeight;

    // used so I can actually save and recover the supports
    public readonly List<ISupport> supportsList = new();

    HashSet<int> verticalWalkables = new();

    // DEBUG
    public readonly int instanceID;
    static int instanceIDTracker = 0;

    /*
    SUPPORT MODEL
    s: support
    b: branch
    <>: load bearing indicator
    |: end of lead bearing (inclusive)
    #: sample id
    *: only half weight applied (when supported from both sides)

          * * * * * *
    1 > | < < 2>|<3 > |<4
    b b S b b b S b b S b
    */

    public Branch(Coord coord, int length, Direction direction, bool autoTrunkSupport = true)
    {
        instanceID = instanceIDTracker++;

        this.coord = coord;
        this.length = length;
        this.direction = direction;

        rawSupports = new int[length];
        weightDeltas = new int[length];
        calculatededSupports = new int[length];
        actualWeight = new float[length];

        LevelManager.GetActiveLevel().AddNewElementToLevel(this);
        if (direction == Direction.RIGHT)
            for (int i = coord.x; i < coord.x + length; i++)
                LevelManager.GetActiveLevel().AddWalkable(new Coord(i, coord.y), this);
        else
            for (int i = coord.x; i > coord.x - length; i--)
                LevelManager.GetActiveLevel().AddWalkable(new Coord(i, coord.y), this);

        if (autoTrunkSupport)
            FindTrunkSupports();

        // TODO:: Have this constructor spawn in a prefab at the desired location
        // Prefab would only be used for animations. But, animated tiles exist, so I might use those instead.
    }

    public Branch(int x, int y, int length, Direction direction, bool autoTrunkSupport = true)
        : this(new Coord(x, y), length, direction, autoTrunkSupport) { }

    public Branch(int x, int y, int length, int direction, bool autoTrunkSupport = true)
        : this(new Coord(x, y), length, (Direction)direction, autoTrunkSupport) { }

    public Branch(Coord coord, int length, int direction, bool autoTrunkSupport = true)
        : this(coord, length, (Direction)direction, autoTrunkSupport) { }

    public bool CanWalkHorizontal(int y, Direction d) => true;

    public bool CanWalkVertical(int x, Direction d) =>
        d == Direction.DOWN && verticalWalkables.Contains(x);

    public void FindTrunkSupports(bool addVerticalConections = false)
    {
        GameManager.DebugLog("Finding trunk supports for branch id " + instanceID);
        Coord end = GetBounds().Item2;
        IWalkable walkable;
        // dir = RIGHT
        if (direction == Direction.RIGHT)
        {
            // left side
            LevelManager
                .GetActiveLevel()
                .GetWalkables()
                .TryGetValue(coord.ShiftX(-1), out walkable);
            if (walkable != null && walkable.GetType() == typeof(Trunk))
            {
                new TrunkSupport(
                    coord,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                );
                ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.RIGHT);
            }
            // right side
            LevelManager.GetActiveLevel().GetWalkables().TryGetValue(end.ShiftX(1), out walkable);
            if (walkable != null && walkable.GetType() == typeof(Trunk))
            {
                new TrunkSupport(
                    end,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                );
                ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.LEFT);
            }
        }
        // dir = LEFT
        else
        {
            // left side
            LevelManager.GetActiveLevel().GetWalkables().TryGetValue(end.ShiftX(-1), out walkable);
            if (walkable != null && walkable.GetType() == typeof(Trunk))
            {
                new TrunkSupport(
                    end,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                );
                ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.RIGHT);
            }
            // right side
            LevelManager
                .GetActiveLevel()
                .GetWalkables()
                .TryGetValue(coord.ShiftX(1), out walkable);
            if (walkable != null && walkable.GetType() == typeof(Trunk))
            {
                new TrunkSupport(
                    coord,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                );
                ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.RIGHT);
            }
        }

        // underneath
        int minX = coord.x;
        int maxX = end.x;

        // in place swap
        if (minX > maxX)
        {
            maxX -= minX;
            minX += maxX;
            maxX = minX - maxX;
        }

        for (int x = minX; x <= maxX; x++)
        {
            LevelManager
                .GetActiveLevel()
                .GetWalkables()
                .TryGetValue(new(x, coord.y - 1), out walkable);
            if (walkable != null && walkable.GetType() == typeof(Trunk))
            {
                new TrunkSupport(
                    new(x, coord.y),
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                );
                if (addVerticalConections)
                    verticalWalkables.Add(x);
            }
        }

        // if (!SaveManager.GetCreatingLevel() && !LevelManager.GetLoadingLevel())
        RecalculateSupports();
    }

    public void UpdateSupports(ISupport support, bool suppressRecalculation = false)
    {
        GameManager.DebugLog(
            "<color=orange>adding trunk support for branch of id: " + instanceID + ".</color>"
        );
        if (!supportsList.Contains(support))
            supportsList.Add(support);
        int segmentIndex = CoordToSegmentIndex(support.GetCoord());
        rawSupports[segmentIndex] += support.GetSupportValue();
        if (!suppressRecalculation)
            RecalculateSupports();
    }

    /// <summary>
    /// THIS METHOD SHOULD ONLY BE CALLED BY SUPPORTS FROM THE `OnBranchSnap` INVOCATION.
    /// ANY OTHER CALLER IS DOING SOMETHING WRONG THAT WILL LIKELY CAUSE AN ERROR.
    /// </summary>
    /// <param name="support">The ISupport to be removed from the list</param>
    public void RemoveSupport(ISupport support)
    {
        supportsList.Remove(support);
    }

    public void RecalculateSupports()
    {
        // sync/reset calced support values
        for (int i = 0; i < length; i++)
            calculatededSupports[i] = rawSupports[i];

        // forward pass (doesn't matter which side is "forward" and which is "backward")
        for (int i = 1; i < length; i++)
        {
            if (rawSupports[i] == 0)
            {
                calculatededSupports[i] = calculatededSupports[i - 1] - 1;
            }
        }

        // backward pass
        for (int i = length - 2; i >= 0; i--)
        {
            if (rawSupports[i] == 0 && calculatededSupports[i] < calculatededSupports[i + 1])
            {
                calculatededSupports[i] =
                    Math.Max(calculatededSupports[i + 1], calculatededSupports[i]) - 1;
            }
        }

        RecalculateActualWeight();
    }

    public void UpdateWeightDelta(Coord crd, int delta)
    {
        // HACK:: ensure that weightDeltas are never negative.
        // Not sure if I actually "need" to, because weightD is typically only applied if above 0..
        // Still, a negative value could cause funkiness if it somehow gets stuck below 0.
        GameManager.DebugLog(
            $"<color=black>Updating weight of branch id {instanceID} at {crd} by {delta} </color>"
        );

        weightDeltas[CoordToSegmentIndex(crd)] += delta;
        // if (
        //     !SaveManager.GetCreatingLevel()
        //     && !LevelManager.GetLoadingLevel()
        //     && !suppressRecalculation
        // )
        //     RecalculateActualWeight();
    }

    public void RecalculateActualWeight()
    {
        GameManager.DebugLog(
            $"<color=black>Recalculating actual weight of branch id {instanceID} with source {coord}</color>"
        );
        List<int> supportIndices = new();

        // We start by assuming there is no load on the branch
        // We do this by making actualWeight a copy of calculatededSupports

        // At the same time, we also record the indices of all supports
        for (int i = 0; i < length; i++)
        {
            actualWeight[i] = calculatededSupports[i];

            if (rawSupports[i] > 0)
                supportIndices.Add(i);
        }

        bool singleSupport = supportIndices.Count == 1;

        // GameManager.DebugLog(
        //     "<color=orange>nb supports for branch of id: "
        //         + instanceID
        //         + " => "
        //         + supportIndices.Count
        //         + ".</color>"
        // );

        // FORWARD PASS

        // by default, I'm assuming all branches face to the right.
        // This is because arrays are 0 indexed and visually described as extending to the right.
        // If a branch is left facing, we simply flip the world coordinate. The segment index is identical.
        int leftSupportIndex = supportIndices[0];
        int rightSupportIndex = -1;
        int nextSupportIndex = -1;
        if (!singleSupport)
            rightSupportIndex = supportIndices[1];
        if (supportIndices.Count > 2)
            nextSupportIndex = 2;

        float half; // I don't want to need to keep recalculating the half. Division is expensive!

        // Then, we scan the weightDeltas.
        // When we find a wd, we need to determine the following:
        // 1: are the supports to the left, the right, or both?
        // 2: based on the closest supports, what are the segments that this weight affects?
        for (int i = 0; i < length; i++)
        {
            // If the current segment is the right support segment, shift over the registered support indices
            if (!singleSupport) // short circuit
            {
                if (i == rightSupportIndex)
                {
                    leftSupportIndex = i;
                    if (nextSupportIndex == -1)
                        singleSupport = true;
                    else
                    {
                        GameManager.DebugLog("DOT: " + nextSupportIndex);
                        rightSupportIndex = supportIndices[nextSupportIndex++];
                        nextSupportIndex =
                            supportIndices.Count == nextSupportIndex ? -1 : nextSupportIndex;
                    }
                }
            }
            // weight delta found!
            if (weightDeltas[i] > 0)
            {
                // step 1: determine load bearing supports
                if (singleSupport)
                {
                    // only leftSupportIndex matters
                    for (int j = leftSupportIndex; j <= i; j++)
                        actualWeight[j] -= weightDeltas[i];
                }
                else
                {
                    // half weight moves to both supports
                    half = weightDeltas[i] / 2f;
                    // If the weight is on the mid-support, the weight should only apply to the supported segment.
                    // The weight should still be halved though due to the existance of a support farther down the branch.
                    // If there were no other supports to the right, singleSupport would be true.
                    if (i == leftSupportIndex)
                        actualWeight[i] -= half;
                    else
                        for (int j = leftSupportIndex; j <= rightSupportIndex; j++)
                            actualWeight[j] -= half;
                }
            }
        }

        // BACKWARD PASS
        // Account for supports that are supporting segments to their left.
        // i.e. what if a branch only has one support and it's not at index 0?
        rightSupportIndex = supportIndices[^1]; // this should be referencing the rightmost support
        leftSupportIndex = -1;
        nextSupportIndex = -1;

        singleSupport = supportIndices.Count == 1;
        if (!singleSupport)
            leftSupportIndex = supportIndices[^2];
        if (supportIndices.Count > 2)
            nextSupportIndex = 3;

        // Then, we scan the weightDeltas.
        // When we find a wd, we need to determine the following:
        // 1: are the supports to the left, the right, or both?
        // 2: based on the closest supports, what are the segments that this weight affects?
        for (int i = length - 1; i >= 0; i--)
        {
            // If the current segment is the LEFT support segment, shift over the registered support indices
            if (!singleSupport) // short circuit
            {
                if (i == leftSupportIndex)
                {
                    rightSupportIndex = i;
                    if (nextSupportIndex == -1)
                        singleSupport = true;
                    else
                    {
                        leftSupportIndex = supportIndices[^nextSupportIndex];
                        nextSupportIndex =
                            supportIndices.Count > nextSupportIndex ? nextSupportIndex + 1 : -1;
                    }
                }
            }
            // weight delta found!
            if (weightDeltas[i] > 0)
            {
                // step 1: determine load bearing supports
                // We don't want to factor in if the weight is directly on the support segment.
                // That has already been accounted for in the forward pass.
                if (singleSupport && i != rightSupportIndex)
                {
                    // only rightSupportIndex matters
                    for (int j = rightSupportIndex; j >= i; j--)
                        actualWeight[j] -= weightDeltas[i];
                }
                // If it is NOT single support, then the weight has already been factored in during the forward pass!
            }
        }

        // TODO:: verify mid-support true minimum accepted weights
        // This means that if a mid support has a support value of X, the neighbouring segments should display a maximum capacity of X as well.
        // (I'm not doing this now. Set this up when mid-supports start to exist for real)
        // Finally, we need to do a consistency check:
        // If a mid-support is bearing a weight, all values between it and its neighbours must be at most the value of the mid-support
        // Exception: if one of the neighbouring supports is strong enough to help the mid-support
        // (THIS EXCEPTION WILL LIKELY BE HARD TO DETERMINE)

        // TODO:: Determine if branch should snap
        // This check should be disabled during level load and only activated once level load is completed.

        // If we are creating the level, we shouldn't update the weight map
        // We should also only check for branch snapping if we are not saving/loading a level.
        if (!SaveManager.GetCreatingLevel() && !LevelManager.GetLoadingLevel())
        {
            if (!BranchSnapCheck(supportIndices))
                UpdateWeightMap();
        }
    }

    bool BranchSnapCheck(List<int> supportIndices)
    {
        for (int seg = 0; seg < length; seg++)
        {
            // A weight of value 0 or less has been detected!
            // The branch should snap!
            if (actualWeight[seg] <= 0)
            {
                GameManager.DebugLog("SNAP DETECTED!");
                GameManager.DebugLog(
                    "PRE number of walkables: " + LevelManager.GetActiveLevel().GetWalkables().Count
                );

                LevelManager levelManager = GameManager
                    .GetManagerSingleton()
                    .GetComponent<LevelManager>();

                // Step 0: remove this branch from the level list.
                LevelManager.GetActiveLevel().RemoveWalkable(this);
                levelManager.ClearBranch(this);

                int dir = direction == Direction.LEFT ? -1 : 1;

                // Step 1: set up for the creation of up to 3 new branches (left, mid (falling), right)
                Branch leftBranch,
                    midBranch,
                    rightBranch;

                // And the corresponding supports that each segment will have
                // The middle segment has no supports because it's falling. The supports don't fall with it..
                // INFO:: Maybe flying fruit? I have an idea for a level where a branch suspended by a flying fruit on each end bobs up and down as needed,
                // so the flying fruit would need to travel with the branch...
                List<ISupport> leftSupports = new();
                List<ISupport> rightSupports = new();

                List<ISupport> midSupports = new();
                List<Coord> midBranchSegments = new();

                // Step 2: determine the legnth of each of the segments.
                // We know that the `seg` value is at the leftmost index that is <= 0, so that's a good starting point.
                // We also determine the actual number of segments needed here.
                // We also build the left/right segments that are the leftover pieces of the old branch that remain in place.

                if (seg > 1)
                {
                    // create leftBranch piece, only if it would exist.
                    // We don't assume the entire branch falls if this is false because it's possible that the only supports are on the right
                    leftBranch = new(coord, seg - 1, direction);
                    OnBranchSnap?.Invoke(
                        leftBranch.coord,
                        new(
                            leftBranch.coord.x + ((leftBranch.length - 1) * dir),
                            leftBranch.coord.y
                        ),
                        leftBranch
                    );
                    levelManager.RefreshBranch(leftBranch);
                    leftBranch.RefreshInteractables();
                }
                int midLength = seg;
                Coord onceLeft = new(coord.x + ((midLength - 1) * dir), coord.y);
                // we only add the coordinate one to the 'left' of the snap point if it is part of the snapped branch
                if (
                    (coord.x <= onceLeft.x && GetBounds().Item2.x >= onceLeft.x)
                    || (coord.x >= onceLeft.x && GetBounds().Item2.x <= onceLeft.x)
                )
                    midBranchSegments.Add(onceLeft);
                // Add segments to the falling piece until a value that is greater than 0 is found.
                // Add that segment, then check for supports to the right
                for (midLength = seg; midLength < length; midLength++)
                {
                    midBranchSegments.Add(new(coord.x + (midLength * dir), coord.y));
                    if (actualWeight[midLength] > 0)
                        break;
                }
                // Are there supports to the right of the minimum falling segments?
                bool right = supportIndices.Any(s => s > midLength);

                midLength++;
                // If not, add the rest of the branch to the mid segment
                if (!right)
                {
                    for (; midLength < length; midLength++)
                        midBranchSegments.Add(new(coord.x + (midLength * dir), coord.y));
                }
                // If yes, create the right segment
                else
                {
                    // right branch segment
                    if (length - midLength > 0)
                    {
                        // create rightBranch piece
                        rightBranch = new(
                            new(coord.x + (midLength * dir), coord.y),
                            length - midLength,
                            direction
                        );
                        OnBranchSnap?.Invoke(
                            rightBranch.coord,
                            new(
                                rightBranch.coord.x + ((rightBranch.length - 1) * dir),
                                rightBranch.coord.y
                            ),
                            rightBranch
                        );
                        levelManager.RefreshBranch(rightBranch);
                        rightBranch.RefreshInteractables();
                    }
                }

                // Step 3: Determine if the falling segment has any applicable supports
                // Since supports outside of the falling region remove themselves, we know that all remaining supports belong to the falling region!
                foreach (ISupport support in supportsList)
                {
                    // TODO:: some operation to see if they are a hanging support or a static support.
                    // For now, I'm assuming all static, so I don't need to do anything.
                    // Will simply need to not add any static supports to the midSupports list
                }

                // Step 4: Find the place where the falling branch should settle. The conditions for settling are:
                // 1) Any segment lands on another branch => combine all touched branches together (form a bridge!)
                // 2) A hanging support becomes taut // TODO I haven't set up hanging supports yet, so I won't write this logic yet
                // 3) Flying fruit stabilize the branch height [NOT SURE IF I WILL IMPLEMENT THIS. IT'S A BIT OF SCOPE CREEP]

                // Debug logs: original position of the falling segments
                // NOTE:: I might want to export the stuff below here to helper methods and switch based on branch typem if that becomes a thing.
                GameManager.DebugLog("mid branch segment coords: ");
                foreach (Coord mbs in midBranchSegments)
                {
                    GameManager.DebugLog(mbs.ToString());
                }

                int minX = midBranchSegments[0].x;
                int maxX = midBranchSegments[^1].x;
                // in place swap
                if (minX > maxX)
                {
                    maxX -= minX;
                    minX += maxX;
                    maxX = minX - maxX;
                }
                int y = coord.y - 1;

                // scan every place the falling branch would pass through, from top to bottom, left to right
                for (; y >= -1; y--)
                {
                    // TODO:: check for taut hanging support here
                    for (int x = minX; x <= maxX; x++)
                        if (LevelManager.GetActiveLevel().GetWalkables().ContainsKey(new(x, y)))
                            goto FallingBranchCollisionFound;
                    // break;
                }

                FallingBranchCollisionFound:
                GameManager.DebugLog("Potential landing coords found at y level: " + y);

                // Step 5: if the branch landed on something (i.e. y > -1), build the new branch!
                // if y is -1, it means it found nothing to land on, and so we shouldn't do anything. Just let the branch delete itself.
                if (y > -1)
                {
                    // Get all the walkabales underneath the branch
                    // Defined as a hashset to enforce element uniqueness
                    HashSet<IWalkable> walkables = LevelManager
                        .GetActiveLevel()
                        .GetWalkables()
                        .Where(kvp => kvp.Key.y == y && kvp.Key.x >= minX && kvp.Key.x <= maxX)
                        .Select(kvp => kvp.Value)
                        .ToHashSet();

                    // if we find at least 1 trunk segment...
                    // We lift up the branch by 1
                    // (we know the space is empty, cause if it wasn't the branch would have collided with whatever is above it)
                    // And have the trunk(s) give support from underneath.
                    // (also need to make the portions connecting the branch to the trunk vertically walkable)
                    if (walkables.Any(w => w.GetType() == typeof(Trunk)))
                    {
                        y++;
                        midBranch = new(
                            new(minX, y),
                            midBranchSegments.Count,
                            Direction.RIGHT,
                            false
                        );
                        midBranch.FindTrunkSupports(true);
                    }
                    // Otherwise, we haven't landed on a trunk.
                    // At the moment, the only possibility in this case is that we've landed on at least 1 branch.
                    else
                    {
                        // In this case, the first thing we need to do is expand the search slightly: 1 tile left, and 1 tile right
                        // left scan
                        // if (
                        //     LevelManager
                        //         .GetActiveLevel()
                        //         .GetWalkables()
                        //         .ContainsKey(new(minX - 1, y))

                        // )
                        if (
                            LevelManager
                                .GetActiveLevel()
                                .GetWalkables()
                                .Where(ws => ws.Key.Equals(new Coord(minX - 1, y)))
                                .Select(kvp => kvp.Value.GetType() == typeof(Branch))
                                .First()
                        )
                            walkables.Add(
                                LevelManager.GetActiveLevel().GetWalkables()[new(minX - 1, y)]
                            );
                        // right scan
                        // if (
                        //     LevelManager
                        //         .GetActiveLevel()
                        //         .GetWalkables()
                        //         .ContainsKey(new(maxX + 1, y))
                        // )
                        if (
                            LevelManager
                                .GetActiveLevel()
                                .GetWalkables()
                                .Where(ws => ws.Key.Equals(new Coord(maxX + 1, y)))
                                .Select(kvp => kvp.Value.GetType() == typeof(Branch))
                                .First()
                        )
                            walkables.Add(
                                LevelManager.GetActiveLevel().GetWalkables()[new(maxX + 1, y)]
                            );

                        (Coord, Coord) bounds;
                        Branch landed;
                        List<Coord> fallenMidBranchSegments = new();
                        foreach (Coord c in midBranchSegments)
                            fallenMidBranchSegments.Add(new(c.x, y));

                        foreach (IWalkable walkable in walkables)
                        {
                            bounds = walkable.GetBounds();
                            if (bounds.Item1.x < bounds.Item2.x)
                                for (int xb = bounds.Item1.x; xb <= bounds.Item2.x; xb++)
                                    fallenMidBranchSegments.Add(new(xb, y));
                            else if (bounds.Item1.x > bounds.Item2.x)
                                for (int xb = bounds.Item2.x; xb <= bounds.Item1.x; xb++)
                                    fallenMidBranchSegments.Add(new(xb, y));

                            // HACK:: I'm assuming that if the branch didn't land on a trunk, it landed on a branch.
                            // This will not always be true.
                            landed = (Branch)walkable;
                            // Know what was supporting the branches the falling branch landed on
                            foreach (ISupport s in landed.supportsList)
                                midSupports.Add(s);
                            // Lastly, remove the branche(s) that we landed on from the level.
                            LevelManager.GetActiveLevel().RemoveWalkable(walkable);
                            levelManager.ClearBranch(landed);
                        }

                        // remove duplicates and order the elements in ascending order of x values (i.e. make it face right)
                        // Also remove the original segments. We only want the segments at the appropriate height.
                        fallenMidBranchSegments = fallenMidBranchSegments
                            .Distinct()
                            .Where(c => c.y == y)
                            .OrderBy(c => c.x)
                            .ToList();

                        GameManager.DebugLog(
                            "<color=yellow>mid branch segment coords - combined: </color>"
                        );
                        foreach (Coord mbs in fallenMidBranchSegments)
                        {
                            GameManager.DebugLog(mbs.ToString());
                        }

                        midBranch = new(
                            fallenMidBranchSegments[0],
                            fallenMidBranchSegments.Count,
                            Direction.RIGHT,
                            false
                        );
                        foreach (ISupport s in midSupports)
                        {
                            if (s.GetType() == typeof(TrunkSupport))
                            {
                                new TrunkSupport(
                                    midBranch,
                                    new(s.GetCoord(), s.GetSupportValue()),
                                    true
                                );
                            }
                        }
                    }
                    levelManager.RefreshBranch(midBranch);
                    midBranch.RefreshInteractables();
                }

                GameManager.DebugLog(
                    "POST number of walkables: "
                        + LevelManager.GetActiveLevel().GetWalkables().Count
                );

                LevelManager.GetActiveLevel().RemoveElementFromLevel(this);
                OnGlobalBranchSnap.Invoke();
                return true;
            }
        }
        return false;
    }

    void RefreshInteractables()
    {
        (Coord, Coord) bounds = GetBounds();
        IInteractable interactable;
        // Since this is only ever called for branches that are a result of a snap, we know that they will alway face right.
        // Therefore, the first bound (which is the source) will have a lower x than the other bound.
        for (int x = bounds.Item1.x; x <= bounds.Item2.x; x++)
        {
            LevelManager
                .GetActiveLevel()
                .GetInteractables()
                .TryGetValue(new(x, coord.y), out interactable);
            if (interactable != null)
            {
                if (interactable.GetType() == typeof(Fruit))
                    interactable.Refresh();
            }
        }
        RecalculateActualWeight();
    }

    public void UpdateWeightMap()
    {
        // GameManager.DebugLog("raw supports:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + rawSupports[i]);

        // GameManager.DebugLog("\ncalculated supports:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + calculatededSupports[i]);

        // GameManager.DebugLog("\nweight deltas:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + weightDeltas[i]);

        // GameManager.DebugLog("actual weight:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + actualWeight[i]);

        GameManager.DebugLog("Updating branch with id " + instanceID);

        int dir = direction == Direction.LEFT ? -1 : 1;
        for (int i = 0; i < length; i++)
        {
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + (i * dir), coord.y), actualWeight[i]);
        }
    }

    public void ManuallySnapBranch(Coord coord)
    {
        // TODO:: for 'heavy branches' (if they become a thing), this is how they will snap all the branches it collides with as it falls
    }

    int CoordToSegmentIndex(Coord crd) =>
        direction == Direction.LEFT ? coord.x - crd.x : crd.x - coord.x;

    Coord SegmentIndexToCoord(int index) =>
        direction == Direction.LEFT ? new(coord.x - index, coord.y) : new(coord.x + index, coord.y);

    public (Coord, Coord) GetBounds() =>
        (coord, new(coord.x + ((length - 1) * (direction == Direction.LEFT ? -1 : 1)), coord.y));
}
