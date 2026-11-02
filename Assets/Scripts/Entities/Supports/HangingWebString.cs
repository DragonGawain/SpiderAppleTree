/*
A few possible cases.
Case 1: the lower branch is only being held by web strings (simple)
- The upper offers equal support to the lower. Weight on the lower connection point will affect the support potential of the upper.

Case 2: the lower branch also has some fixed (trunk) supports (complex)
- The upper increases the support strength of the lower by its own value. Load only gets transferred to the upper once the
initial support value of the lower is surpassed.

*/
using UnityEngine;

public class HangingWebString : ISupport, IWalkable
{
    // If I end up having water mechanics/'enemies', it might become important to know exactly where this web support is.
    // Even without that, it should ideally be possible to walk vertically on the web support...
    IWalkable upper;
    IWalkable lower;
    readonly Coord upperCoord;
    readonly Coord lowerCoord;

    public HangingWebString(IWalkable upper, IWalkable lower, Coord upperCoord, Coord lowerCoord)
    {
        this.upper = upper;
        this.lower = lower;
        this.upperCoord = upperCoord;
        this.lowerCoord = lowerCoord;

        Branch.OnGlobalBranchSnap += OnSnap;
        Player.PostMove += RecalculateLowerSupport;
    }

    // called by the lower branch.
    // Therefore, if this is called, the lower is certain to be a branch.

    // HWS's can only be made from/to trunks/branches
    // (I don't think this will ever change?)
    public int GetSupportValue()
    {
        if (upper.GetType() == typeof(Trunk))
            return LevelManager.GetActiveLevel().GetInitialLevelDataContainer().baseTrunkSupport;
        else
            return Mathf.CeilToInt(
                    ((Branch)upper).actualWeight[((Branch)upper).CoordToSegmentIndex(upperCoord)]
                ) - 1;
    }

    // For vertical structures, the source is typically considered the lowest coord.
    // For this, the source (intuitively) would be the upper...
    public Coord GetCoord() => lowerCoord;

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch)
    {
        throw new System.NotImplementedException();
    }

    void OnSnap()
    {
        Level level = LevelManager.GetActiveLevel();
        // Check if the top and bottom still exist.
        // top check
        if (
            !level.GetWalkables().TryGetValue(upperCoord, out upper)
            || !level.GetWalkables().TryGetValue(lowerCoord, out lower)
        )
        {
            // If either one of the two end points does NOT contain a walkable, this HWS needs to be destroyed
            // I'm not sure what that process looks like just yet..
            Branch.OnGlobalBranchSnap -= OnSnap;
            Player.PostMove -= RecalculateLowerSupport;
            return;
        }

        RecalculateLowerSupport();
    }

    void RecalculateLowerSupport()
    {
        if (lower.GetType() == typeof(Branch))
        {
            // add this support to the lower branch,
            // then recalculate that branches supports

            ((Branch)lower).AddOrUpdateSupports(this);
            ((Branch)lower).RecalculateSupports();
        }
    }

    public SupportType GetSupportType() => SupportType.HANGING_WEB_STRING;

    public bool IsAutoSupport() => false;

    public (Coord, Coord) GetBounds()
    {
        // Convetnion states that we provide coords left, right OR lower, upper.
        return (lowerCoord, upperCoord);
    }

    public bool LandMergeCheck(Branch branch)
    {
        return false;
    }
}
