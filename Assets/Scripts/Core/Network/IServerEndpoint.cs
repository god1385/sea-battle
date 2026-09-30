using System;
using UniRx;

namespace SeaBattle.Core.Network
{
    public interface IServerEndpoint
    {
        /// <summary>
        /// Queues a serialized message for the client. A disconnected client never receives it.
        /// </summary>
        public void Send(string json);

        public IObservable<IncomingMessage> Incoming { get; }
    }
}
