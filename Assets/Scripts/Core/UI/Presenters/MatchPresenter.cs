using System;
using SeaBattle.Common.Debug;
using SeaBattle.Core.Client;
using SeaBattle.Core.Match;
using SeaBattle.Core.UI.Views;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace SeaBattle.Core.UI.Presenters
{
    public class MatchPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly MatchUi _ui;
        private readonly NetworkLog _log;
        private readonly ClientSession _first;
        private readonly ClientSession _second;

        public MatchPresenter(
            MatchUi ui,
            NetworkLog log,
            [Inject(Id = SeatId.First)] ClientSession first,
            [Inject(Id = SeatId.Second)] ClientSession second)
        {
            _ui = ui;
            _log = log;
            _first = first;
            _second = second;
        }

        public void Initialize()
        {
            Bind(_ui.First, _first);
            Bind(_ui.Second, _second);
            _ui.Hud.Restart
                .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex))
                .AddTo(_subscriptions);
            _ui.Hud.LogToggle.Subscribe(enabled => _log.Enabled = enabled).AddTo(_subscriptions);
            _log.Lines.Subscribe(_ui.Hud.ShowLog).AddTo(_subscriptions);
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            _ui.Dispose();
        }

        private void Bind(IPlayerBoardView view, ClientSession session)
        {
            session.State.Subscribe(view.Render).AddTo(_subscriptions);
            view.EnemyShot.Subscribe(cell => session.Shoot(cell.X, cell.Y)).AddTo(_subscriptions);
            view.Disconnect.Subscribe(_ => session.Disconnect()).AddTo(_subscriptions);
            view.Connect.Subscribe(_ => session.Connect()).AddTo(_subscriptions);
            view.DelaySubmitted.Subscribe(text => ApplyDelay(session, text)).AddTo(_subscriptions);
            view.LossSubmitted.Subscribe(text => ApplyLoss(session, text)).AddTo(_subscriptions);
        }

        private static void ApplyDelay(ClientSession session, string text)
        {
            if (int.TryParse(text, out var milliseconds))
                session.SetDeliveryDelay(milliseconds);
        }

        private static void ApplyLoss(ClientSession session, string text)
        {
            if (int.TryParse(text, out var percent))
                session.SetLossPercent(percent);
        }
    }
}
