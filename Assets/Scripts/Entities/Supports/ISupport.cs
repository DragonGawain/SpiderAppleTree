public interface ISupport : ILevelElement
{
    public int GetSupportValue();
    public Coord GetCoord();

    public void OnSnap(Coord lcrd, Coord rcrd, Branch branch);

    public const int TRUNK_SUPPORT = 6;

    public SupportType GetSupportType();
}

public enum SupportType
{
    TRUNK_SUPPORT_V,
    TRUNK_SUPPORT_H,
    WEB_REINFORCEMENT,
    HANGING_WEB_STRING,
    BRANCH_SUPPORT
}
