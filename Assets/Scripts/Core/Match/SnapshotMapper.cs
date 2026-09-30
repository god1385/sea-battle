using SeaBattle.Common.Messages;
using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public static class SnapshotMapper
    {
        public static SnapshotMessage FromView(PlayerView view) =>
            new SnapshotMessage
            {
                Width = view.OwnCells.GetLength(0),
                Height = view.OwnCells.GetLength(1),
                CurrentTurn = (int)view.CurrentTurn,
                Phase = (int)view.Phase,
                Winner = view.Winner.HasValue ? (int)view.Winner.Value : -1,
                OwnCells = Flatten(view.OwnCells, view.OwnHulls),
                EnemyCells = Flatten(view.EnemyCells, view.EnemyHulls)
            };

        private static CellDto[] Flatten(CellMark[,] cells, int[,] hulls)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            var result = new CellDto[width * height];
            var index = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    result[index] = new CellDto
                    {
                        X = x,
                        Y = y,
                        Mark = (int)cells[x, y],
                        Hull = hulls[x, y]
                    };
                    index++;
                }
            }

            return result;
        }
    }
}
