using SeaBattle.Core.Game;
using UnityEngine;

namespace SeaBattle.Core.UI
{
    public class BoardArt
    {
        /// <summary>
        /// Loads the notebook cell, hull pieces, and shot marks.
        /// </summary>
        public BoardArt()
        {
            Cell = Resources.Load<Sprite>("Sprites/cell_paper");
            Single = Resources.Load<Sprite>("Sprites/ship_1");
            Bow = Resources.Load<Sprite>("Sprites/ship_bow");
            Mid = Resources.Load<Sprite>("Sprites/ship_mid");
            Stern = Resources.Load<Sprite>("Sprites/ship_stern");
            Hit = Resources.Load<Sprite>("Sprites/mark_hit");
            Miss = Resources.Load<Sprite>("Sprites/mark_miss");
        }

        public Sprite Cell { get; }

        public Sprite Single { get; }

        public Sprite Bow { get; }

        public Sprite Mid { get; }

        public Sprite Stern { get; }

        public Sprite Hit { get; }

        public Sprite Miss { get; }

        /// <summary>
        /// Picks the hull piece for one cell. Bow points right, and vertical ships are rotated.
        /// </summary>
        public bool TryGetShip(CellMark[,] cells, int x, int y, bool ownBoard, out Sprite sprite, out float rotation)
        {
            sprite = Single;
            rotation = 0f;
            if (!IsHull(cells[x, y], ownBoard))
                return false;

            var left = IsHull(At(cells, x - 1, y), ownBoard);
            var right = IsHull(At(cells, x + 1, y), ownBoard);
            var up = IsHull(At(cells, x, y - 1), ownBoard);
            var down = IsHull(At(cells, x, y + 1), ownBoard);

            if (!left && !right && !up && !down)
                return true;

            if (left || right)
            {
                sprite = Piece(left, right);
                return true;
            }

            sprite = Piece(down, up);
            rotation = 90f;
            return true;
        }

        /// <summary>
        /// hasBackward is the neighbor on the flat side. The bow points away from that neighbor.
        /// </summary>
        private Sprite Piece(bool hasBackward, bool hasForward)
        {
            if (hasBackward && hasForward)
                return Mid;

            if (hasBackward)
                return Bow;

            return Stern;
        }

        private static bool IsHull(CellMark mark, bool ownBoard)
        {
            if (mark == CellMark.Sunk)
                return true;

            return ownBoard && (mark == CellMark.Ship || mark == CellMark.Hit);
        }

        private static CellMark At(CellMark[,] cells, int x, int y)
        {
            if (x < 0 || y < 0 || x >= cells.GetLength(0) || y >= cells.GetLength(1))
                return CellMark.Empty;

            return cells[x, y];
        }
    }
}
