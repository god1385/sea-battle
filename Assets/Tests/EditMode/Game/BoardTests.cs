using NUnit.Framework;
using SeaBattle.Core.Game;

namespace SeaBattle.Tests.Game
{
    public class BoardTests
    {
        [Test]
        public void TryPlaceShip_RejectsTouchAndBentShapes()
        {
            var board = new Board(6, 6);

            Assert.That(board.TryPlaceShip(new[] { new CellCoord(0, 0) }), Is.True);
            Assert.That(board.TryPlaceShip(new[] { new CellCoord(0, 1) }), Is.False);
            Assert.That(board.TryPlaceShip(new[] { new CellCoord(1, 1) }), Is.False);
            Assert.That(board.TryPlaceShip(new[] { new CellCoord(2, 0) }), Is.True);
            Assert.That(board.TryPlaceShip(new[] { new CellCoord(4, 4), new CellCoord(5, 5) }), Is.False);
        }

        [Test]
        public void ApplyShot_ReportsMissHitAndSunk()
        {
            var board = new Board(6, 6);
            Assert.That(board.TryPlaceShip(new[] { new CellCoord(0, 0), new CellCoord(1, 0) }), Is.True);

            var miss = board.ApplyShot(new CellCoord(3, 3));
            Assert.That(miss.Kind, Is.EqualTo(ShotKind.Miss));
            Assert.That(board.GetOwnMark(new CellCoord(3, 3)), Is.EqualTo(CellMark.Miss));
            Assert.That(board.GetEnemyMark(new CellCoord(3, 3)), Is.EqualTo(CellMark.Miss));

            var hit = board.ApplyShot(new CellCoord(0, 0));
            Assert.That(hit.Kind, Is.EqualTo(ShotKind.Hit));
            Assert.That(board.GetOwnMark(new CellCoord(0, 0)), Is.EqualTo(CellMark.Hit));
            Assert.That(board.GetOwnMark(new CellCoord(1, 0)), Is.EqualTo(CellMark.Ship));
            Assert.That(board.GetEnemyMark(new CellCoord(1, 0)), Is.EqualTo(CellMark.Unknown));

            var sunk = board.ApplyShot(new CellCoord(1, 0));
            Assert.That(sunk.Kind, Is.EqualTo(ShotKind.Sunk));
            Assert.That(sunk.Cells, Has.Count.EqualTo(2));
            Assert.That(board.GetOwnMark(new CellCoord(0, 0)), Is.EqualTo(CellMark.Sunk));
            Assert.That(board.GetOwnMark(new CellCoord(1, 0)), Is.EqualTo(CellMark.Sunk));
            Assert.That(board.GetEnemyMark(new CellCoord(0, 0)), Is.EqualTo(CellMark.Sunk));
            Assert.That(board.GetEnemyMark(new CellCoord(1, 0)), Is.EqualTo(CellMark.Sunk));
            Assert.That(board.AreAllShipsSunk(), Is.True);
        }
    }
}
