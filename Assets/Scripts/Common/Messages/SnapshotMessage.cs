using System;

namespace SeaBattle.Common.Messages
{
    [Serializable]
    public class SnapshotMessage
    {
        public int Width;
        public int Height;
        public int CurrentTurn;
        public int Phase;
        public int Winner;
        public CellDto[] OwnCells;
        public CellDto[] EnemyCells;
    }
}
