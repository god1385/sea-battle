using System;
using SeaBattle.Common.Config;
using UnityEngine;
using Zenject;

namespace SeaBattle.Core.UI.Views
{
    public class MatchUi : IDisposable
    {
        private readonly PlayerBoardView _first;
        private readonly PlayerBoardView _second;

        /// <summary>
        /// Builds both player columns and the shared debug bar under the scene context.
        /// </summary>
        public MatchUi(GameConfig config, BoardArt art, SceneContext scene)
        {
            var hud = new MatchHudView(scene.transform, config.MessageLogEnabled);
            Hud = hud;
            _first = new PlayerBoardView(hud.FirstColumn, "Игрок 1", config.DeliveryDelayMilliseconds, art);
            _second = new PlayerBoardView(hud.SecondColumn, "Игрок 2", config.DeliveryDelayMilliseconds, art);
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
