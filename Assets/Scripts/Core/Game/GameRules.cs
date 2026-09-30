using System;
using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class GameRules
    {
        public GameRules(int width, int height, IReadOnlyList<int> shipLengths, int turnSeconds = 20)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (shipLengths.Count == 0)
                throw new ArgumentException("At least one ship is required.", nameof(shipLengths));

            if (turnSeconds < 1)
                throw new ArgumentOutOfRangeException(nameof(turnSeconds));

            for (var i = 0; i < shipLengths.Count; i++)
            {
                if (shipLengths[i] < 1)
                    throw new ArgumentOutOfRangeException(nameof(shipLengths));
            }

            Width = width;
            Height = height;
            var copy = new int[shipLengths.Count];
            for (var i = 0; i < shipLengths.Count; i++)
                copy[i] = shipLengths[i];

            ShipLengths = copy;
            TurnSeconds = turnSeconds;
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<int> ShipLengths { get; }

        public int TurnSeconds { get; }
    }
}
