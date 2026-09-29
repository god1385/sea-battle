using System;
using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class RandomShipPlacer : IShipPlacer
    {
        private const int AttemptsPerShip = 80;

        /// <summary>
        /// Places every ship so that ships do not overlap or touch, including diagonally.
        /// Returns false and may leave a partial fleet when a later ship does not fit.
        /// </summary>
        public bool TryPlace(Board board, IReadOnlyList<int> shipLengths, Random random)
        {
            var lengths = LongestFirst(shipLengths);
            for (var i = 0; i < lengths.Length; i++)
            {
                if (!TryPlaceShip(board, lengths[i], random))
                    return false;
            }

            return true;
        }

        private static bool TryPlaceShip(Board board, int length, Random random)
        {
            for (var attempt = 0; attempt < AttemptsPerShip; attempt++)
            {
                var horizontal = random.Next(0, 2) == 0;
                if (!Fits(board, length, horizontal))
                    continue;

                if (board.TryPlaceShip(BuildCells(board, length, horizontal, random)))
                    return true;
            }

            return false;
        }

        private static bool Fits(Board board, int length, bool horizontal) =>
            horizontal ? length <= board.Width : length <= board.Height;

        private static CellCoord[] BuildCells(Board board, int length, bool horizontal, Random random)
        {
            var maxX = horizontal ? board.Width - length : board.Width - 1;
            var maxY = horizontal ? board.Height - 1 : board.Height - length;
            var originX = random.Next(0, maxX + 1);
            var originY = random.Next(0, maxY + 1);
            var cells = new CellCoord[length];
            for (var i = 0; i < length; i++)
            {
                var x = horizontal ? originX + i : originX;
                var y = horizontal ? originY : originY + i;
                cells[i] = new CellCoord(x, y);
            }

            return cells;
        }

        /// <summary>
        /// Longer ships go down first so a cramped board still has room for them.
        /// </summary>
        private static int[] LongestFirst(IReadOnlyList<int> shipLengths)
        {
            var lengths = new int[shipLengths.Count];
            for (var i = 0; i < shipLengths.Count; i++)
                lengths[i] = shipLengths[i];

            Array.Sort(lengths, (left, right) => right.CompareTo(left));
            return lengths;
        }
    }
}
