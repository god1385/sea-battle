using System;
using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public interface IShipPlacer
    {
        public bool TryPlace(Board board, IReadOnlyList<int> shipLengths, Random random);
    }
}
