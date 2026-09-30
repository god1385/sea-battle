using System;

namespace SeaBattle.Common.Messages
{
    [Serializable]
    public class ShootMessage
    {
        public int X;
        public int Y;
        public int RequestId;
    }
}
