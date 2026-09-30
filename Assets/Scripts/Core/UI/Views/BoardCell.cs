using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class BoardCell
    {
        private readonly Image _paper;
        private readonly Image _ship;
        private readonly Image _mark;
        private readonly Text _label;
        private readonly Button _button;
        private readonly BoardArt _art;
        private readonly int _x;
        private readonly int _y;

        /// <summary>
        /// Creates one notebook cell. Enemy cells push clicks into the shared shot stream.
        /// </summary>
        public BoardCell(
            RectTransform host,
            int x,
            int y,
            bool enemy,
            BoardArt art,
            Subject<CellCoord> shots,
            CompositeDisposable clicks)
        {
            _art = art;
            _x = x;
            _y = y;
            var button = UiFactory.CreateButton(host, string.Empty);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(42f, 42f);
            rect.anchoredPosition = new Vector2(x * 42f, -y * 42f);
            button.transition = Selectable.Transition.None;
            var element = button.gameObject.AddComponent<LayoutElement>();
            element.ignoreLayout = true;
            _paper = button.GetComponent<Image>();
            _button = button;
            _ship = CreateOverlay(button.transform, "Ship");
            _mark = CreateOverlay(button.transform, "Mark");
            _label = button.GetComponentInChildren<Text>();
            _label.color = new Color(0.2f, 0.28f, 0.38f);
            if (enemy)
                button.OnClickAsObservable().Subscribe(_ => shots.OnNext(new CellCoord(_x, _y))).AddTo(clicks);
        }

        /// <summary>
        /// Draws the paper, the hull piece, and the shot mark for this cell.
        /// </summary>
        public void Paint(CellMark[,] cells, ClientBoardState state, bool enemy)
        {
            var mark = cells[_x, _y];
            var pending = enemy && state.IsWaiting && _x == state.PendingX && _y == state.PendingY;
            _paper.sprite = _art.Cell;
            _paper.color = pending ? new Color(1f, 0.9f, 0.55f) : Color.white;
            _label.text = pending ? "..." : string.Empty;
            PaintShip(cells, mark, !enemy);
            PaintMark(mark);
            _button.interactable = enemy && state.CanShoot(mark);
        }

        private void PaintShip(CellMark[,] cells, CellMark mark, bool ownBoard)
        {
            if (!_art.TryGetShip(cells, _x, _y, ownBoard, out var sprite, out var rotation))
            {
                Hide(_ship);
                return;
            }

            var tint = mark == CellMark.Sunk ? new Color(0.62f, 0.66f, 0.72f) : Color.white;
            Show(_ship, sprite, rotation, tint);
        }

        private void PaintMark(CellMark mark)
        {
            switch (mark)
            {
                case CellMark.Miss:
                    Show(_mark, _art.Miss, 0f, Color.white);
                    break;
                case CellMark.Hit:
                case CellMark.Sunk:
                    Show(_mark, _art.Hit, 0f, Color.white);
                    break;
                default:
                    Hide(_mark);
                    break;
            }
        }

        private static Image CreateOverlay(Transform parent, string name)
        {
            var rect = UiFactory.CreateRect(parent, name);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(42f, 42f);
            rect.anchoredPosition = Vector2.zero;
            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }

        private static void Show(Image image, Sprite sprite, float rotation, Color color)
        {
            image.sprite = sprite;
            image.enabled = true;
            image.color = color;
            image.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        }

        private static void Hide(Image image) => image.enabled = false;
    }
}
