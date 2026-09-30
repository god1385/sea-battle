using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace SeaBattle.Core.UI.Views
{
    public class MatchHudView : IMatchHudView
    {
        private readonly Text _log;
        private readonly ScrollRect _scroll;

        public MatchHudView(MatchScreenRefs screen)
        {
            _log = screen.Log;
            _scroll = _log.GetComponentInParent<ScrollRect>();
            Restart = screen.RestartButton.OnClickAsObservable();
            LogToggle = screen.LogToggle.OnValueChangedAsObservable();
        }

        public IObservable<Unit> Restart { get; }

        public IObservable<bool> LogToggle { get; }

        public void ShowLog(string text)
        {
            _log.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_log.rectTransform);
            _scroll.verticalNormalizedPosition = 0f;
        }
    }
}
