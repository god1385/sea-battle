namespace SeaBattle.Core.Match
{
    public enum ShotRejectReason
    {
        MatchNotRunning = 0,
        NotYourTurn = 1,
        AlreadyShot = 2,
        OutOfBounds = 3
    }
}
