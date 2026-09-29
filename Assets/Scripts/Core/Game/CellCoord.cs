namespace SeaBattle.Core.Game
{
    public readonly struct CellCoord
    {
        public CellCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }
    }
}
