public enum SupportType
{
    TRUNK_SUPPORT,
    WEB_REINFORCEMENT,
    HANGIN_WEB_STRING
}

public interface ISupportDTO
{
    public int GetSupportValue();
    public Coord GetCoord();
    public SupportType GetSupportType();
}
