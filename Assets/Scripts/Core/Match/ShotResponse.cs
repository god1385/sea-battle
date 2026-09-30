using SeaBattle.Core.Game;

namespace SeaBattle.Core.Match
{
    public class ShotResponse
    {
        private ShotResponse(bool isAccepted, ShotRejectReason? rejectReason, ShotKind shotKind, PlayerId? winner)
        {
            IsAccepted = isAccepted;
            RejectReason = rejectReason;
            ShotKind = shotKind;
            Winner = winner;
        }

        public bool IsAccepted { get; }

        public ShotRejectReason? RejectReason { get; }

        public ShotKind ShotKind { get; }

        public PlayerId? Winner { get; }

        public static ShotResponse Rejected(ShotRejectReason reason, PlayerId? winner) =>
            new ShotResponse(false, reason, default, winner);

        public static ShotResponse Accepted(ShotKind shotKind, PlayerId? winner) =>
            new ShotResponse(true, null, shotKind, winner);
    }
}
