using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        private Image _paper;
        private Image _ship;
        private Image _mark;
        private Text _label;
        private Button _button;
        private bool _ready;

        public int X => x;

        public int Y => y;

        public Button Button
        {
            get
            {
                Ensure();
                return _button;
            }
        }

        /// <summary>
        /// Stores the grid coordinate. The scene builder calls this while placing the cell.
        /// </summary>
        public void SetCoord(int cellX, int cellY)
        {
            x = cellX;
            y = cellY;
        }

        /// <summary>
        /// Draws the paper, the hull piece, and the shot mark for this cell.
        /// </summary>
        public void Paint(CellMark[,] cells, ClientBoardState state, bool enemy, BoardArt art)
        {
            Ensure();
            var mark = cells[x, y];
            var pending = enemy && state.IsWaiting && x == state.PendingX && y == state.PendingY;
            _paper.sprite = art.Cell;
            _paper.color = pending ? new Color(1f, 0.9f, 0.55f) : Color.white;
            _label.text = pending ? "..." : string.Empty;
            var hulls = enemy ? state.EnemyHulls : state.OwnHulls;
            PaintShip(cells, mark, !enemy, art, hulls[x, y]);
            PaintMark(mark, art);
            _button.interactable = enemy && state.CanShoot(mark);
        }

        private void PaintShip(CellMark[,] cells, CellMark mark, bool ownBoard, BoardArt art, int hull)
        {
            if (!art.TryGetShip(cells, x, y, ownBoard, hull, out var sprite, out var rotation))
            {
                Hide(_ship);
                return;
            }

            var tint = mark == CellMark.Sunk ? new Color(0.62f, 0.66f, 0.72f) : Color.white;
            Show(_ship, sprite, rotation, tint);
        }

        private void PaintMark(CellMark mark, BoardArt art)
        {
            switch (mark)
            {
                case CellMark.Miss:
                    Show(_mark, art.Miss, 0f, Color.white);
                    break;
                case CellMark.Hit:
                case CellMark.Sunk:
                    Show(_mark, art.Hit, 0f, Color.white);
                    break;
                default:
                    Hide(_mark);
                    break;
            }
        }

        private static void Show(Image image, Sprite sprite, float rotation, Color color)
        {
            image.sprite = sprite;
            image.enabled = true;
            image.color = color;
            image.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }

        private static void Hide(Image image) => image.enabled = false;

        private void Ensure()
        {
            if (_ready)
                return;

            _ready = true;
            _paper = GetComponent<Image>();
            _button = GetComponent<Button>();
            _ship = transform.Find("Ship").GetComponent<Image>();
            _mark = transform.Find("Mark").GetComponent<Image>();
            _label = GetComponentInChildren<Text>();
        }
    }
}
