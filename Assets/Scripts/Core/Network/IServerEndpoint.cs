using System;
using UniRx;

namespace SeaBattle.Core.Network
{
    public interface IServerEndpoint
    {
        public void Send(string json);

        public IObservable<IncomingMessage> Incoming { get; }
    }
}
