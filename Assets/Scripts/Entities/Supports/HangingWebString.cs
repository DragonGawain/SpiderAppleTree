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
    readonly Branch upper;
    readonly Branch lower;
    readonly int upperSegmentIndex;
    readonly int lowerSegmentIndex;

    public HangingWebString(
        Branch upper,
        Branch lower,
        int upperSegmentIndex,
        int lowerSegmentIndex
    )
    {
        this.upper = upper;
        this.lower = lower;
        this.upperSegmentIndex = upperSegmentIndex;
        this.lowerSegmentIndex = lowerSegmentIndex;
    }

    public int GetSupportValue()
    {
        return Mathf.FloorToInt(upper.actualWeight[upperSegmentIndex]) - 1;
    }

    public Coord GetCoord() => upper.SegmentIndexToCoord(upperSegmentIndex);

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch)
    {
        throw new System.NotImplementedException();
    }

    public SupportType GetSupportType() => SupportType.HANGING_WEB_STRING;

    public (Coord, Coord) GetBounds()
    {
        // Convetnion states that we provide coords left, right OR lower, upper. (shouldn't matter either way, but conventions are nice!)
        return (
            lower.SegmentIndexToCoord(lowerSegmentIndex),
            upper.SegmentIndexToCoord(upperSegmentIndex)
        );
    }
}
