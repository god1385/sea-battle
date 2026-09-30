using System;
using UniRx;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class MatchHudView : IMatchHudView
    {
        private readonly Text _log;

        /// <summary>
        /// Binds the restart button, the log toggle, and the log text placed in the scene.
        /// </summary>
        public MatchHudView(MatchScreenRefs screen)
        {
            _log = screen.Log;
            Restart = screen.RestartButton.OnClickAsObservable();
            LogToggle = screen.LogToggle.OnValueChangedAsObservable();
        }

        public IObservable<Unit> Restart { get; }

        public IObservable<bool> LogToggle { get; }

        /// <summary>
        /// Replaces the traffic log text.
        /// </summary>
        public void ShowLog(string text) => _log.text = text;
    }
}
