public enum Direction
{
    LEFT,
    RIGHT
}

public interface IWalkable : ILevelElement
{
    bool CanWalkHorizontal(int y) => false;
    bool CanWalkVertical() => false;
}
