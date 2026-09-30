using SeaBattle.Core.Game;
using SeaBattle.Core.Match;

namespace SeaBattle.Core.Client
{
    public class ClientBoardState
    {
        /// <summary>
        /// One rendered frame: the latest server snapshot plus whether a shot is still waiting.
        /// </summary>
        public ClientBoardState(
            PlayerId player,
            bool isConnected,
            bool isWaiting,
            bool hasSnapshot,
            int width,
            int height,
            CellMark[,] ownCells,
            CellMark[,] enemyCells,
            int[,] ownHulls,
            int[,] enemyHulls,
            PlayerId currentTurn,
            MatchPhase phase,
            PlayerId? winner,
            int pendingX,
            int pendingY,
            string status)
        {
            Player = player;
            IsConnected = isConnected;
            IsWaiting = isWaiting;
            HasSnapshot = hasSnapshot;
            Width = width;
            Height = height;
            OwnCells = ownCells;
            EnemyCells = enemyCells;
            OwnHulls = ownHulls;
            EnemyHulls = enemyHulls;
            CurrentTurn = currentTurn;
            Phase = phase;
            Winner = winner;
            PendingX = pendingX;
            PendingY = pendingY;
            Status = status;
        }

        public PlayerId Player { get; }

        public bool IsConnected { get; }

        public bool IsWaiting { get; }

        public bool HasSnapshot { get; }

        public int Width { get; }

        public int Height { get; }

        public CellMark[,] OwnCells { get; }

        public CellMark[,] EnemyCells { get; }

        public int[,] OwnHulls { get; }

        public int[,] EnemyHulls { get; }

        public PlayerId CurrentTurn { get; }

        public MatchPhase Phase { get; }

        public PlayerId? Winner { get; }

        public int PendingX { get; }

        public int PendingY { get; }

        public string Status { get; }

        /// <summary>
        /// True when this player may fire at that enemy cell.
        /// </summary>
        public bool CanShoot(CellMark mark) =>
            IsConnected
            && !IsWaiting
            && HasSnapshot
            && Phase == MatchPhase.InProgress
            && CurrentTurn == Player
            && mark == CellMark.Unknown;
    }
}
