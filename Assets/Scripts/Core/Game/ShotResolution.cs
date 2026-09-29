using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class ShotResolution
    {
        public ShotResolution(ShotKind kind, IReadOnlyList<CellCoord> cells)
        {
            Kind = kind;
            Cells = Copy(cells);
        }

        public ShotKind Kind { get; }

        public IReadOnlyList<CellCoord> Cells { get; }

        private static CellCoord[] Copy(IReadOnlyList<CellCoord> cells)
        {
            var copy = new CellCoord[cells.Count];
            for (var i = 0; i < cells.Count; i++)
                copy[i] = cells[i];

            return copy;
        }
    }
}
