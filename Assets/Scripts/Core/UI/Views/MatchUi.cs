using System;
using SeaBattle.Common.Config;

namespace SeaBattle.Core.UI.Views
{
    public class MatchUi : IDisposable
    {
        private readonly PlayerBoardView _first;
        private readonly PlayerBoardView _second;

        public MatchUi(MatchScreenRefs screen, GameConfig config, BoardArt art)
        {
            screen.ApplyConfig(config.MessageLogEnabled, config.DeliveryDelayMilliseconds, config.MessageLossPercent);
            Hud = new MatchHudView(screen);
            _first = new PlayerBoardView(screen.First, art);
            _second = new PlayerBoardView(screen.Second, art);
        }

        public IMatchHudView Hud { get; }

        public IPlayerBoardView First => _first;

        public IPlayerBoardView Second => _second;

        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }
    }
}
