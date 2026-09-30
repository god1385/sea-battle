using System;
using SeaBattle.Common.Config;
using SeaBattle.Core.Client;
using SeaBattle.Core.Network;
using UnityEngine;
using Zenject;

namespace SeaBattle.Core.Match
{
    public class MatchRuntime : IInitializable, ITickable, IDisposable
    {
        private readonly GameConfig _config;
        private readonly MatchGateway _gateway;
        private readonly InProcessLink _firstLink;
        private readonly InProcessLink _secondLink;
        private readonly ClientSession _firstSession;
        private readonly ClientSession _secondSession;

        public MatchRuntime(
            GameConfig config,
            MatchGateway gateway,
            [Inject(Id = SeatId.First)] InProcessLink firstLink,
            [Inject(Id = SeatId.Second)] InProcessLink secondLink,
            [Inject(Id = SeatId.First)] ClientSession firstSession,
            [Inject(Id = SeatId.Second)] ClientSession secondSession)
        {
            _config = config;
            _gateway = gateway;
            _firstLink = firstLink;
            _secondLink = secondLink;
            _firstSession = firstSession;
            _secondSession = secondSession;
        }

        public void Initialize()
        {
            _gateway.Start(_config.CreateRules(), Environment.TickCount);
            _firstSession.Connect();
            _secondSession.Connect();
        }

        public void Tick()
        {
            var delta = Time.unscaledDeltaTime;
            _firstLink.Tick(delta);
            _secondLink.Tick(delta);
        }

        public void Dispose()
        {
            _firstSession.Dispose();
            _secondSession.Dispose();
            _gateway.Dispose();
            _firstLink.Dispose();
            _secondLink.Dispose();
        }
    }
}
