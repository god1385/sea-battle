using System;
using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public interface IShipPlacer
    {
        /// <summary>
        /// Places every ship so that ships do not overlap or touch, including diagonally.
        /// Returns false and may leave a partial fleet when a later ship does not fit.
        /// </summary>
        public bool TryPlace(Board board, IReadOnlyList<int> shipLengths, Random random);
    }
}
