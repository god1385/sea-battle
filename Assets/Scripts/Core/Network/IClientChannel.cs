using System;
using UniRx;

namespace SeaBattle.Core.Network
{
    public interface IClientChannel
    {
        public bool IsConnected { get; }

        public int DeliveryDelayMilliseconds { get; set; }

        /// <summary>
        /// Opens the channel. Messages lost while it was closed stay lost.
        /// </summary>
        public void Connect();

        /// <summary>
        /// Drops every message still in flight and tells the client the link is down.
        /// </summary>
        public void Disconnect();

        /// <summary>
        /// Queues one serialized message. It is ignored while the channel is closed.
        /// </summary>
        public void Send(string json);

        public IObservable<IncomingMessage> Incoming { get; }

        public IObservable<bool> Connection { get; }
    }
}
