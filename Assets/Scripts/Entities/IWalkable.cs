public enum Direction
{
    LEFT,
    RIGHT
}

public interface IWalkable
{
    bool CanWalkHorizontal(int y) => false;
    bool CanWalkVertical() => false;
}
