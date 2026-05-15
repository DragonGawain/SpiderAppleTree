using UnityEngine;

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

    // public SupportType GetSupportType()
    // {
    //     return SupportType.ADDITIVE;
    // }

    public int GetSupportValue() => supportValue;

    public Coord GetCoord() => coord;

    public void OnSnap(Coord coord)
    {
        throw new System.NotImplementedException();
    }
}

public readonly struct TrunkSupportDTO
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
}
