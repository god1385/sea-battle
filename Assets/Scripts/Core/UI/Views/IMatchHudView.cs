using System;
using UniRx;
using UnityEngine;

namespace SeaBattle.Core.UI.Views
{
    public interface IMatchHudView
    {
        /// <summary>
        /// Fires when the shared restart button is pressed.
        /// </summary>
        public IObservable<Unit> Restart { get; }

        /// <summary>
        /// Fires with the log toggle, including its current value on subscribe.
        /// </summary>
        public IObservable<bool> LogToggle { get; }

        /// <summary>
        /// Replaces the traffic log text.
        /// </summary>
        public void ShowLog(string text);
    }
}
