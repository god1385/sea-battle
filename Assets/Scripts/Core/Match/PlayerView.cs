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
            PlayerId currentTurn,
            MatchPhase phase,
            PlayerId? winner)
        {
            OwnCells = ownCells;
            EnemyCells = enemyCells;
            CurrentTurn = currentTurn;
            Phase = phase;
            Winner = winner;
        }

        public CellMark[,] OwnCells { get; }

        public CellMark[,] EnemyCells { get; }

        public PlayerId CurrentTurn { get; }

        public MatchPhase Phase { get; }

        public PlayerId? Winner { get; }
    }
}
