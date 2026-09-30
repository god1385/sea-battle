using System;
using SeaBattle.Common.Config;
using UnityEngine;

namespace SeaBattle.Core.UI.Views
{
    public class MatchUi : IDisposable
    {
        private readonly PlayerBoardView _first;
        private readonly PlayerBoardView _second;

        /// <summary>
        /// Binds both player columns and the shared debug bar that are placed in the scene.
        /// </summary>
        public MatchUi(MatchScreenRefs screen, GameConfig config, BoardArt art)
        {
            screen.ApplyConfig(config.MessageLogEnabled, config.DeliveryDelayMilliseconds);
            Hud = new MatchHudView(screen);
            _first = new PlayerBoardView(screen.First, art);
            _second = new PlayerBoardView(screen.Second, art);
        }

        public IMatchHudView Hud { get; }

        public IPlayerBoardView First => _first;

        public IPlayerBoardView Second => _second;

        /// <summary>
        /// Releases the cell click streams.
        /// </summary>
        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }
    }
}
