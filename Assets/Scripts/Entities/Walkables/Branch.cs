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

    /// <summary>
    /// This is the amount of weight that the source of a branch can hold.
    /// </summary>
    public const int MAX_BRANCH_SUPPORT = 6;

    public readonly float[] supports;

    public Branch(Coord coord, int length, Direction direction, int supportLevel)
    {
        this.coord = coord;
        this.length = length;
        this.direction = direction;
        this.supportLevel = supportLevel;

        supports = new float[length];

        supports[0] = MAX_BRANCH_SUPPORT;
        for (int i = 1; i < length; i++)
            supports[i] = supports[i - 1] - 1;

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

    public void Interacted(int delta)
    {
        supports[0] += delta;
        // check for snaps here!
    }

    // When the player moves from one branch segment to another, this method will end up getting called twice
    /// <summary>
    /// Recalculate the amount of support that each segment of the branch has according to the new change that occured.
    /// </summary>
    /// <param name="pos">The position of the change</param>
    /// <param name="delta">The amount of change.
    /// Positive value => branch has more support (thing removed)
    /// Negative value => branch has less support (thing added)
    /// </param>
    public void RecalculateWeights(Coord pos, int delta)
    {
        int segmentIndex;
        int dir = 1;
        if (direction == Direction.LEFT)
        {
            segmentIndex = coord.x - pos.x;
            dir = -1;
        }
        else
            segmentIndex = pos.x - coord.x;

        for (int i = 0; i <= segmentIndex; i++)
        {
            supports[i] += delta;
            GameManager
                .GetManagerSingleton()
                .GetComponent<LevelManager>()
                .UpdateWeightMap(new Coord(coord.x + (i * dir), coord.y), supports[i]);
        }
    }
}
