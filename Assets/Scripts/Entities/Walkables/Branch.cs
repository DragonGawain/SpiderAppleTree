using System;
using System.Collections.Generic;

// It is important that trunks get loaded before branches.
// This is because when a branch is created, it will check to see if its source is next to a trunk.
// If it is, then it sets that trunk segment to be horizontally walkable.

using System.Linq;

public class Branch : IWalkable
{
    public event Action<Coord> OnBranchSnap;

    // Y is NOT readonly! Branches can fall!
    // Y IS in fact readonly. If a branch falls, I will create a new branch instance.
    public Coord coord;
    public readonly int length;

    // A branch is a single object. The direction determines in what direction it extends.
    public readonly Direction direction;

    /// <summary>
    /// This is the amount of weight that the source of a branch can hold.
    /// </summary>
    public const int MAX_BRANCH_SUPPORT = 6;

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

    public Branch(Coord coord, int length, Direction direction)
    {
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

        // TODO:: Have this constructor spawn in a prefab at the desired location
        // Prefab would only be used for animations. But, animated tiles exist, so I might use those instead.
    }

    public Branch(int x, int y, int length, Direction direction)
        : this(new Coord(x, y), length, direction) { }

    public Branch(int x, int y, int length, int direction)
        : this(new Coord(x, y), length, (Direction)direction) { }

    public Branch(Coord coord, int length, int direction)
        : this(coord, length, (Direction)direction) { }

    public bool CanWalkHorizontal(int y, Direction d) => true;

    public void UpdateSupports(ISupport support)
    {
        if (!supportsList.Contains(support))
            supportsList.Add(support);
        int segmentIndex = CoordToSegmentIndex(support.GetCoord());
        rawSupports[segmentIndex] += support.GetSupportValue();
        RecalculateSupports();
    }

    // public void UpdateSupports(Coord crd, int delta)
    // {
    //     int segmentIndex = CoordToSegmentIndex(crd);
    //     rawSupports[segmentIndex] += delta;

    //     RecalculateSupports();
    // }

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
        weightDeltas[CoordToSegmentIndex(crd)] += delta;
        RecalculateActualWeight();
    }

    void RecalculateActualWeight()
    {
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
                        rightSupportIndex = supportIndices[nextSupportIndex];
                        nextSupportIndex =
                            supportIndices.Count > nextSupportIndex ? nextSupportIndex + 1 : -1;
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
                            supportIndices.Count >= nextSupportIndex ? nextSupportIndex + 1 : -1;
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
                int dir = direction == Direction.LEFT ? -1 : 1;

                // Step 1: determine how much of the branch should snap
                // Step 1.5: short circuit, are there supports on both sides of the snap point, or only one side?

                // A support on the same segment that snapped is considered to be on the left side (arbitrary)
                bool left = supportIndices.Any(s => s <= seg);
                bool right = supportIndices.Any(s => s > seg);

                // If there is support on both sides...
                if (left && right)
                {
                    //
                }
                // else, there is support only on one side (and that side is likely to be left, but we should not assume that)
                else
                {
                    // single support on the left => snap from seg to the end
                    if (left)
                    {
                        GameManager.DebugLog("L: Branch segments to snap are at coords: ");
                        for (int i = seg; i < length; i++)
                        {
                            GameManager.DebugLog("" + new Coord(coord.x + (i * dir), coord.y));
                        }
                    }
                    // single support on the right (this means the support is closer to the end of the branch)
                    // => snap from seg to root of branch
                    else
                    {
                        GameManager.DebugLog("R: Branch segments to snap are at coords: ");
                        for (int i = seg; i >= 0; i--)
                        {
                            GameManager.DebugLog("" + new Coord(coord.x + (i * dir), coord.y));
                        }
                    }
                }

                return true;
            }
        }
        return false;
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

        int dir = direction == Direction.LEFT ? -1 : 1;
        for (int i = 0; i < length; i++)
        {
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + (i * dir), coord.y), actualWeight[i]);
        }
    }

    int CoordToSegmentIndex(Coord crd) =>
        direction == Direction.LEFT ? coord.x - crd.x : crd.x - coord.x;
}
