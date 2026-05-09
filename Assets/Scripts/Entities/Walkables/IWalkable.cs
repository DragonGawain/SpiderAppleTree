using Unity.Mathematics;

public enum Direction
{
    LEFT,
    UP,
    RIGHT,
    DOWN
}

public interface IWalkable : ILevelElement
{
    /// <summary>
    /// Determine if a walkable can be stepped on horizontally.
    /// This method is called when the player attemptsto walk onto this cell horizontally.
    /// You can always exit a cell in any direction.
    /// </summary>
    /// <param name="y">The height of the walkable</param>
    /// <param name="d">The direction that player is in relative to the walkable.
    /// This means that if the player is moving to the left, the direction will be RIGHT.
    /// This odd behaviour comes about from the definition - "is the right side horizontally walkable?"
    /// This question only needs to be answered when the player is coming from the right side.
    /// In other words, when the player is moving to the left.</param>
    /// <returns>True if the horizontal movement is legal. False otherwise</returns>
    bool CanWalkHorizontal(int y, Direction d) => false;
    bool CanWalkVertical(int x, Direction d) => false;
}
