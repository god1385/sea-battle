using System;
using UniRx;

namespace SeaBattle.Core.Network
{
    public interface IClientChannel
    {
        public bool IsConnected { get; }

        public int DeliveryDelayMilliseconds { get; set; }

        public void Connect();

        public void Disconnect();

        public void Send(string json);

        public IObservable<IncomingMessage> Incoming { get; }

        public IObservable<bool> Connection { get; }
    }
}
