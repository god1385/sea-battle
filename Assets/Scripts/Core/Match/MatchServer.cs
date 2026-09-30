using System;
using System.Collections.Generic;
using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public class MatchServer : IMatchServer
    {
        private const int MaxPlacementAttempts = 100;

        private readonly IShipPlacer _shipPlacer;
        private readonly Dictionary<(PlayerId Player, int RequestId), ShotResponse> _requests =
            new Dictionary<(PlayerId, int), ShotResponse>();

        private Board _firstBoard;
        private Board _secondBoard;
        private PlayerId _currentTurn;
        private MatchPhase _phase = MatchPhase.NotStarted;
        private PlayerId? _winner;

        public MatchServer(IShipPlacer shipPlacer)
        {
            _shipPlacer = shipPlacer;
        }

        public void Start(GameRules rules, int seed)
        {
            var random = new Random(seed);
            for (var attempt = 0; attempt < MaxPlacementAttempts; attempt++)
            {
                var first = new Board(rules.Width, rules.Height);
                var second = new Board(rules.Width, rules.Height);
                if (!_shipPlacer.TryPlace(first, rules.ShipLengths, random)
                    || !_shipPlacer.TryPlace(second, rules.ShipLengths, random))
                    continue;

                _firstBoard = first;
                _secondBoard = second;
                _currentTurn = random.Next(0, 2) == 0 ? PlayerId.First : PlayerId.Second;
                _phase = MatchPhase.InProgress;
                _winner = null;
                _requests.Clear();
                return;
            }

            throw new InvalidOperationException($"Could not place ships after {MaxPlacementAttempts} attempts.");
        }

        public ShotResponse TryShoot(PlayerId player, CellCoord cell, int requestId)
        {
            var key = (player, requestId);
            if (_requests.TryGetValue(key, out var stored))
                return stored;

            var response = Resolve(player, cell);
            _requests.Add(key, response);
            return response;
        }

        public PlayerView GetView(PlayerId player)
        {
            if (_phase == MatchPhase.NotStarted)
                throw new InvalidOperationException("Match has not started.");

            var own = BoardOf(player);
            var enemy = TargetBoard(player);
            return new PlayerView(
                CopyMarks(own, MarkSide.Own),
                CopyMarks(enemy, MarkSide.Enemy),
                CopyHulls(own, true),
                CopyHulls(enemy, false),
                _currentTurn,
                _phase,
                _winner);
        }

        private ShotResponse Resolve(PlayerId player, CellCoord cell)
        {
            if (_phase != MatchPhase.InProgress)
                return ShotResponse.Rejected(ShotRejectReason.MatchNotRunning, _winner);

            if (player != _currentTurn)
                return ShotResponse.Rejected(ShotRejectReason.NotYourTurn, _winner);

            var target = TargetBoard(player);
            if (!target.Contains(cell))
                return ShotResponse.Rejected(ShotRejectReason.OutOfBounds, _winner);

            if (target.IsShot(cell))
                return ShotResponse.Rejected(ShotRejectReason.AlreadyShot, _winner);

            var resolution = target.ApplyShot(cell);
            if (target.AreAllShipsSunk())
            {
                _phase = MatchPhase.Finished;
                _winner = player;
            }
            else
                _currentTurn = Opponent(player);

            return ShotResponse.Accepted(resolution.Kind, _winner);
        }

        private Board BoardOf(PlayerId player) => player == PlayerId.First ? _firstBoard : _secondBoard;

        private Board TargetBoard(PlayerId shooter) => shooter == PlayerId.First ? _secondBoard : _firstBoard;

        private static PlayerId Opponent(PlayerId player) =>
            player == PlayerId.First ? PlayerId.Second : PlayerId.First;

        private static CellMark[,] CopyMarks(Board board, MarkSide side)
        {
            var marks = new CellMark[board.Width, board.Height];
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    var cell = new CellCoord(x, y);
                    marks[x, y] = side == MarkSide.Own ? board.GetOwnMark(cell) : board.GetEnemyMark(cell);
                }
            }

            return marks;
        }

        private static int[,] CopyHulls(Board board, bool ownSide)
        {
            var hulls = new int[board.Width, board.Height];
            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                    hulls[x, y] = (int)board.GetVisibleHull(new CellCoord(x, y), ownSide);
            }

            return hulls;
        }

        private enum MarkSide
        {
            Own = 0,
            Enemy = 1
        }
    }
}
