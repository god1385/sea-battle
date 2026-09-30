using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public class PlayerView
    {
        /// <summary>
        /// A snapshot of one player's visible boards, the turn, and the winner.
        /// </summary>
        public PlayerView(
            CellMark[,] ownCells,
            CellMark[,] enemyCells,
            int[,] ownHulls,
            int[,] enemyHulls,
            PlayerId currentTurn,
            MatchPhase phase,
            PlayerId? winner)
        {
            OwnCells = ownCells;
            EnemyCells = enemyCells;
            OwnHulls = ownHulls;
            EnemyHulls = enemyHulls;
            CurrentTurn = currentTurn;
            Phase = phase;
            Winner = winner;
        }

        public CellMark[,] OwnCells { get; }

        public CellMark[,] EnemyCells { get; }

        public int[,] OwnHulls { get; }

        public int[,] EnemyHulls { get; }

        public PlayerId CurrentTurn { get; }

        public MatchPhase Phase { get; }

        public PlayerId? Winner { get; }
    }
}
