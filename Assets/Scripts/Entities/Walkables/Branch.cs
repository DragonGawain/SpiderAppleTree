// It is important that trunks get loaded before branches.
// This is because when a branch is created, it will check to see if its source is next to a trunk.
// If it is, then it sets that trunk segment to be horizontally walkable.
public class Branch : IWalkable
{
    enum SupportSides
    {
        LEFT,
        RIGHT,
        BOTH
    }

    // Y is NOT readonly! Branches can fall!
    // Y IS in fact readonly. If a branch falls, I will create a new branch instance.
    public Coord coord;
    public readonly int length;

    // A branch is a single object. The direction determines in what direction it extends.
    public readonly Direction direction;
    public int supportLevel;

    public const int BRANCH_SUPPORT = 5;

    public Branch(Coord coord, int length, Direction direction, int supportLevel)
    {
        this.coord = coord;
        this.length = length;
        this.direction = direction;
        this.supportLevel = supportLevel;

        if (SaveManager.GetCreatingLevel())
        {
            SaveManager.GetNewLevel().AddNewElementToLevel(this);
            if (direction == Direction.RIGHT)
                for (int i = coord.x; i < coord.x + length; i++)
                    SaveManager.GetNewLevel().AddWalkable(new Coord(i, coord.y), this);
            else
                for (int i = coord.x; i > coord.x - length; i--)
                    SaveManager.GetNewLevel().AddWalkable(new Coord(i, coord.y), this);
        }
        else
        {
            LevelManager.GetActiveLevel().AddNewElementToLevel(this);
            if (direction == Direction.RIGHT)
                for (int i = coord.x; i < coord.x + length; i++)
                    LevelManager.GetActiveLevel().AddWalkable(new Coord(i, coord.y), this);
            else
                for (int i = coord.x; i > coord.x - length; i--)
                    LevelManager.GetActiveLevel().AddWalkable(new Coord(i, coord.y), this);
        }

        // TODO:: Have this constructor spawn in a prefab at the desired location
        // Prefab would only be used for animations. But, animated tiles exist, so I might use those instead.
    }

    public Branch(int x, int y, int length, Direction direction, int supportLevel)
        : this(new Coord(x, y), length, direction, supportLevel) { }

    public Branch(int x, int y, int length, int direction, int supportLevel)
        : this(new Coord(x, y), length, (Direction)direction, supportLevel) { }

    public Branch(Coord coord, int length, int direction, int supportLevel)
        : this(coord, length, (Direction)direction, supportLevel) { }

    public bool CanWalkHorizontal(int y, Direction d) => true;
}
