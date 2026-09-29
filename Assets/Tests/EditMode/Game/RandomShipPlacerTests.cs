using System;
using NUnit.Framework;
using SeaBattle.Core.Game;

namespace SeaBattle.Tests.Game
{
    public class RandomShipPlacerTests
    {
        [Test]
        public void TryPlace_FitsDefaultFleetWithoutTouching()
        {
            var placer = new RandomShipPlacer();
            var lengths = new[] { 3, 2, 2, 1 };

            for (var seed = 0; seed < 30; seed++)
            {
                var board = new Board(6, 6);
                var placed = placer.TryPlace(board, lengths, new Random(seed));

                Assert.That(placed, Is.True, $"seed {seed}");
                Assert.That(ShipLengths(board), Is.EquivalentTo(lengths));
                AssertShipsDoNotTouch(board);
            }
        }

        private static int[] ShipLengths(Board board)
        {
            var ships = board.Ships;
            var lengths = new int[ships.Count];
            for (var i = 0; i < ships.Count; i++)
                lengths[i] = ships[i].Length;

            return lengths;
        }

        private static void AssertShipsDoNotTouch(Board board)
        {
            var ships = board.Ships;
            for (var i = 0; i < ships.Count; i++)
            {
                for (var j = i + 1; j < ships.Count; j++)
                {
                    var left = ships[i].Cells;
                    var right = ships[j].Cells;
                    for (var a = 0; a < left.Count; a++)
                    {
                        for (var b = 0; b < right.Count; b++)
                        {
                            var distance = Math.Max(
                                Math.Abs(left[a].X - right[b].X),
                                Math.Abs(left[a].Y - right[b].Y));
                            Assert.That(distance, Is.GreaterThanOrEqualTo(2));
                        }
                    }
                }
            }
        }
    }
}
