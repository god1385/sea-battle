using System;
using NUnit.Framework;
using UniRx;
using SeaBattle.Common.Messages;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using SeaBattle.Core.Match;
using SeaBattle.Core.Network;
using SeaBattle.Utilities.Json;

namespace SeaBattle.Tests.Game
{
    public class ClientLinkTests
    {
        [Test]
        public void Link_DeliversMessagesInOrderAfterTheDelay()
        {
            var link = new InProcessLink("Игрок 1", 500, _ => { });
            var received = new System.Collections.Generic.List<string>();
            link.Server.Incoming.Subscribe(message => received.Add(message.Type));
            link.Client.Connect();
            link.Client.Send(WireJson.Pack(MessageTypes.Join, new JoinMessage()));
            link.Client.Send(WireJson.Pack(MessageTypes.Shoot, new ShootMessage()));

            link.Tick(0.2f);
            Assert.That(received, Is.Empty);

            link.Tick(0.4f);
            Assert.That(received, Is.EqualTo(new[] { MessageTypes.Join, MessageTypes.Shoot }));
            link.Dispose();
            Assert.DoesNotThrow(() => link.Tick(1f));
        }

        [Test]
        public void Link_DisconnectDropsMessagesStillInFlight()
        {
            var link = new InProcessLink("Игрок 1", 1000, _ => { });
            var received = 0;
            link.Server.Incoming.Subscribe(_ => received++);
            link.Client.Connect();
            link.Client.Send(WireJson.Pack(MessageTypes.Join, new JoinMessage()));
            link.Client.Disconnect();

            link.Tick(3f);

            Assert.That(received, Is.EqualTo(0));
            link.Dispose();
        }

        [Test]
        public void Client_SeesOwnShipsAndNotTheEnemyFleet()
        {
            using var table = new MatchTable(4, 0);
            table.ConnectBoth();
            table.Tick();

            var state = table.First.State.Value;
            Assert.That(Count(state.OwnCells, CellMark.Ship), Is.EqualTo(8));
            Assert.That(Count(state.EnemyCells, CellMark.Ship), Is.EqualTo(0));
            Assert.That(Count(state.EnemyCells, CellMark.Unknown), Is.EqualTo(36));
        }

        [Test]
        public void Client_SecondClickWhileWaiting_IsNotSent()
        {
            using var table = new MatchTable(8, 0);
            table.ConnectBoth();
            table.Tick();
            table.SetDelay(5000);

            var shots = 0;
            var shooter = table.Current;
            table.Link(shooter.Player).Server.Incoming.Subscribe(message =>
            {
                if (message.Type == MessageTypes.Shoot)
                    shots++;
            });

            shooter.Shoot(0, 0);
            shooter.Shoot(2, 2);
            Assert.That(shooter.State.Value.IsWaiting, Is.True);
            Assert.That(shooter.State.Value.Status, Is.EqualTo("Выстрел отправлен"));

            table.Tick(6f);
            table.Tick(6f);

            Assert.That(shots, Is.EqualTo(1));
            Assert.That(shooter.State.Value.IsWaiting, Is.False);
            Assert.That(shooter.State.Value.EnemyCells[2, 2], Is.EqualTo(CellMark.Unknown));
        }

        [Test]
        public void Server_RejectsShotWhenItIsNotThatPlayersTurn()
        {
            using var table = new MatchTable(6, 0);
            table.ConnectBoth();
            table.Tick();
            var shooter = table.Current;
            shooter.Shoot(0, 0);
            table.Tick();
            var afterFirst = Copy(shooter.State.Value.EnemyCells);

            ShotAckMessage ack = null;
            table.Link(shooter.Player).Client.Incoming.Subscribe(message =>
            {
                if (message.Type == MessageTypes.ShotAck)
                    ack = UnityEngine.JsonUtility.FromJson<ShotAckMessage>(message.Payload);
            });
            table.Link(shooter.Player).Client.Send(WireJson.Pack(MessageTypes.Shoot, new ShootMessage
            {
                X = 1,
                Y = 1,
                RequestId = 99
            }));
            table.Tick();

            Assert.That(ack.Accepted, Is.False);
            Assert.That(ack.RejectReason, Is.EqualTo((int)ShotRejectReason.NotYourTurn));
            AssertSame(shooter.State.Value.EnemyCells, afterFirst);
        }

        [Test]
        public void Client_ReconnectResendsTheLostShotOnce()
        {
            var normal = ShootOnce(15);
            using var dropped = new MatchTable(15, 0);
            dropped.ConnectBoth();
            dropped.Tick();
            dropped.SetDelay(2000);
            var shooter = dropped.Current;
            shooter.Shoot(0, 0);
            dropped.Disconnect(shooter.Player);
            dropped.Tick(3f);

            Assert.That(dropped.Session(shooter.Player).State.Value.EnemyCells[0, 0], Is.EqualTo(CellMark.Unknown));

            dropped.Connect(shooter.Player);
            dropped.Tick(3f);
            dropped.Tick(3f);

            AssertSame(dropped.Session(shooter.Player).State.Value.EnemyCells, normal);
        }

        private static CellMark[,] ShootOnce(int seed)
        {
            using var table = new MatchTable(seed, 0);
            table.ConnectBoth();
            table.Tick();
            var shooter = table.Current;
            shooter.Shoot(0, 0);
            table.Tick();
            return Copy(shooter.State.Value.EnemyCells);
        }

        private static int Count(CellMark[,] cells, CellMark mark)
        {
            var count = 0;
            for (var x = 0; x < cells.GetLength(0); x++)
            {
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    if (cells[x, y] == mark)
                        count++;
                }
            }

            return count;
        }

        private static CellMark[,] Copy(CellMark[,] cells)
        {
            var copy = new CellMark[cells.GetLength(0), cells.GetLength(1)];
            Array.Copy(cells, copy, cells.Length);
            return copy;
        }

        private static void AssertSame(CellMark[,] left, CellMark[,] right)
        {
            Assert.That(left.GetLength(0), Is.EqualTo(right.GetLength(0)));
            Assert.That(left.GetLength(1), Is.EqualTo(right.GetLength(1)));
            for (var x = 0; x < left.GetLength(0); x++)
            {
                for (var y = 0; y < left.GetLength(1); y++)
                    Assert.That(left[x, y], Is.EqualTo(right[x, y]));
            }
        }

        private class MatchTable : IDisposable
        {
            private readonly InProcessLink _firstLink;
            private readonly InProcessLink _secondLink;
            private readonly MatchGateway _gateway;

            public MatchTable(int seed, int delay)
            {
                _firstLink = new InProcessLink("Игрок 1", delay, _ => { });
                _secondLink = new InProcessLink("Игрок 2", delay, _ => { });
                var server = new MatchServer(new RandomShipPlacer());
                _gateway = new MatchGateway(server, _firstLink.Server, _secondLink.Server);
                _gateway.Start(new GameRules(6, 6, new[] { 3, 2, 2, 1 }), seed);
                First = new ClientSession(PlayerId.First, _firstLink.Client);
                Second = new ClientSession(PlayerId.Second, _secondLink.Client);
            }

            public ClientSession First { get; }

            public ClientSession Second { get; }

            public ClientSession Current =>
                First.State.Value.CurrentTurn == PlayerId.First ? First : Second;

            public void ConnectBoth()
            {
                First.Connect();
                Second.Connect();
            }

            public void Tick(float seconds = 0f)
            {
                _firstLink.Tick(seconds);
                _secondLink.Tick(seconds);
            }

            public void SetDelay(int milliseconds)
            {
                _firstLink.Client.DeliveryDelayMilliseconds = milliseconds;
                _secondLink.Client.DeliveryDelayMilliseconds = milliseconds;
            }

            public InProcessLink Link(PlayerId player) => player == PlayerId.First ? _firstLink : _secondLink;

            public ClientSession Session(PlayerId player) => player == PlayerId.First ? First : Second;

            public void Disconnect(PlayerId player) => Session(player).Disconnect();

            public void Connect(PlayerId player) => Session(player).Connect();

            public void Dispose()
            {
                First.Dispose();
                Second.Dispose();
                _gateway.Dispose();
                _firstLink.Dispose();
                _secondLink.Dispose();
            }
        }
    }
}
