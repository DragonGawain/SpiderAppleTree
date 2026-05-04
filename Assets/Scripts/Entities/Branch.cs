using System;

[Serializable]
public class Branch : IWalkable
{
    // Y is NOT readonly! Branches can fall!
    public Coord coord;
    public readonly int length;
    public readonly Direction direction;

    int supportLevel;

    public bool CanWalkHorizontal(int y) => true;
}
