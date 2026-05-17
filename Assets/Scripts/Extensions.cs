public static class Extensions
{
    public static Coord ShiftX(this Coord coord, int delta) => new(coord.x + delta, coord.y);

    public static Coord ShiftY(this Coord coord, int delta) => new(coord.x, coord.y + delta);
}
