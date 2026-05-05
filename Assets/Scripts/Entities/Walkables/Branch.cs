using System;

public class Branch : IWalkable
{
    public Branch(
        Coord coord,
        int length,
        Direction direction,
        int supportLevel,
        Level level = null
    )
    {
        this.coord = coord;
        this.length = length;
        this.direction = direction;
        this.supportLevel = supportLevel;

        if (level == null)
            for (int i = coord.x; i < coord.x + length; i++)
                LevelManager.GetActiveLevel().AddToWalkableDict(new Coord(i, coord.y), this);
        else
            for (int i = coord.x; i < coord.x + length; i++)
                level.AddToWalkableDict(new Coord(i, coord.y), this);
    }

    public Branch(
        int x,
        int y,
        int length,
        Direction direction,
        int supportLevel,
        Level level = null
    )
        : this(new Coord(x, y), length, direction, supportLevel) { }

    public Branch(int x, int y, int length, int direction, int supportLevel, Level level = null)
        : this(new Coord(x, y), length, (Direction)direction, supportLevel) { }

    public Branch(Coord coord, int length, int direction, int supportLevel, Level level = null)
        : this(coord, length, (Direction)direction, supportLevel) { }

    // Y is NOT readonly! Branches can fall!
    public Coord coord;
    public readonly int length;
    public readonly Direction direction;
    public int supportLevel;

    public bool CanWalkHorizontal(int y) => true;
}
