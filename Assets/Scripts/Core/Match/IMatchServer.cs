using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public interface IMatchServer
    {
        public void Start(GameRules rules, int seed);

        public ShotResponse TryShoot(PlayerId player, CellCoord cell, int requestId);

        public PlayerView GetView(PlayerId player);
    }
}
