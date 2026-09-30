using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class Ship
    {
        private readonly CellCoord[] _cells;
        private int _hitCount;

        public Ship(IReadOnlyList<CellCoord> cells, HullKind hull)
        {
            _cells = new CellCoord[cells.Count];
            for (var i = 0; i < cells.Count; i++)
                _cells[i] = cells[i];

            Hull = hull;
        }

        public IReadOnlyList<CellCoord> Cells => _cells;

        public HullKind Hull { get; }

        public int Length => _cells.Length;

        public bool IsSunk => _hitCount >= _cells.Length;

        public void RegisterHit() => _hitCount++;
    }
}
