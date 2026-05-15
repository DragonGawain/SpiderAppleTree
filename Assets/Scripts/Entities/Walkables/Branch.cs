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

        Level level = SaveManager.GetCreatingLevel()
            ? SaveManager.GetNewLevel()
            : LevelManager.GetActiveLevel();

        level.AddNewElementToLevel(this);
        if (direction == Direction.RIGHT)
            for (int i = coord.x; i < coord.x + length; i++)
                level.AddWalkable(new Coord(i, coord.y), this);
        else
            for (int i = coord.x; i > coord.x - length; i--)
                level.AddWalkable(new Coord(i, coord.y), this);

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
        // by default, I'm assuming all branches face to the right.
        // This is because arrays are 0 indexed and visually described as extending to the right.
        // If a branch is left facing, we simply flip the world coordinate. The segment index is identical.
        int leftSupportIndex = supportIndices[0];
        int rightSupportIndex = -1;
        int nextSupportIndex = -1;
        if (!singleSupport)
            rightSupportIndex = supportIndices[1];
        if (supportIndices.Count > 1)
            nextSupportIndex = 2;

        float half; // I don't want to need to keep recalculating the half. Division is expensive!

        // Then, we scan the weightDeltas.
        // When we find a wd, we need to determine the following:
        // 1: are the supports to the left, the right, or both?
        // 2: based on the closest supports, what are the segments that this weight affects?
        for (int i = 0; i < length; i++)
        {
            // If the current segment is the right support segment, shift over the registered support indices
            if (!singleSupport)
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

        // TODO:: verify mid-support true minimum accepted weights
        // (I'm not doing this now. Set this up when mid-supports start to exist for real)
        // Finally, we need to do a consistency check:
        // If a mid-support is bearing a weight, all values between it and its neighbours must be at most the value of the mid-support
        // Exception: if one of the neighbouring supports is strong enough to help the mid-support
        // (THIS EXCEPTION WILL LIKELY BE HARD TO DETERMINE)

        // TODO:: Determine if branch should snap
        // This check should be disabled during level load and only activated once level load is completed.

        // If we are creating the level, we shouldn't update the weight map
        if (!SaveManager.GetCreatingLevel() && !LevelManager.GetLoadingLevel())
            UpdateWeightMap();
    }

    public void UpdateWeightMap()
    {
        // GameManager.DebugLog("raw supports:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + rawSupports[i]);

        // GameManager.DebugLog("\ncalculated supports:");
        // for (int i = 0; i < length; i++)
        //     GameManager.DebugLog("" + calculatededSupports[i]);

        GameManager.DebugLog("\nweight deltas:");
        for (int i = 0; i < length; i++)
            GameManager.DebugLog("" + weightDeltas[i]);

        GameManager.DebugLog("actual weight:");
        for (int i = 0; i < length; i++)
            GameManager.DebugLog("" + actualWeight[i]);

        int dir = direction == Direction.LEFT ? -1 : 1;
        for (int i = 0; i < length; i++)
        {
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + (i * dir), coord.y), actualWeight[i]);
        }
    }

    public void Interacted(int delta)
    {
        weightDeltas[0] += delta;
        // check for snaps here!
    }

    // ------------------------

    // ------------------------

    // ------------------------

    // When the player moves from one branch segment to another, this method will end up getting called twice
    /// <summary>
    /// Recalculate the amount of support that each segment of the branch has according to the new change that occured.
    /// </summary>
    /// <param name="pos">The position of the change</param>
    /// <param name="delta">The amount of change.
    /// Positive value => branch has more support (thing removed)
    /// Negative value => branch has less support (thing added)
    /// </param>
    public void RecalculateWeights(Coord pos, int delta)
    {
        int segmentIndex;
        int dir = 1;
        if (direction == Direction.LEFT)
        {
            segmentIndex = coord.x - pos.x;
            dir = -1;
        }
        else
            segmentIndex = pos.x - coord.x;

        weightDeltas[segmentIndex] += delta;

        // determine if there are multiple supports on this branch.
        // weightDeltas are literal, not inverse.
        // So, a positive value will indicate a branch support.
        // A negative value indicates that an object with positive weight is there, requiring support.
        int nbs = rawSupports.Where((t, v) => v > 0).Count();
        GameManager.DebugLog("There are " + nbs + " weight deltas greater than 0.");
        int allNbs = rawSupports.Where((t, v) => v != 0).Count();
        GameManager.DebugLog("There are " + nbs + " non-zero weight deltas.");

        // up propagation: stepping on the end of a branch will impact the support of the source
        for (int i = 0; i <= segmentIndex; i++)
        {
            weightDeltas[i] += delta;
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + (i * dir), coord.y), weightDeltas[i]);
        }
    }

    // HACK:: Maybe this should also be a delta?
    // The reason it's not is because it's a tuple, and that makes checking the value slightly tough.

    // WAIT WAIT WAIT!! I just realized that we can have multiple supports on a single branch segment.
    // Hmm, either I disallow that, or I need 2 arrays. One for additive, one for multiplicative.
    public void SetSupport(Coord pos, int delta)
    {
        int segmentIndex;
        if (direction == Direction.LEFT)
            segmentIndex = coord.x - pos.x;
        else
            segmentIndex = pos.x - coord.x;

        rawSupports[segmentIndex] += delta;
        RecalculateWeights(new Coord(coord.x + length - 1, coord.y), 0);
    }

    int CoordToSegmentIndex(Coord crd) =>
        direction == Direction.LEFT ? coord.x - crd.x : crd.x - coord.x;
}
