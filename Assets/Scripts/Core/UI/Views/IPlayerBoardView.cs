using System;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UniRx;

namespace SeaBattle.Core.UI.Views
{
    public interface IPlayerBoardView
    {
        public IObservable<CellCoord> EnemyShot { get; }

        public IObservable<Unit> Disconnect { get; }

        public IObservable<Unit> Connect { get; }

        public IObservable<string> DelaySubmitted { get; }

        public IObservable<string> LossSubmitted { get; }

        public void Render(ClientBoardState state);
    }
}
