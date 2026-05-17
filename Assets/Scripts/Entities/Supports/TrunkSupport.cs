public class TrunkSupport : ISupport
{
    readonly Branch branch;
    readonly Coord coord;
    readonly int supportValue;

    public TrunkSupport(Branch branch, TrunkSupportDTO dto)
    {
        this.branch = branch;
        this.coord = dto.coord;
        this.supportValue = dto.supportValue;
        branch.OnBranchSnap += OnSnap;

        branch.UpdateSupports(this);
    }

    public TrunkSupport(Coord coord, int supportValue)
    {
        this.coord = coord;
        this.supportValue = supportValue;

        branch = (Branch)LevelManager.GetActiveLevel().GetWalkables()[coord];
        branch.OnBranchSnap += OnSnap;

        branch.UpdateSupports(this);
    }

    public int GetSupportValue() => supportValue;

    public Coord GetCoord() => coord;

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch)
    {
        // if this support is within the requested bounds, reassign the support to the part of the branch that is left behind.
        if (coord.y == lcrd.y && lcrd.x <= coord.x && rcrd.x >= coord.x)
        {
            new TrunkSupport(branch, new(coord, supportValue));
            // GameManager.DebugLog(
            //     "<color=blue>Removing trunk support at coord: " + coord + "</color>"
            // );
            this.branch.RemoveSupport(this);
        }
    }
}

public readonly struct TrunkSupportDTO : ISupportDTO
{
    public readonly Coord coord;
    public readonly int supportValue;

    public TrunkSupportDTO(Coord coord, int supportValue)
    {
        this.coord = coord;
        this.supportValue = supportValue;
    }

    public TrunkSupportDTO(TrunkSupport ts)
    {
        this.coord = ts.GetCoord();
        this.supportValue = ts.GetSupportValue();
    }

    public int GetSupportValue() => supportValue;

    public Coord GetCoord() => coord;

    public SupportType GetSupportType() => SupportType.TRUNK_SUPPORT;
}
