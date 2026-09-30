using System;
using System.Collections.Generic;

namespace SeaBattle.Core.Game
{
    public class Board
    {
        private readonly int _width;
        private readonly int _height;
        private readonly CellData[,] _cells;
        private readonly List<Ship> _ships = new List<Ship>();

        public Board(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new CellData[width, height];
        }

        public int Width => _width;

        public int Height => _height;

        public IReadOnlyList<Ship> Ships => _ships;

        public bool Contains(CellCoord cell) =>
            cell.X >= 0 && cell.Y >= 0 && cell.X < _width && cell.Y < _height;

        public bool TryPlaceShip(IReadOnlyList<CellCoord> cells)
        {
            if (!CanPlace(cells))
                return false;

            var ship = new Ship(cells, KindFor(cells.Count));
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                _cells[cell.X, cell.Y] = new CellData(CellKind.Ship, ship);
            }

            _ships.Add(ship);
            return true;
        }

        public ShotResolution ApplyShot(CellCoord cell)
        {
            EnsureInside(cell);
            var data = _cells[cell.X, cell.Y];
            if (data.Kind != CellKind.Empty && data.Kind != CellKind.Ship)
                throw new InvalidOperationException("Cell was already shot.");

            if (data.Kind == CellKind.Empty)
            {
                _cells[cell.X, cell.Y] = new CellData(CellKind.Miss, null);
                return new ShotResolution(ShotKind.Miss, new[] { cell });
            }

            data.Ship.RegisterHit();
            if (!data.Ship.IsSunk)
            {
                _cells[cell.X, cell.Y] = new CellData(CellKind.Hit, data.Ship);
                return new ShotResolution(ShotKind.Hit, new[] { cell });
            }

            MarkSunk(data.Ship);
            return new ShotResolution(ShotKind.Sunk, data.Ship.Cells);
        }

        public bool IsShot(CellCoord cell)
        {
            EnsureInside(cell);
            var kind = _cells[cell.X, cell.Y].Kind;
            return kind == CellKind.Miss || kind == CellKind.Hit || kind == CellKind.Sunk;
        }

        public bool AreAllShipsSunk()
        {
            if (_ships.Count == 0)
                return false;

            for (var i = 0; i < _ships.Count; i++)
            {
                if (!_ships[i].IsSunk)
                    return false;
            }

            return true;
        }

        public CellMark GetOwnMark(CellCoord cell)
        {
            EnsureInside(cell);
            return ToOwnMark(_cells[cell.X, cell.Y].Kind);
        }

        public HullKind GetVisibleHull(CellCoord cell, bool ownSide)
        {
            EnsureInside(cell);
            var data = _cells[cell.X, cell.Y];
            var hullVisible = data.Kind == CellKind.Ship || data.Kind == CellKind.Hit || data.Kind == CellKind.Sunk;
            if (!hullVisible || (!ownSide && data.Kind != CellKind.Sunk))
                return HullKind.None;

            return data.Ship.Hull;
        }

        public CellMark GetEnemyMark(CellCoord cell)
        {
            EnsureInside(cell);
            return ToEnemyMark(_cells[cell.X, cell.Y].Kind);
        }

        private static bool IsContiguousStraight(IReadOnlyList<CellCoord> cells)
        {
            if (cells.Count == 0)
                return false;

            var minX = cells[0].X;
            var maxX = cells[0].X;
            var minY = cells[0].Y;
            var maxY = cells[0].Y;
            for (var i = 1; i < cells.Count; i++)
            {
                minX = Math.Min(minX, cells[i].X);
                maxX = Math.Max(maxX, cells[i].X);
                minY = Math.Min(minY, cells[i].Y);
                maxY = Math.Max(maxY, cells[i].Y);
            }

            var horizontal = minY == maxY && maxX - minX + 1 == cells.Count;
            var vertical = minX == maxX && maxY - minY + 1 == cells.Count;
            if (!horizontal && !vertical)
                return false;

            for (var i = 0; i < cells.Count; i++)
            {
                for (var j = i + 1; j < cells.Count; j++)
                {
                    if (cells[i].X == cells[j].X && cells[i].Y == cells[j].Y)
                        return false;
                }
            }

            return true;
        }

        private bool CanPlace(IReadOnlyList<CellCoord> cells)
        {
            if (!IsContiguousStraight(cells))
                return false;

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (!Contains(cell) || _cells[cell.X, cell.Y].Kind != CellKind.Empty)
                    return false;

                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        var neighbor = new CellCoord(cell.X + dx, cell.Y + dy);
                        if (!Contains(neighbor) || ContainsCell(cells, neighbor))
                            continue;

                        if (_cells[neighbor.X, neighbor.Y].Kind == CellKind.Ship)
                            return false;
                    }
                }
            }

            return true;
        }

        private HullKind KindFor(int length)
        {
            if (length <= 1)
                return HullKind.Boat;

            if (length >= 3)
                return HullKind.Battleship;

            for (var i = 0; i < _ships.Count; i++)
            {
                if (_ships[i].Length == 2)
                    return HullKind.Gunboat;
            }

            return HullKind.Submarine;
        }

        private static bool ContainsCell(IReadOnlyList<CellCoord> cells, CellCoord candidate)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].X == candidate.X && cells[i].Y == candidate.Y)
                    return true;
            }

            return false;
        }

        private void MarkSunk(Ship ship)
        {
            var cells = ship.Cells;
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                _cells[cell.X, cell.Y] = new CellData(CellKind.Sunk, ship);
            }
        }

        private void EnsureInside(CellCoord cell)
        {
            if (!Contains(cell))
                throw new ArgumentOutOfRangeException(nameof(cell));
        }

        private static CellMark ToOwnMark(CellKind kind) => kind switch
        {
            CellKind.Empty => CellMark.Empty,
            CellKind.Ship => CellMark.Ship,
            CellKind.Miss => CellMark.Miss,
            CellKind.Hit => CellMark.Hit,
            CellKind.Sunk => CellMark.Sunk,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        private static CellMark ToEnemyMark(CellKind kind) => kind switch
        {
            CellKind.Empty => CellMark.Unknown,
            CellKind.Ship => CellMark.Unknown,
            CellKind.Miss => CellMark.Miss,
            CellKind.Hit => CellMark.Hit,
            CellKind.Sunk => CellMark.Sunk,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        private enum CellKind
        {
            Empty = 0,
            Ship = 1,
            Miss = 2,
            Hit = 3,
            Sunk = 4
        }

        private readonly struct CellData
        {
            public CellData(CellKind kind, Ship ship)
            {
                Kind = kind;
                Ship = ship;
            }

            public CellKind Kind { get; }

            public Ship Ship { get; }
        }
    }
}
