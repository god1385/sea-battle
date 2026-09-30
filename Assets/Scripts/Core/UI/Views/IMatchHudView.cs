using System;
using UniRx;

namespace SeaBattle.Core.UI.Views
{
    public interface IMatchHudView
    {
        public IObservable<Unit> Restart { get; }

        public IObservable<bool> LogToggle { get; }

        public void ShowLog(string text);
    }
}
