using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class Ship
    {
        private readonly CellCoord[] _cells;
        private int _hitCount;

        /// <summary>
        /// Creates a ship on the given cells. The coordinates are copied.
        /// </summary>
        public Ship(IReadOnlyList<CellCoord> cells)
        {
            _cells = new CellCoord[cells.Count];
            for (var i = 0; i < cells.Count; i++)
                _cells[i] = cells[i];
        }

        public IReadOnlyList<CellCoord> Cells => _cells;

        public int Length => _cells.Length;

        public bool IsSunk => _hitCount >= _cells.Length;

        /// <summary>
        /// Records one hit. The ship sinks when every cell has been hit.
        /// </summary>
        public void RegisterHit() => _hitCount++;
    }
}
