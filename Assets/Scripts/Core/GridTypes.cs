namespace GridLearn
{
    public enum Move { Up, Down, Left, Right }
    public enum Tile { Empty, Wall, Start, Goal, Hazard, Mud }

    public static class Moves
    {
        public static readonly int Count = System.Enum.GetValues(typeof(Move)).Length;
    }
}
