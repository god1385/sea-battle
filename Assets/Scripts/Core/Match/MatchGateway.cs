using System;
using SeaBattle.Common.Messages;
using SeaBattle.Core.Game;
using SeaBattle.Core.Network;
using SeaBattle.Utilities.Json;
using UniRx;
using UnityEngine;

namespace SeaBattle.Core.Match
{
    public class MatchGateway : IDisposable
    {
        private readonly IMatchServer _server;
        private readonly IServerEndpoint _first;
        private readonly IServerEndpoint _second;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public MatchGateway(IMatchServer server, IServerEndpoint first, IServerEndpoint second)
        {
            _server = server;
            _first = first;
            _second = second;
            _first.Incoming.Subscribe(message => OnMessage(PlayerId.First, message)).AddTo(_subscriptions);
            _second.Incoming.Subscribe(message => OnMessage(PlayerId.Second, message)).AddTo(_subscriptions);
        }

        public void Start(GameRules rules, int seed) => _server.Start(rules, seed);

        public void Dispose() => _subscriptions.Dispose();

        private void OnMessage(PlayerId player, IncomingMessage message)
        {
            if (message.Type == MessageTypes.Join)
            {
                SendSnapshot(player);
                return;
            }

            if (message.Type != MessageTypes.Shoot)
                return;

            var shot = JsonUtility.FromJson<ShootMessage>(message.Payload);
            var response = _server.TryShoot(player, new CellCoord(shot.X, shot.Y), shot.RequestId);
            if (response.IsAccepted)
            {
                SendSnapshot(PlayerId.First);
                SendSnapshot(PlayerId.Second);
            }

            Endpoint(player).Send(WireJson.Pack(MessageTypes.ShotAck, new ShotAckMessage
            {
                RequestId = shot.RequestId,
                Accepted = response.IsAccepted,
                RejectReason = response.RejectReason.HasValue ? (int)response.RejectReason.Value : -1
            }));
        }

        private void SendSnapshot(PlayerId player)
        {
            var message = SnapshotMapper.FromView(_server.GetView(player));
            Endpoint(player).Send(WireJson.Pack(MessageTypes.Snapshot, message));
        }

        private IServerEndpoint Endpoint(PlayerId player) => player == PlayerId.First ? _first : _second;
    }
}
