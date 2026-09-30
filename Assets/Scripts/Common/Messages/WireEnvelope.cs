using System;

namespace SeaBattle.Common.Messages
{
    [Serializable]
    public class WireEnvelope
    {
        public string Type;
        public string Payload;
    }
}
