using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public interface IMatchServer
    {
        public void Start(GameRules rules, int seed);

        public ShotResponse TryShoot(PlayerId player, CellCoord cell, int requestId);

        public bool IsTurnPaused { get; }

        public bool TickTurn(float deltaSeconds, bool firstConnected, bool secondConnected);

        public PlayerView GetView(PlayerId player);
    }
}
