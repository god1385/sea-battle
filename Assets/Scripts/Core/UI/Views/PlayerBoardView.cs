using System;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class PlayerBoardView : IPlayerBoardView, IDisposable
    {
        private readonly Subject<CellCoord> _shots = new Subject<CellCoord>();
        private readonly CompositeDisposable _enemyClicks = new CompositeDisposable();
        private readonly BoardArt _art;
        private readonly Text _status;
        private readonly RectTransform _ownHost;
        private readonly RectTransform _enemyHost;
        private readonly Button _disconnect;
        private readonly Button _connect;
        private readonly InputField _delay;
        private BoardCell[,] _own = new BoardCell[0, 0];
        private BoardCell[,] _enemy = new BoardCell[0, 0];

        /// <summary>
        /// Builds one player's column: own board, enemy board, and the connection controls.
        /// </summary>
        public PlayerBoardView(RectTransform parent, string title, int delayMs, BoardArt art)
        {
            _art = art;
            var column = UiFactory.CreatePanel(parent, title, new Color(0.07f, 0.1f, 0.16f, 0.94f));
            var columnLayout = column.gameObject.AddComponent<LayoutElement>();
            columnLayout.flexibleWidth = 1f;
            var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLabel(column, title, 22, 32);
            _status = AddLabel(column, "Ожидание партии", 18, 36);
            AddLabel(column, "Ваше поле", 16, 24);
            _ownHost = AddHost(column);
            AddLabel(column, "Поле соперника", 16, 24);
            _enemyHost = AddHost(column);

            var controls = AddHost(column);
            var row = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 8;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;
            SetHeight(controls, 40);
            _delay = UiFactory.CreateIntegerField(controls, delayMs.ToString());
            _disconnect = UiFactory.CreateButton(controls, "Разорвать");
            _connect = UiFactory.CreateButton(controls, "Подключить");
        }

        public IObservable<CellCoord> EnemyShot => _shots;

        public IObservable<Unit> Disconnect => _disconnect.OnClickAsObservable();

        public IObservable<Unit> Connect => _connect.OnClickAsObservable();

        public IObservable<string> DelaySubmitted => _delay.OnEndEditAsObservable();

        /// <summary>
        /// Paints the column from the latest client state.
        /// </summary>
        public void Render(ClientBoardState state)
        {
            _status.text = state.Status;
            if (!state.HasSnapshot)
                return;

            Ensure(_ownHost, ref _own, state.Width, state.Height, false);
            Ensure(_enemyHost, ref _enemy, state.Width, state.Height, true);
            Paint(_own, state.OwnCells, state, false);
            Paint(_enemy, state.EnemyCells, state, true);
        }

        /// <summary>
        /// Closes the shot stream so a reloaded scene cannot receive old clicks.
        /// </summary>
        public void Dispose()
        {
            _enemyClicks.Dispose();
            _shots.Dispose();
        }

        private void Ensure(RectTransform host, ref BoardCell[,] cells, int width, int height, bool enemy)
        {
            if (cells.GetLength(0) == width && cells.GetLength(1) == height)
                return;

            if (enemy)
                _enemyClicks.Clear();

            for (var i = host.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(host.GetChild(i).gameObject);

            cells = new BoardCell[width, height];
            SetHeight(host, height * 42f);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    cells[x, y] = new BoardCell(host, x, y, enemy, _art, _shots, _enemyClicks);
            }
        }

        private static void Paint(BoardCell[,] cells, CellMark[,] marks, ClientBoardState state, bool enemy)
        {
            var width = marks.GetLength(0);
            var height = marks.GetLength(1);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    cells[x, y].Paint(marks, state, enemy);
            }
        }

        private static Text AddLabel(Transform parent, string content, int size, float height)
        {
            var host = AddHost(parent);
            SetHeight(host, height);
            return UiFactory.CreateText(host, content, size, TextAnchor.MiddleCenter);
        }

        private static RectTransform AddHost(Transform parent)
        {
            var host = UiFactory.CreateRect(parent, "Host");
            var element = host.gameObject.AddComponent<LayoutElement>();
            element.flexibleWidth = 1;
            return host;
        }

        private static void SetHeight(RectTransform host, float height)
        {
            var element = host.GetComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
        }
    }
}
