using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// It is important that trunks get loaded before branches.
// This is because when a branch is created, it will check to see if its source is next to a trunk.
// If it is, then it sets that trunk segment to be horizontally walkable.

// TODO:: REMOVE LEFT FACING BRANCHES!

public enum BranchType
{
    NORMAL, // no special effects
    HEAVY, // can cause cascading breaks
    WEBBED, // does not consider player weight!
    HOLLOW, // Lands on top of other branches (as opposed to merging with them)
}

public class Branch : IWalkable
{
    public event Action<Coord, Coord, Branch> OnBranchCreate;
    public event Action<Branch> OnBranchSnap;
    public static event Action OnGlobalBranchSnap;

    // Y is NOT readonly! Branches can fall!
    // Y IS in fact readonly. If a branch falls, I will create a new branch instance.
    public Coord coord;
    public readonly int length;

    // A branch is a single object. The direction determines in what direction it extends.
    // public readonly Direction direction;

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

    // FALLING BRANCH STUFF
    bool isFalling;
    const float BRANCH_FALL_SPEED = 20f;
    List<EntityVisualizer> visualizers = new();

    // Used to determine if the next time a branch moves, will it merge with another, already existing branch
    readonly HashSet<Branch> mergeTargets = new();

    // DEBUG:: BRANCH INFO
    public readonly int instanceID;
    static int instanceIDTracker = 0;
    public readonly BranchType branchType = BranchType.NORMAL;

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

    public Branch(Coord coord, int length, bool autoTrunkSupport = true, bool isFalling = false)
    {
        instanceID = instanceIDTracker++;

        this.coord = coord;
        this.length = length;
        this.isFalling = isFalling;

        rawSupports = new int[length];
        weightDeltas = new int[length];
        calculatededSupports = new int[length];
        actualWeight = new float[length];

        if (!isFalling)
        {
            // LevelManager.GetActiveLevel().AddNewElementToLevel(this);

            // for (int i = coord.x; i < coord.x + length; i++)
            //     LevelManager.GetActiveLevel().AddWalkable(new Coord(i, coord.y), this);
            LevelManager.GetActiveLevel().AddWalkable(this);

            if (autoTrunkSupport)
                FindTrunkSupports();
        }
        else
        {
            // I technically don't need this. I can directly add to the list, and then access list[index i].
            // This looks a bit cleaner though?
            EntityVisualizer viz;

            for (int i = 0; i < length; i++)
            {
                viz = GameObject
                    .Instantiate(
                        GameManager.GetManagerSingleton().GetEntityVisualizerPrefab(),
                        coord.ShiftX(i).ToWorldSpace(),
                        Quaternion.identity
                    )
                    .GetComponent<EntityVisualizer>();
                viz.Setup(Visualizer.BRANCH);
                visualizers.Add(viz);
            }
            Player.PostMove += BranchFalling;
            GameManager.DebugLog($"Branch {instanceID} is falling!");
        }

        // TODO:: Have this constructor spawn in a prefab at the desired location
        // Prefab would only be used for animations. But, animated tiles exist, so I might use those instead.
    }

    public Branch(int x, int y, int length, bool autoTrunkSupport = true)
        : this(new Coord(x, y), length, autoTrunkSupport) { }

    public bool CanWalkHorizontal(int y, Direction d) => true;

    public bool CanWalkVertical(int x, Direction d) =>
        d == Direction.UP && verticalWalkables.Contains(x);

    public void FindTrunkSupports(bool addVerticalConections = false)
    {
        GameManager.DebugLog("Finding trunk supports for branch id " + instanceID);
        Coord end = GetBounds().Item2;

        // left side of branch
        LevelManager
            .GetActiveLevel()
            .GetWalkables()
            .TryGetValue(coord.ShiftX(-1), out IWalkable walkable);
        if (walkable != null && walkable.GetType() == typeof(Trunk))
        {
            supportsList.Add(
                new TrunkSupport(
                    coord,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                )
            );
            ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.LEFT);
        }
        // right side of branch
        LevelManager.GetActiveLevel().GetWalkables().TryGetValue(end.ShiftX(1), out walkable);
        if (walkable != null && walkable.GetType() == typeof(Trunk))
        {
            supportsList.Add(
                new TrunkSupport(
                    end,
                    LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport,
                    true
                )
            );
            ((Trunk)walkable).AddHorizontalConnection(coord.y, Direction.RIGHT);
        }

        // underneath
        int minX = coord.x;
        int maxX = end.x;

        // in place swap
        // Since branches now always face right, this in place swap should no longer be needed.
        // I'll leave it in as a safeguard though.
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
                // TODO:: change to veritcal trunk support
                supportsList.Add(
                    new TrunkSupport(
                        new(x, coord.y),
                        LevelManager
                            .GetActiveLevel()
                            .GetInitialLevelDataContainer()
                            .baseTrunkSupport,
                        true
                    )
                );
                if (addVerticalConections)
                    verticalWalkables.Add(x);
            }
        }

        // if (!SaveManager.GetCreatingLevel() && !LevelManager.GetLoadingLevel())
        RecalculateSupports();
    }

    public void AddOrUpdateSupports(ISupport support, bool suppressRecalculation = false)
    {
        GameManager.DebugLog(
            "<color=orange>adding trunk support for branch of id: " + instanceID + ".</color>"
        );
        if (!supportsList.Contains(support))
            supportsList.Add(support);
        int segmentIndex = CoordToSegmentIndex(support.GetCoord());
        rawSupports[segmentIndex] = 0;
        foreach (ISupport iSup in supportsList)
            if (iSup.GetCoord().Equals(support.GetCoord()))
                rawSupports[segmentIndex] += support.GetSupportValue();

        // HACK
        // DEBUG
        // FIXME
        // This exists to deal with branch weight overflow. It shouldn't need to exist.
        if (rawSupports[segmentIndex] > 9)
            rawSupports[segmentIndex] = 9;
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
            if (rawSupports[i] == 0)
                calculatededSupports[i] = calculatededSupports[i - 1] - 1;

        // backward pass
        for (int i = length - 2; i >= 0; i--)
            if (rawSupports[i] == 0 && calculatededSupports[i] < calculatededSupports[i + 1])
                calculatededSupports[i] = calculatededSupports[i + 1] - 1;

        RecalculateActualWeight();
    }

    public void UpdateWeightDelta(Coord crd, int delta, bool isWeightPlayer = false)
    {
        // HACK:: ensure that weightDeltas are never negative.
        // Not sure if I actually "need" to, because weightD is typically only applied if above 0..
        // Still, a negative value could cause funkiness if it somehow gets stuck below 0.
        GameManager.DebugLog(
            $"<color=black>Updating weight of branch id {instanceID} at {crd} by {delta} </color>"
        );
        if (isWeightPlayer && branchType == BranchType.WEBBED)
        {
            GameManager.DebugLog(
                $"<color=black>Weight source is player on webbed branch! Not updating weight delta!</color>"
            );
            return;
        }

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

        int currentSupportIndexIndex = 0;

        // These are the indices of the segment with support.
        int leftSupportIndex = -1;
        int specialSupportIndex = -1;
        int rightSupportIndex = supportIndices[currentSupportIndexIndex++];

        // loop through each SEGment of the branch. seg is the index of the segment.
        for (int seg = 0; seg < length; seg++)
        {
            // We start scanning the branch. Since all branches face right,
            // we know that we start at the leftmost segment and move to the right.

            // Step 1: Determine the supports for this branch segment.
            // There are 4 cases:
            // Cases are determined by two factors: support(s) in direction (L/R), branch segment has a support (on/off)
            // Depending on the case, we calculate as either SINGLE, DUAL, or DUAL_SPEC
            // Case 1: Only left support (off)                  (SINGLE)
            // Case 2: Only right support (off)                 (SINGLE)
            // Case 3: Support to both left and right (off)     (DUAL)
            // Case 4: ~~Support to either left or right~~ (on) (DUAL_SPEC)

            // Do the index shuffle!
            // If there was a special, shift it to the left
            if (specialSupportIndex != -1)
            {
                leftSupportIndex = specialSupportIndex;
                specialSupportIndex = -1;
            }
            // If the right support index is the current support index, shuffle the indices
            if (rightSupportIndex == seg)
            {
                specialSupportIndex = rightSupportIndex;
                rightSupportIndex =
                    supportIndices.Count > currentSupportIndexIndex
                        ? supportIndices[currentSupportIndexIndex++]
                        : -1;
            }

            // We need to do the index shuffle for every branch segment,
            // but beyond this point, we only care about segments with a positive weight delta.
            if (weightDeltas[seg] == 0)
                continue;

            // Case 4: On a support => DUAL_SPEC (half weight to ONLY the supported segment)
            if (specialSupportIndex != -1)
            {
                // Apply half the weight to this segment only. No cascading!
                actualWeight[seg] -= weightDeltas[seg] / 2f;
            }
            // Case 3: Support to both sides. Not on a support. => DUAL
            else if (leftSupportIndex != -1 && rightSupportIndex != -1)
            {
                float half = weightDeltas[seg] / 2f;
                for (int i = leftSupportIndex; i <= rightSupportIndex; i++)
                    actualWeight[i] -= half;
            }
            // Case 2: Support only to the right (SINGLE)
            else if (leftSupportIndex == -1 && rightSupportIndex != -1)
            {
                for (int i = seg; i <= rightSupportIndex; i++)
                    actualWeight[i] -= weightDeltas[seg];
            }
            // Case 1: Support only to the left ((SINGLE))
            else if (leftSupportIndex != -1 && rightSupportIndex == -1)
            {
                for (int i = leftSupportIndex; i <= seg; i++)
                    actualWeight[i] -= weightDeltas[seg];
            }
            else
            {
                GameManager.DebugLog(
                    $"A branch segment without any supports has been found at coord {coord}.",
                    GameManager.LogType.ERROR
                );
            }
        }

        // Old notes
        // ----------
        // TODO:: verify mid-support true minimum accepted weights
        // This means that if a mid support has a support value of X, the neighbouring segments should display a maximum capacity of X as well.
        // (I'm not doing this now. Set this up when mid-supports start to exist for real)
        // Finally, we need to do a consistency check:
        // If a mid-support is bearing a weight, all values between it and its neighbours must be at most the value of the mid-support
        // Exception: if one of the neighbouring supports is strong enough to help the mid-support
        // (THIS EXCEPTION WILL LIKELY BE HARD TO DETERMINE)

        // TODO:: Determine if branch should snap
        // This check should be disabled during level load and only activated once level load is completed.
        // ----------

        // If we are creating the level, we shouldn't update the weight map
        // We should also only check for branch snapping if we are not saving/loading a level.
        if (!SaveManager.GetCreatingLevel() && !LevelManager.GetLoadingLevel())
        {
            // if (!BranchSnapCheck(supportIndices))
            //     UpdateWeightMap();
            if (!BranchSnap())
                UpdateWeightMap();
        }
    }

    bool BranchSnap()
    {
        List<int> supportIndices = new();

        // build support indices
        for (int i = 0; i < length; i++)
            if (rawSupports[i] > 0)
                supportIndices.Add(i);

        // Loop through entire branch checking for breaks
        for (int seg = 0; seg < length; seg++)
        {
            // never nesting: if the weight of this segment is greater than 0, this segment is not a snap source
            if (actualWeight[seg] > 0)
                continue;

            // DEBUG BELOW v
            List<ISupport> leftSupports = supportsList
                .Where(s =>
                    s.GetCoord().x < coord.x + seg - 1 /*&& !s.IsAutoSupport()*/
                )
                .ToList();

            GameManager.DebugLog("IS's supporting the left branch:");
            foreach (ISupport IS in leftSupports)
            {
                GameManager.DebugLog(
                    $"Type: {IS.GetType()}, coord: {IS.GetCoord()}, val: {IS.GetSupportValue()}"
                );
            }
            // DEBUG ABOVE ^

            GameManager.DebugLog("SNAP DETECTED!");
            GameManager.DebugLog(
                "PRE number of walkables: " + LevelManager.GetActiveLevel().GetWalkables().Count
            );

            // Invoke 'OnBranchSnap' on the branch that snapped.
            // This will clean up any auto-supports that were being used by the snapped branch.
            // INFO:: Is this even needed? Won't the garbage collector deal with this?
            OnBranchSnap?.Invoke(this);

            LevelManager levelManager = GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>();

            // Step 0: remove this branch from the level list.
            LevelManager.GetActiveLevel().RemoveWalkable(this);
            levelManager.ClearBranch(this);

            // Step 1: set up for the creation of up to 3 new branches (left, mid (falling), right)

            List<ISupport> midSupports = new(); // ???

            List<Coord> midBranchSegments = new(); // ???

            // Step 2: construct the branch segments.

            // Step 2.1: THE LEFT BRANCH
            // We know that the seg value is the index of the leftmost segment with a support value of 0.
            // Therefore, the left segment is everything to left of [seg-1]!
            // Therefore, we only need a left branch segment iff the breaking segment is segment 2 or greater
            if (seg > 1)
                GenerateSubBranch(coord, seg - 1, (a, b) => a < b);

            // Step 2.2: THE MIDDLE BRANCH (i.e. the falling section)
            // If we are not on the very first segment, then we add the segment to the left of the break to the middle branch
            if (seg > 0)
                midBranchSegments.Add(coord.ShiftX(seg - 1));
            // Add segments to the falling piece until a value that is greater than 0 is found.
            // Add that segment, then check for supports to the right
            for (; seg < length; seg++)
            {
                midBranchSegments.Add(coord.ShiftX(seg));
                if (actualWeight[seg] > 0)
                    break;
            }
            // Step 2.3: THE RIGHT BRANCH (we'll finish up with middle branch stuff in step 3 after we're done with the right branch)
            // We'll first do a quick check to make sure a right branch needs to be made:
            seg++;
            // All the segments, from seg until the end of the branch, belong to the right segment
            if (length > seg)
                GenerateSubBranch(coord.ShiftX(seg), length - seg, (a, b) => a > b);

            // Step 3: dealing with the falling branch
            // (This step is the core of the refactor. The branch will no longer teleport to its destination, and will instead gracefully move downwards)

            // HACK:: There might be a case where the midsegment of a snapped branch does NOT fall.
            // By definition, this should never happen.
            // TODO
            // BUT! What if there is an underneath trunk support? Underneath trunk supports should (technically) offer infinite support
            // to the branch resting ontop of it. But it's possible that a branch snap occured right next to an under-tree support.
            // In this case, that branch segment simply should not be included in the midbranch.
            // This is a bit of a special case, so I'll deal with it later.

            new Branch(midBranchSegments[0], midBranchSegments.Count, false, true);

            GameManager.DebugLog(
                "POST number of walkables: " + LevelManager.GetActiveLevel().GetWalkables().Count
            );

            LevelManager.GetActiveLevel().RemoveElementFromLevel(this);
            OnGlobalBranchSnap.Invoke();
            GameManager.DebugLog(
                "<color=red>Branch snap completed successfully. A branch with original origin of "
                    + coord
                    + " been snapped and recalculated.</color>"
            );

            return true;
        }

        return false;
    }

    //  My refactoring class is telling me that it's a good idea to make this small submethod!
    void GenerateSubBranch(Coord src, int length, Func<int, int, bool> comp)
    {
        Branch branch = new(src, length);

        // Add each of the non-auto supports.
        // BUG:: This has a problem with hanging web strings. HWS's need references to their upper and lower branches.
        supportsList.ForEach(s =>
        {
            if (comp(s.GetCoord().x, src.x - 1) && !s.IsAutoSupport())
                branch.AddOrUpdateSupports(s);
        });

        // List<ISupport> nonAutos = supportsList
        //     .Where(s => comp(s.GetCoord().x, src.x - 1) && !s.IsAutoSupport())
        //     .ToList();
        // foreach (ISupport IS in nonAutos)
        //     branch.AddOrUpdateSupports(IS, true);

        GameManager.GetManagerSingleton().GetComponent<LevelManager>().RefreshBranch(branch); // RefreshBranch calls RecalculateSupports
        branch.RefreshInteractables();
    }

    /// <summary>
    /// This method is subscribed to Player.PostMove if the branch is falling.
    /// The player cannot walk onto a falling branch. (even if they could, it would instant kill them)
    /// </summary>
    void BranchFalling()
    {
        // Move visualizer down 1 cell
        coord = coord.ShiftY(-1);
        if (mergeTargets.Count > 0)
        {
            foreach (EntityVisualizer viz in visualizers)
                viz.StartCoroutine(viz.SmoothMove(Vector3.down, BRANCH_FALL_SPEED, true));
            GameManager.DebugLog($"<color=blue>FALLING BRANCH MERGED AT COORD {coord}!</color>");
            MergeBranch();
            return;
        }

        // Check if space bellow (second fall):
        if (!IsBranchLandingOrMerging())
        {
            foreach (EntityVisualizer viz in visualizers)
                viz.StartCoroutine(viz.SmoothMove(Vector3.down * 2, BRANCH_FALL_SPEED * 2));
            coord = coord.ShiftY(-1);
        }

        if (mergeTargets.Count > 0)
        {
            foreach (EntityVisualizer viz in visualizers)
                viz.StartCoroutine(viz.SmoothMove(Vector3.down, BRANCH_FALL_SPEED, true));
            MergeBranch();
            GameManager.DebugLog($"<color=blue>FALLING BRANCH MERGED AT COORD {coord}!</color>");
            return;
        }

        if (coord.y <= -1)
        {
            // Remove the branch full
            // TODO:: if meta branch, check for world map meta branch placement
            foreach (EntityVisualizer viz in visualizers)
                viz.StartCoroutine(viz.SmoothMove(Vector3.zero, 0, true));
            GameManager.DebugLog($"<color=black>FALLING BRANCH FELL BELOW THE LEVEL!</color>");
        }

        if (IsBranchLandingOrMerging())
        {
            GameManager.DebugLog($"<color=green>FALLING BRANCH LANDED AT COORD {coord}</color>");
            LandBranch();
        }

        GameManager.DebugLog($"<color=red>FALLING BRANCH NOW AT COORD {coord}</color>");

        // Place branch tiles (tilemapping)
    }

    /// <summary>
    /// Helper method for falling branches. Checks if the branch has landed.
    /// </summary>
    /// <returns>true if branch is done falling, false otherwise</returns>
    bool IsBranchLandingOrMerging()
    {
        IWalkable walkable;
        // A falling branch cannot be caught by side tree supports
        // A falling branch can only be stopped by landing on top of a tree (stump), or by landing on top of another branch.
        // This is NOT the same as landing on top of another walkable since there are other types of walkables (webs)

        bool ret = false;

        for (int seg = 0; seg < length; seg++)
        {
            // If landing ON TOP OF something
            if (
                LevelManager
                    .GetActiveLevel()
                    .GetWalkables()
                    .TryGetValue(coord.ShiftXY(seg, -1), out walkable)
            )
            {
                // if ret is already true, this will short circuit
                ret = ret || walkable.LandMergeCheck(this);

                // // Hollow branches land on top of existing branches
                // if (
                //     walkable.GetType() == typeof(Trunk)
                //     || (branchType == BranchType.HOLLOW && walkable.GetType() == typeof(Branch))
                // )
                //     return true;
                // // If there is another branch below, set the flag to state such.
                // // We do not return false here because there might be a trunkunder the falling branch to the right.
                // // If there is a trunk, we prioritize landing on it over mergin with the branch below.
                // else if (walkable.GetType() == typeof(Branch))
                //     mergeTargets.Add((Branch)walkable);
                // else
                // {
                //     // TODO:: falling branch-???(web?) collision handling
                //     // Webs should be destroyed?
                //     // Maybe webs can catch webbed branches?
                //     // Will need to decide.
                // }
            }
        }
        return ret;
        // return false;
    }

    /// <summary>
    /// The falling branch is now merging with an (or two) existing branch(es)
    /// </summary>
    void MergeBranch()
    {
        Player.PostMove -= BranchFalling;
        isFalling = false;
        // The set `mergeTargets` contains all the branches directly below the falling one.
        // However, when merging, we also merge with branches immediately to the left and right, so we check for the existence of those:
        LevelManager levelManager = GameManager.GetManagerSingleton().GetComponent<LevelManager>();
        Level level = LevelManager.GetActiveLevel();

        IWalkable walkable;
        // Check to the left
        if (level.GetWalkables().TryGetValue(coord.ShiftX(-1), out walkable))
            if (walkable.GetType() == typeof(Branch))
                // only merge with branches of the same type?? => maybe type conversion to falling type?
                if (((Branch)walkable).branchType == branchType)
                    mergeTargets.Add((Branch)walkable);

        // Check to the right
        if (level.GetWalkables().TryGetValue(coord.ShiftX(1), out walkable))
            if (walkable.GetType() == typeof(Branch))
                // only merge with branches of the same type?? => maybe type conversion to falling type?
                if (((Branch)walkable).branchType == branchType)
                    mergeTargets.Add((Branch)walkable);

        int minX = coord.x;
        int maxX = coord.x + length - 1;
        (Coord, Coord) bounds;
        foreach (Branch mt in mergeTargets)
        {
            bounds = mt.GetBounds();
            minX = bounds.Item1.x < minX ? bounds.Item1.x : minX;
            maxX = bounds.Item2.x > maxX ? bounds.Item2.x : maxX;

            // HACK:: This will likely have problems with non-auto supports!
            level.RemoveWalkable(mt);
            levelManager.ClearBranch(mt);
        }

        Branch merged = new(new(minX, coord.y), maxX - minX + 1);
        levelManager.RefreshBranch(merged);
        merged.RefreshInteractables();
    }

    /// <summary>
    /// The falling branch is landing on top of something else (likely a trunk)
    /// </summary>
    void LandBranch()
    {
        Player.PostMove -= BranchFalling;
        isFalling = false;
        LevelManager.GetActiveLevel().AddWalkable(this);
        FindTrunkSupports(true);
        GameManager.GetManagerSingleton().GetComponent<LevelManager>().RefreshBranch(this);
        RefreshInteractables();
    }

    public void AddMergeTarget(Branch b) => mergeTargets.Add(b);

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

        for (int i = 0; i < length; i++)
        {
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + i, coord.y), actualWeight[i]);
        }
    }

    public void ManuallySnapBranch(Coord coord)
    {
        // TODO:: for 'heavy branches' (if they become a thing), this is how they will snap all the branches it collides with as it falls
    }

    public int CoordToSegmentIndex(Coord crd) => crd.x - coord.x;

    public Coord SegmentIndexToCoord(int index) => new(coord.x + index, coord.y);

    public (Coord, Coord) GetBounds() => (coord, new(coord.x + length - 1, coord.y));

    public bool LandMergeCheck(Branch branch)
    {
        if (branch.branchType == BranchType.HOLLOW)
            return true;
        branch.AddMergeTarget(this);
        return false;
    }

    ~Branch()
    {
        GameManager.DebugLog(
            $"<color=magenta>Branch of id {instanceID}, sourced at coord {coord} has been fully deleted!</color>"
        );
    }
}
