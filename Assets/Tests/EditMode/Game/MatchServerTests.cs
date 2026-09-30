using System;
using System.Collections.Generic;
using NUnit.Framework;
using SeaBattle.Core.Game;
using SeaBattle.Core.Match;

namespace SeaBattle.Tests.Game
{
    public class MatchServerTests
    {
        [Test]
        public void TryShoot_BeforeStart_RejectsMatchNotRunning()
        {
            var server = new MatchServer(new RandomShipPlacer());

            var response = server.TryShoot(PlayerId.First, new CellCoord(0, 0), 1);

            Assert.That(response.IsAccepted, Is.False);
            Assert.That(response.RejectReason, Is.EqualTo(ShotRejectReason.MatchNotRunning));
            Assert.Throws<InvalidOperationException>(() => server.GetView(PlayerId.First));
        }

        [Test]
        public void Start_FailsWhenShipsCannotBePlaced()
        {
            var server = new MatchServer(new FailingPlacer());

            Assert.Throws<InvalidOperationException>(() =>
                server.Start(new GameRules(6, 6, new[] { 3, 2, 2, 1 }), 1));
        }

        [Test]
        public void Start_SameSeed_PlacesTheSameFleets()
        {
            var first = Start(42);
            var second = Start(42);

            AssertSameMarks(first.GetView(PlayerId.First).OwnCells, second.GetView(PlayerId.First).OwnCells);
            AssertSameMarks(first.GetView(PlayerId.Second).OwnCells, second.GetView(PlayerId.Second).OwnCells);
            Assert.That(
                first.GetView(PlayerId.First).CurrentTurn,
                Is.EqualTo(second.GetView(PlayerId.First).CurrentTurn));
        }

        [Test]
        public void GetView_HidesEnemyShips()
        {
            var server = Start(3);
            var view = server.GetView(PlayerId.First);

            Assert.That(Count(view.OwnCells, CellMark.Ship), Is.EqualTo(8));
            Assert.That(Count(view.EnemyCells, CellMark.Ship), Is.EqualTo(0));
            Assert.That(Count(view.EnemyCells, CellMark.Unknown), Is.EqualTo(36));
        }

        [Test]
        public void TryShoot_MissAndHit_PassTheTurn()
        {
            var server = Start(5);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;
            var target = Opponent(shooter);
            var targetOwn = server.GetView(target).OwnCells;

            var miss = server.TryShoot(shooter, FindMark(targetOwn, CellMark.Empty), 1);
            Assert.That(miss.IsAccepted, Is.True);
            Assert.That(miss.ShotKind, Is.EqualTo(ShotKind.Miss));
            Assert.That(server.GetView(shooter).CurrentTurn, Is.EqualTo(target));

            var returnShot = server.TryShoot(target, FindMark(server.GetView(shooter).OwnCells, CellMark.Empty), 2);
            Assert.That(returnShot.IsAccepted, Is.True);
            Assert.That(server.GetView(shooter).CurrentTurn, Is.EqualTo(shooter));

            var hit = server.TryShoot(shooter, FindMark(server.GetView(target).OwnCells, CellMark.Ship), 3);
            Assert.That(hit.IsAccepted, Is.True);
            Assert.That(hit.ShotKind, Is.EqualTo(ShotKind.Hit).Or.EqualTo(ShotKind.Sunk));
            Assert.That(server.GetView(shooter).CurrentTurn, Is.EqualTo(target));
        }

        [Test]
        public void TryShoot_OutOfTurnAndRepeatedCell_AreRejected()
        {
            var server = Start(7);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;
            var other = Opponent(shooter);
            var cell = FindMark(server.GetView(other).OwnCells, CellMark.Empty);

            var denied = server.TryShoot(other, cell, 1);
            Assert.That(denied.IsAccepted, Is.False);
            Assert.That(denied.RejectReason, Is.EqualTo(ShotRejectReason.NotYourTurn));
            Assert.That(server.GetView(shooter).CurrentTurn, Is.EqualTo(shooter));

            var accepted = server.TryShoot(shooter, cell, 2);
            Assert.That(accepted.IsAccepted, Is.True);

            var passer = server.GetView(shooter).CurrentTurn;
            var passCell = FindMark(server.GetView(shooter).OwnCells, CellMark.Empty);
            Assert.That(server.TryShoot(passer, passCell, 3).IsAccepted, Is.True);

            var repeated = server.TryShoot(shooter, cell, 4);
            Assert.That(repeated.IsAccepted, Is.False);
            Assert.That(repeated.RejectReason, Is.EqualTo(ShotRejectReason.AlreadyShot));
        }

        [Test]
        public void TryShoot_RepeatedRequest_DoesNotChangeTheBoard()
        {
            var server = Start(9);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;
            var target = Opponent(shooter);
            var pair = FindAdjacentShip(server.GetView(target).OwnCells);

            var first = server.TryShoot(shooter, pair.First, 42);
            Assert.That(first.IsAccepted, Is.True);
            Assert.That(first.ShotKind, Is.EqualTo(ShotKind.Hit));

            var beforeOwn = Copy(server.GetView(target).OwnCells);
            var beforeEnemy = Copy(server.GetView(shooter).EnemyCells);

            var opponent = server.GetView(shooter).CurrentTurn;
            var passCell = FindMark(server.GetView(shooter).OwnCells, CellMark.Ship);
            Assert.That(server.TryShoot(opponent, passCell, 1).IsAccepted, Is.True);

            var replay = server.TryShoot(shooter, pair.First, 42);
            Assert.That(replay, Is.SameAs(first));
            AssertSameMarks(server.GetView(target).OwnCells, beforeOwn);
            AssertSameMarks(server.GetView(shooter).EnemyCells, beforeEnemy);
            Assert.That(server.GetView(target).OwnCells[pair.Second.X, pair.Second.Y], Is.EqualTo(CellMark.Ship));
        }

        [Test]
        public void TryShoot_SunkShip_RevealsOnlyThatShip()
        {
            var server = Start(11);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;
            var target = Opponent(shooter);
            var isolated = FindIsolatedShip(server.GetView(target).OwnCells);
            var otherShip = FindMarkExcept(server.GetView(target).OwnCells, CellMark.Ship, isolated);

            var response = server.TryShoot(shooter, isolated, 1);

            Assert.That(response.ShotKind, Is.EqualTo(ShotKind.Sunk));
            Assert.That(server.GetView(shooter).EnemyCells[isolated.X, isolated.Y], Is.EqualTo(CellMark.Sunk));
            Assert.That(server.GetView(shooter).EnemyCells[otherShip.X, otherShip.Y], Is.EqualTo(CellMark.Unknown));
        }

        [Test]
        public void TryShoot_SinkingEveryEnemyShip_FinishesTheMatch()
        {
            var server = Start(13);
            var starter = server.GetView(PlayerId.First).CurrentTurn;
            ShotResponse last = null;

            for (var step = 0; step < 40; step++)
            {
                var view = server.GetView(PlayerId.First);
                if (view.Phase == MatchPhase.Finished)
                    break;

                var shooter = view.CurrentTurn;
                var cell = FindMark(server.GetView(Opponent(shooter)).OwnCells, CellMark.Ship);
                last = server.TryShoot(shooter, cell, step + 1);
                Assert.That(last.IsAccepted, Is.True);
            }

            var finished = server.GetView(starter);
            Assert.That(finished.Phase, Is.EqualTo(MatchPhase.Finished));
            Assert.That(finished.Winner, Is.EqualTo(starter));
            Assert.That(last.Winner, Is.EqualTo(starter));
            Assert.That(
                server.TryShoot(starter, new CellCoord(0, 0), 100).RejectReason,
                Is.EqualTo(ShotRejectReason.MatchNotRunning));
        }

        [Test]
        public void TryShoot_OutsideTheBoard_IsRejected()
        {
            var server = Start(15);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;

            var response = server.TryShoot(shooter, new CellCoord(8, 8), 1);

            Assert.That(response.IsAccepted, Is.False);
            Assert.That(response.RejectReason, Is.EqualTo(ShotRejectReason.OutOfBounds));
            Assert.That(server.GetView(shooter).CurrentTurn, Is.EqualTo(shooter));
        }

        [Test]
        public void TickTurn_Expires_PassesTheTurnWithoutAShot()
        {
            var server = Start(3);
            var before = server.GetView(PlayerId.First);
            var shooter = before.CurrentTurn;

            Assert.That(server.TickTurn(19f, true, true), Is.False);
            Assert.That(server.GetView(PlayerId.First).CurrentTurn, Is.EqualTo(shooter));

            Assert.That(server.TickTurn(1.1f, true, true), Is.True);
            var after = server.GetView(shooter);
            Assert.That(after.CurrentTurn, Is.EqualTo(Opponent(shooter)));
            Assert.That(Count(after.EnemyCells, CellMark.Unknown), Is.EqualTo(36));
        }

        [Test]
        public void TickTurn_WhileCurrentPlayerIsDisconnected_DoesNotPass()
        {
            var server = Start(3);
            var shooter = server.GetView(PlayerId.First).CurrentTurn;
            var firstConnected = shooter != PlayerId.First;
            var secondConnected = shooter != PlayerId.Second;

            server.TickTurn(5f, true, true);
            var left = server.GetView(PlayerId.First).TurnSecondsLeft;

            Assert.That(server.TickTurn(10f, firstConnected, secondConnected), Is.False);
            var paused = server.GetView(PlayerId.First);
            Assert.That(paused.CurrentTurn, Is.EqualTo(shooter));
            Assert.That(paused.TurnPaused, Is.True);
            Assert.That(paused.TurnSecondsLeft, Is.EqualTo(left));
        }

        private static MatchServer Start(int seed)
        {
            var server = new MatchServer(new RandomShipPlacer());
            server.Start(new GameRules(6, 6, new[] { 3, 2, 2, 1 }), seed);
            return server;
        }

        private static PlayerId Opponent(PlayerId player) =>
            player == PlayerId.First ? PlayerId.Second : PlayerId.First;

        private static CellCoord FindMark(CellMark[,] cells, CellMark mark)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (cells[x, y] == mark)
                        return new CellCoord(x, y);
                }
            }

            throw new InvalidOperationException($"No cell with {mark}.");
        }

        private static CellCoord FindMarkExcept(CellMark[,] cells, CellMark mark, CellCoord excluded)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if ((x != excluded.X || y != excluded.Y) && cells[x, y] == mark)
                        return new CellCoord(x, y);
                }
            }

            throw new InvalidOperationException($"No other cell with {mark}.");
        }

        private static CellCoord FindIsolatedShip(CellMark[,] cells)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (cells[x, y] != CellMark.Ship || HasOrthogonalShip(cells, x, y))
                        continue;

                    return new CellCoord(x, y);
                }
            }

            throw new InvalidOperationException("No isolated ship.");
        }

        private static bool HasOrthogonalShip(CellMark[,] cells, int x, int y)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            if (x > 0 && cells[x - 1, y] == CellMark.Ship)
                return true;
            if (x + 1 < width && cells[x + 1, y] == CellMark.Ship)
                return true;
            if (y > 0 && cells[x, y - 1] == CellMark.Ship)
                return true;

            return y + 1 < height && cells[x, y + 1] == CellMark.Ship;
        }

        private static (CellCoord First, CellCoord Second) FindAdjacentShip(CellMark[,] cells)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (cells[x, y] != CellMark.Ship)
                        continue;

                    if (x + 1 < width && cells[x + 1, y] == CellMark.Ship)
                        return (new CellCoord(x, y), new CellCoord(x + 1, y));

                    if (y + 1 < height && cells[x, y + 1] == CellMark.Ship)
                        return (new CellCoord(x, y), new CellCoord(x, y + 1));
                }
            }

            throw new InvalidOperationException("No adjacent ship cells.");
        }

        private static int Count(CellMark[,] cells, CellMark mark)
        {
            var count = 0;
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
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

        private static void AssertSameMarks(CellMark[,] left, CellMark[,] right)
        {
            Assert.That(left.GetLength(0), Is.EqualTo(right.GetLength(0)));
            Assert.That(left.GetLength(1), Is.EqualTo(right.GetLength(1)));
            for (var x = 0; x < left.GetLength(0); x++)
            {
                for (var y = 0; y < left.GetLength(1); y++)
                    Assert.That(left[x, y], Is.EqualTo(right[x, y]));
            }
        }

        private class FailingPlacer : IShipPlacer
        {
            public bool TryPlace(Board board, IReadOnlyList<int> shipLengths, Random random) => false;
        }
    }
}
