using System;
using SeaBattle.Core.Client;
using SeaBattle.Core.Game;
using UniRx;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class PlayerBoardView : IPlayerBoardView, IDisposable
    {
        private readonly Subject<CellCoord> _shots = new Subject<CellCoord>();
        private readonly CompositeDisposable _clicks = new CompositeDisposable();
        private readonly BoardArt _art;
        private readonly Text _status;
        private readonly CellView[,] _own;
        private readonly CellView[,] _enemy;

        /// <summary>
        /// Binds one player column that is already placed in the scene.
        /// </summary>
        public PlayerBoardView(PlayerColumnRefs column, BoardArt art)
        {
            _art = art;
            _status = column.Status;
            Disconnect = column.Disconnect.OnClickAsObservable();
            Connect = column.Connect.OnClickAsObservable();
            DelaySubmitted = column.Delay.OnEndEditAsObservable();
            _own = Index(column.OwnCells);
            _enemy = Index(column.EnemyCells);
            var enemy = column.EnemyCells;
            for (var i = 0; i < enemy.Length; i++)
            {
                var cell = enemy[i];
                cell.Button.OnClickAsObservable()
                    .Subscribe(_ => _shots.OnNext(new CellCoord(cell.X, cell.Y)))
                    .AddTo(_clicks);
            }
        }

        public IObservable<CellCoord> EnemyShot => _shots;

        public IObservable<Unit> Disconnect { get; }

        public IObservable<Unit> Connect { get; }

        public IObservable<string> DelaySubmitted { get; }

        /// <summary>
        /// Paints the placed grids from the latest client state.
        /// </summary>
        public void Render(ClientBoardState state)
        {
            _status.text = state.Status;
            if (!state.HasSnapshot)
                return;

            Paint(_own, state.OwnCells, state, false);
            Paint(_enemy, state.EnemyCells, state, true);
        }

        /// <summary>
        /// Closes the shot stream so a reloaded scene cannot receive old clicks.
        /// </summary>
        public void Dispose()
        {
            _clicks.Dispose();
            _shots.Dispose();
        }

        private static CellView[,] Index(CellView[] cells)
        {
            var width = 0;
            var height = 0;
            for (var i = 0; i < cells.Length; i++)
            {
                width = Math.Max(width, cells[i].X + 1);
                height = Math.Max(height, cells[i].Y + 1);
            }

            var grid = new CellView[width, height];
            for (var i = 0; i < cells.Length; i++)
                grid[cells[i].X, cells[i].Y] = cells[i];

            return grid;
        }

        private void Paint(CellView[,] grid, CellMark[,] marks, ClientBoardState state, bool enemy)
        {
            var width = Math.Min(grid.GetLength(0), marks.GetLength(0));
            var height = Math.Min(grid.GetLength(1), marks.GetLength(1));
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    grid[x, y].Paint(marks, state, enemy, _art);
            }
        }
    }
}
