using System;

namespace SeaBattle.Common.Messages
{
    [Serializable]
    public class ShotAckMessage
    {
        public int RequestId;
        public bool Accepted;
        public int RejectReason;
    }
}
