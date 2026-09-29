using System;
using System.Collections.Generic;
using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public class ShotResponse
    {
        private ShotResponse(
            bool isAccepted,
            ShotRejectReason? rejectReason,
            ShotKind shotKind,
            CellCoord cell,
            IReadOnlyList<CellCoord> cells,
            PlayerId currentTurn,
            PlayerId? winner)
        {
            IsAccepted = isAccepted;
            RejectReason = rejectReason;
            ShotKind = shotKind;
            Cell = cell;
            Cells = cells;
            CurrentTurn = currentTurn;
            Winner = winner;
        }

        public bool IsAccepted { get; }

        public ShotRejectReason? RejectReason { get; }

        public ShotKind ShotKind { get; }

        public CellCoord Cell { get; }

        public IReadOnlyList<CellCoord> Cells { get; }

        public PlayerId CurrentTurn { get; }

        public PlayerId? Winner { get; }

        /// <summary>
        /// Builds a rejected shot. The board is left unchanged.
        /// </summary>
        public static ShotResponse Rejected(ShotRejectReason reason, PlayerId currentTurn, PlayerId? winner) =>
            new ShotResponse(false, reason, default, default, Array.Empty<CellCoord>(), currentTurn, winner);

        /// <summary>
        /// Builds an accepted shot. CurrentTurn is the player who moves next, or the winner when the match is over.
        /// </summary>
        public static ShotResponse Accepted(
            ShotKind shotKind,
            CellCoord cell,
            IReadOnlyList<CellCoord> cells,
            PlayerId currentTurn,
            PlayerId? winner) =>
            new ShotResponse(true, null, shotKind, cell, cells, currentTurn, winner);
    }
}
