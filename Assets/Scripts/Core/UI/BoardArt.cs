using SeaBattle.Core.Game;
using UnityEngine;

namespace SeaBattle.Core.UI
{
    public class BoardArt
    {
        private readonly Sprite _single;
        private readonly Sprite _bow;
        private readonly Sprite _mid;
        private readonly Sprite _stern;
        private readonly Sprite _subBow;
        private readonly Sprite _subStern;
        private readonly Sprite _gunBow;
        private readonly Sprite _gunStern;

        public BoardArt()
        {
            Cell = Resources.Load<Sprite>("Sprites/cell_paper");
            _single = Resources.Load<Sprite>("Sprites/ship_1");
            _bow = Resources.Load<Sprite>("Sprites/ship_bow");
            _mid = Resources.Load<Sprite>("Sprites/ship_mid");
            _stern = Resources.Load<Sprite>("Sprites/ship_stern");
            _subBow = Resources.Load<Sprite>("Sprites/sub_bow");
            _subStern = Resources.Load<Sprite>("Sprites/sub_stern");
            _gunBow = Resources.Load<Sprite>("Sprites/gun_bow");
            _gunStern = Resources.Load<Sprite>("Sprites/gun_stern");
            Hit = Resources.Load<Sprite>("Sprites/mark_hit");
            Miss = Resources.Load<Sprite>("Sprites/mark_miss");
        }

        public Sprite Cell { get; }

        public Sprite Hit { get; }

        public Sprite Miss { get; }

        public bool TryGetShip(CellMark[,] cells, int x, int y, bool ownBoard, int hull, out Sprite sprite, out float rotation)
        {
            sprite = _single;
            rotation = 0f;
            if (!IsHull(cells[x, y], ownBoard))
                return false;

            var left = IsHull(At(cells, x - 1, y), ownBoard);
            var right = IsHull(At(cells, x + 1, y), ownBoard);
            var up = IsHull(At(cells, x, y - 1), ownBoard);
            var down = IsHull(At(cells, x, y + 1), ownBoard);
            if (!left && !right && !up && !down)
                return true;

            var kind = (HullKind)hull;
            if (left || right)
            {
                sprite = Piece(left, right, kind);
                return true;
            }

            sprite = Piece(down, up, kind);
            rotation = 90f;
            return true;
        }

        private Sprite Piece(bool hasBackward, bool hasForward, HullKind kind)
        {
            if (hasBackward && hasForward)
                return _mid;

            if (kind == HullKind.Submarine)
                return hasBackward ? _subBow : _subStern;

            if (kind == HullKind.Gunboat)
                return hasBackward ? _gunBow : _gunStern;

            return hasBackward ? _bow : _stern;
        }

        private static bool IsHull(CellMark mark, bool ownBoard) =>
            mark == CellMark.Sunk || (ownBoard && (mark == CellMark.Ship || mark == CellMark.Hit));

        private static CellMark At(CellMark[,] cells, int x, int y) =>
            x < 0 || y < 0 || x >= cells.GetLength(0) || y >= cells.GetLength(1)
                ? CellMark.Empty
                : cells[x, y];
    }
}
