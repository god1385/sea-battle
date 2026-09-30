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
        public int TurnSecondsLeft;
        public bool TurnPaused;
        public CellDto[] OwnCells;
        public CellDto[] EnemyCells;
    }
}
