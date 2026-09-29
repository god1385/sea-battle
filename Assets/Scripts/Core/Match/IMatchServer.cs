using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public interface IMatchServer
    {
        /// <summary>
        /// Places both fleets from the seed and chooses the first player. Throws when the ships cannot be placed.
        /// </summary>
        public void Start(GameRules rules, int seed);

        /// <summary>
        /// Applies a shot for the current player, then passes the turn. A hit does not grant an extra turn.
        /// A repeated requestId returns the stored response and does not change the board.
        /// </summary>
        public ShotResponse TryShoot(PlayerId player, CellCoord cell, int requestId);

        /// <summary>
        /// Returns the caller's own ships and only that player's shots on the enemy board.
        /// </summary>
        public PlayerView GetView(PlayerId player);
    }
}
