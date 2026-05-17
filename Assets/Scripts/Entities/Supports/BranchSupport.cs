using UnityEngine;

public class BranchSupport : ISupport
{
    readonly Coord coord;
    readonly Branch supportSource;
    readonly Branch supportTarget;
    readonly int supportValue;

    public BranchSupport(Coord coord, Branch supportSource, Branch supportTarget, int supportValue)
    {
        this.coord = coord;
        this.supportSource = supportSource;
        this.supportTarget = supportTarget;
        this.supportValue = supportValue;

        // supportSource.???
        supportTarget.UpdateSupports(this);
    }

    public Coord GetCoord() => coord;

    public int GetSupportValue() => supportValue;

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch)
    {
        throw new System.NotImplementedException();
    }
}
