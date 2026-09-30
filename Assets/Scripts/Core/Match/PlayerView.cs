using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public class PlayerView
    {
        public PlayerView(
            CellMark[,] ownCells,
            CellMark[,] enemyCells,
            int[,] ownHulls,
            int[,] enemyHulls,
            PlayerId currentTurn,
            MatchPhase phase,
            PlayerId? winner,
            int turnSecondsLeft,
            bool turnPaused)
        {
            OwnCells = ownCells;
            EnemyCells = enemyCells;
            OwnHulls = ownHulls;
            EnemyHulls = enemyHulls;
            CurrentTurn = currentTurn;
            Phase = phase;
            Winner = winner;
            TurnSecondsLeft = turnSecondsLeft;
            TurnPaused = turnPaused;
        }

        public CellMark[,] OwnCells { get; }

        public CellMark[,] EnemyCells { get; }

        public int[,] OwnHulls { get; }

        public int[,] EnemyHulls { get; }

        public PlayerId CurrentTurn { get; }

        public MatchPhase Phase { get; }

        public PlayerId? Winner { get; }

        public int TurnSecondsLeft { get; }

        public bool TurnPaused { get; }
    }
}
