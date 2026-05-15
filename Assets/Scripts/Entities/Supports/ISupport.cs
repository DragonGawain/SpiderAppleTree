// public enum SupportType
// {
//     ADDITIVE,
//     MULTIPLICATIVE,
// }

public interface ISupport : ILevelElement
{
    // public SupportType GetSupportType();
    public int GetSupportValue();
    public Coord GetCoord();

    public void OnSnap(Coord coord);

    public const int TRUNK_SUPPORT = 6;
}
