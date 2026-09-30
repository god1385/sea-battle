using System;
using SeaBattle.Common.Messages;
using SeaBattle.Core.Game;
using SeaBattle.Core.Match;
using SeaBattle.Core.Network;
using SeaBattle.Utilities.Json;
using UniRx;
using UnityEngine;

namespace SeaBattle.Core.Client
{
    public class ClientSession : IDisposable
    {
        private readonly IClientChannel _channel;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly ReactiveProperty<ClientBoardState> _state;

        private CellMark[,] _own = new CellMark[0, 0];
        private CellMark[,] _enemy = new CellMark[0, 0];
        private int _width;
        private int _height;
        private bool _connected;
        private bool _hasSnapshot;
        private bool _waiting;
        private bool _hasReject;
        private int _pendingX = -1;
        private int _pendingY = -1;
        private int _pendingRequestId;
        private int _nextRequestId = 1;
        private PlayerId _currentTurn;
        private MatchPhase _phase = MatchPhase.NotStarted;
        private PlayerId? _winner;
        private string _rejectText = string.Empty;

        /// <summary>
        /// Listens to one channel. The session never reads the server's boards.
        /// </summary>
        public ClientSession(PlayerId player, IClientChannel channel)
        {
            Player = player;
            _channel = channel;
            _state = new ReactiveProperty<ClientBoardState>(BuildState());
            _channel.Connection.Subscribe(OnConnection).AddTo(_subscriptions);
            _channel.Incoming.Subscribe(OnIncoming).AddTo(_subscriptions);
        }

        public PlayerId Player { get; }

        public IReadOnlyReactiveProperty<ClientBoardState> State => _state;

        /// <summary>
        /// Connects and asks the server for the current visible state.
        /// </summary>
        public void Connect() => _channel.Connect();

        /// <summary>
        /// Closes the channel. An unconfirmed shot is sent again with the same request id after reconnect.
        /// </summary>
        public void Disconnect() => _channel.Disconnect();

        /// <summary>
        /// Changes how long this client's messages stay in flight.
        /// </summary>
        public void SetDeliveryDelay(int milliseconds) => _channel.DeliveryDelayMilliseconds = milliseconds;

        /// <summary>
        /// Sends one shot and blocks further clicks until the server answers.
        /// </summary>
        public void Shoot(int x, int y)
        {
            if (!CanShoot(x, y))
                return;

            _waiting = true;
            _pendingX = x;
            _pendingY = y;
            _pendingRequestId = _nextRequestId++;
            _hasReject = false;
            SendShoot();
            Publish();
        }

        /// <summary>
        /// Stops listening so a scene reload cannot update a destroyed view.
        /// </summary>
        public void Dispose()
        {
            _subscriptions.Dispose();
            _state.Dispose();
        }

        private void OnConnection(bool connected)
        {
            _connected = connected;
            if (connected)
            {
                _channel.Send(WireJson.Pack(MessageTypes.Join, new JoinMessage()));
                if (_waiting)
                    SendShoot();
            }

            Publish();
        }

        private void OnIncoming(IncomingMessage message)
        {
            if (message.Type == MessageTypes.Snapshot)
                ApplySnapshot(message.Payload);
            else if (message.Type == MessageTypes.ShotAck)
                ApplyAck(message.Payload);
        }

        private void ApplySnapshot(string payload)
        {
            var message = JsonUtility.FromJson<SnapshotMessage>(payload);
            _width = message.Width;
            _height = message.Height;
            _own = Read(message.OwnCells, message.Width, message.Height);
            _enemy = Read(message.EnemyCells, message.Width, message.Height);
            _currentTurn = (PlayerId)message.CurrentTurn;
            _phase = (MatchPhase)message.Phase;
            _winner = message.Winner < 0 ? (PlayerId?)null : (PlayerId)message.Winner;
            _hasSnapshot = true;
            _hasReject = false;
            Publish();
        }

        private void ApplyAck(string payload)
        {
            var ack = JsonUtility.FromJson<ShotAckMessage>(payload);
            if (ack.RequestId != _pendingRequestId)
                return;

            _waiting = false;
            if (!ack.Accepted)
            {
                _hasReject = true;
                _rejectText = RejectText(ack.RejectReason);
            }

            Publish();
        }

        private void SendShoot()
        {
            _channel.Send(WireJson.Pack(MessageTypes.Shoot, new ShootMessage
            {
                X = _pendingX,
                Y = _pendingY,
                RequestId = _pendingRequestId
            }));
        }

        private bool CanShoot(int x, int y)
        {
            if (!_connected || _waiting || !_hasSnapshot || _phase != MatchPhase.InProgress)
                return false;

            if (_currentTurn != Player)
                return false;

            if (x < 0 || y < 0 || x >= _width || y >= _height)
                return false;

            return _enemy[x, y] == CellMark.Unknown;
        }

        private void Publish() => _state.Value = BuildState();

        private ClientBoardState BuildState() => new ClientBoardState(
            Player,
            _connected,
            _waiting,
            _hasSnapshot,
            _width,
            _height,
            _own,
            _enemy,
            _currentTurn,
            _phase,
            _winner,
            _pendingX,
            _pendingY,
            BuildStatus());

        private string BuildStatus()
        {
            if (!_connected)
                return "Нет соединения";

            if (_waiting)
                return "Выстрел отправлен";

            if (_phase == MatchPhase.Finished)
                return _winner == Player ? "Победа" : "Поражение";

            if (_hasReject)
                return _rejectText;

            if (!_hasSnapshot)
                return "Ожидание партии";

            return _currentTurn == Player ? "Ваш ход" : "Ход соперника";
        }

        private static string RejectText(int reason) => reason switch
        {
            (int)ShotRejectReason.NotYourTurn => "Сервер отклонил: не ваш ход",
            (int)ShotRejectReason.AlreadyShot => "Сервер отклонил: клетка уже обстреляна",
            (int)ShotRejectReason.MatchNotRunning => "Сервер отклонил: партия не идёт",
            (int)ShotRejectReason.OutOfBounds => "Сервер отклонил: выстрел вне поля",
            _ => "Сервер отклонил выстрел"
        };

        private static CellMark[,] Read(CellDto[] cells, int width, int height)
        {
            var marks = new CellMark[width, height];
            for (var i = 0; i < cells.Length; i++)
                marks[cells[i].X, cells[i].Y] = (CellMark)cells[i].Mark;

            return marks;
        }
    }
}
