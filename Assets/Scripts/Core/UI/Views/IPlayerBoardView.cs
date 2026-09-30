using System;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UniRx;
using UnityEngine;

namespace SeaBattle.Core.UI.Views
{
    public interface IPlayerBoardView
    {
        /// <summary>
        /// Fires when the player clicks an enemy cell.
        /// </summary>
        public IObservable<CellCoord> EnemyShot { get; }

        /// <summary>
        /// Fires when the player presses disconnect.
        /// </summary>
        public IObservable<Unit> Disconnect { get; }

        /// <summary>
        /// Fires when the player presses connect.
        /// </summary>
        public IObservable<Unit> Connect { get; }

        /// <summary>
        /// Fires with the raw delay field when editing finishes.
        /// </summary>
        public IObservable<string> DelaySubmitted { get; }

        /// <summary>
        /// Paints the column from the latest client state.
        /// </summary>
        public void Render(ClientBoardState state);
    }
}
