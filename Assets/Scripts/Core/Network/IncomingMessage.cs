namespace SeaBattle.Core.Network
{
    public readonly struct IncomingMessage
    {
        public IncomingMessage(string type, string payload)
        {
            Type = type;
            Payload = payload;
        }

        public string Type { get; }

        public string Payload { get; }
    }
}
