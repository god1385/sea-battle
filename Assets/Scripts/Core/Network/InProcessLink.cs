using System;
using System.Collections.Generic;
using SeaBattle.Utilities.Json;
using UniRx;

namespace SeaBattle.Core.Network
{
    public class InProcessLink : IDisposable
    {
        private readonly string _label;
        private readonly Action<string> _log;
        private readonly List<Delivery> _toServer = new List<Delivery>();
        private readonly List<Delivery> _toClient = new List<Delivery>();
        private readonly Subject<IncomingMessage> _clientIncoming = new Subject<IncomingMessage>();
        private readonly Subject<IncomingMessage> _serverIncoming = new Subject<IncomingMessage>();
        private readonly BehaviorSubject<bool> _connection;

        private float _elapsedMs;
        private int _delayMs;
        private bool _clientConnected;
        private bool _disposed;

        public InProcessLink(string label, int deliveryDelayMilliseconds, Action<string> log)
        {
            _label = label;
            _log = log;
            _delayMs = deliveryDelayMilliseconds;
            _connection = new BehaviorSubject<bool>(false);
            Client = new ClientFacade(this);
            Server = new ServerFacade(this);
        }

        public IClientChannel Client { get; }

        public IServerEndpoint Server { get; }

        public void Tick(float deltaSeconds)
        {
            if (_disposed)
                return;

            _elapsedMs += deltaSeconds * 1000f;
            DeliverDue(_toServer, _serverIncoming, false);
            DeliverDue(_toClient, _clientIncoming, true);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _toServer.Clear();
            _toClient.Clear();
            _clientIncoming.Dispose();
            _serverIncoming.Dispose();
            _connection.Dispose();
        }

        private void ConnectClient()
        {
            if (_disposed || _clientConnected)
                return;

            _clientConnected = true;
            _connection.OnNext(true);
        }

        private void DisconnectClient()
        {
            if (_disposed || !_clientConnected)
                return;

            _clientConnected = false;
            Drop(_toServer, "к серверу");
            Drop(_toClient, "к клиенту");
            _connection.OnNext(false);
        }

        private void EnqueueToServer(string json)
        {
            if (_disposed || !_clientConnected)
                return;

            var envelope = WireJson.Unpack(json);
            _log($"{_label} отправил {envelope.Type}");
            _toServer.Add(new Delivery(envelope.Type, envelope.Payload, _elapsedMs + _delayMs));
        }

        private void EnqueueToClient(string json)
        {
            if (_disposed || !_clientConnected)
                return;

            var envelope = WireJson.Unpack(json);
            _toClient.Add(new Delivery(envelope.Type, envelope.Payload, _elapsedMs + _delayMs));
        }

        private void DeliverDue(List<Delivery> queue, Subject<IncomingMessage> destination, bool logAsReceived)
        {
            while (queue.Count > 0 && queue[0].DeliverAtMs <= _elapsedMs)
            {
                var item = queue[0];
                queue.RemoveAt(0);
                if (logAsReceived)
                    _log($"{_label} получил {item.Type}");

                destination.OnNext(new IncomingMessage(item.Type, item.Payload));
            }
        }

        private void Drop(List<Delivery> queue, string direction)
        {
            for (var i = 0; i < queue.Count; i++)
                _log($"{_label} потерял {queue[i].Type} ({direction})");

            queue.Clear();
        }

        private readonly struct Delivery
        {
            public Delivery(string type, string payload, float deliverAtMs)
            {
                Type = type;
                Payload = payload;
                DeliverAtMs = deliverAtMs;
            }

            public string Type { get; }

            public string Payload { get; }

            public float DeliverAtMs { get; }
        }

        private class ClientFacade : IClientChannel
        {
            private readonly InProcessLink _link;

            public ClientFacade(InProcessLink link) => _link = link;

            public bool IsConnected => _link._clientConnected;

            public int DeliveryDelayMilliseconds
            {
                get => _link._delayMs;
                set => _link._delayMs = value < 0 ? 0 : value;
            }

            public void Connect() => _link.ConnectClient();

            public void Disconnect() => _link.DisconnectClient();

            public void Send(string json) => _link.EnqueueToServer(json);

            public IObservable<IncomingMessage> Incoming => _link._clientIncoming;

            public IObservable<bool> Connection => _link._connection;
        }

        private class ServerFacade : IServerEndpoint
        {
            private readonly InProcessLink _link;

            public ServerFacade(InProcessLink link) => _link = link;

            public void Send(string json) => _link.EnqueueToClient(json);

            public IObservable<IncomingMessage> Incoming => _link._serverIncoming;
        }
    }
}
