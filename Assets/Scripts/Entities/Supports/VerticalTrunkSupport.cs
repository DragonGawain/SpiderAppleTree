/// <summary>
/// This support is for trunk supports where the trunk segment is directly underneath the branch that it is supporting.
/// </summary>
public class VerticalTrunkSupport : ISupport
{
    readonly Branch branch;
    readonly Coord coord;
    readonly int supportValue;

    public VerticalTrunkSupport(
        Branch branch,
        TrunkSupportDTO dto,
        bool suppressRecalculation = false
    )
    {
        this.branch = branch;
        this.coord = dto.coord;
        this.supportValue = dto.supportValue;
        branch.OnBranchSnap += OnSnap;

        branch.UpdateSupports(this, suppressRecalculation);
    }

    public VerticalTrunkSupport(Coord coord, int supportValue, bool suppressRecalculation = false)
    {
        this.coord = coord;
        this.supportValue = supportValue;

        branch = (Branch)LevelManager.GetActiveLevel().GetWalkables()[coord];
        branch.OnBranchSnap += OnSnap;

        branch.UpdateSupports(this, suppressRecalculation);
    }

    public int GetSupportValue() => supportValue;

    public Coord GetCoord() => coord;

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch)
    {
        // if this support is within the requested bounds, remove it from the list to avoid duplication.
        // If applicable, the new branch will create a new TrunkSupport.
        if (coord.y == lcrd.y && lcrd.x <= coord.x && rcrd.x >= coord.x)
            this.branch.RemoveSupport(this);
    }
}
